using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace IngameScript
{
    class RoomController
    {
        private const int SECONDS_BETWEEN_REDISCOVERY = 10;
        private const int MILLISECONDS_BETWEEN_SENSORSCANS= 200;
        private const bool DEBUG_GROUPS = true;
        private const bool DEBUG_DOORS = false;
        private const bool DEBUG_DOOR_DISCOVERY = false;
        private const bool DEBUG_DISCOVERY = false;

        private readonly IMyTerminalBlock _me;
        private readonly IMyGridTerminalSystem _grid;
        private DateTime _lastBlockScan = DateTime.MinValue;
        private DateTime _lastSensorScan = DateTime.MinValue;
        private readonly Dictionary<string, Room> _rooms;
        private readonly Program _program;

        public RoomController(Program program, IMyTerminalBlock Me)
        {
            _program = program;
            _grid = _program.GridTerminalSystem;
            _me = Me;
            _rooms = new Dictionary<string, Room>();
            Discover();
        }

        public void Discover()
        {
            // Reset the room list
            _rooms.Clear();

            List<IMyBlockGroup> groups = FindAllRoomGroups();

            // Now that we have a list of known rooms, attach the various entities to them
            // Add all the vents to the room
            _program.Debug(new string('=', 25), DEBUG_DOOR_DISCOVERY);
            foreach (var group in groups)
            {
                // Get the room name out of the group name
                string name = RoomNameFromGroupName(group);
                Room room = _rooms[name];

                _program.Debug($"Looking for blocks in room [{room}]...", DEBUG_DISCOVERY || DEBUG_DOOR_DISCOVERY);

                // We're only interested in blocks in the same grid as the script for now
                // Sub-grid stuff gets tricky.
                List<IMyTerminalBlock> blocks = FindBlocksOnSameGrid(group, name);

                AddVentsToRoom(room, blocks);
                AddDisplaysToRoom(room, blocks);
                AddLightsToRoom(room, blocks);
                AddSensorsToRoom(room, blocks);
                AddDoorsToRoom(name, room, blocks);
            }

            // Now that we have all the entities, attempt to link the sensors to their nearest door
            LinkSensorsToDoors();

            // Finally remove any rooms that are still emtpy. They're probably on other grids
            PruneEmptyRooms();

            _lastBlockScan = DateTime.Now;
        }

        private List<IMyBlockGroup> FindAllRoomGroups()
        {
            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
            _grid.GetBlockGroups(groups, g => g.Name.Contains(Program.TAG));
            foreach (var group in groups)
            {
                // Get the room name out of the group name
                string name = RoomNameFromGroupName(group);
                // Can't have duplicate room names
                if (_rooms.ContainsKey(name))
                {
                    continue;
                }

                // Create a new room
                Room room = new Room(_program, name);
                //room.Discover(blocks);
                _program.Debug($"Added room [{name}]", DEBUG_DISCOVERY);
                _rooms.Add(name, room);
            }

            return groups;
        }

        private List<IMyTerminalBlock> FindBlocksOnSameGrid(IMyBlockGroup group, string name)
        {
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks, x => x.IsSameConstructAs(_me));
            if (DEBUG_GROUPS)
            {
                _program.Debug($"Found room group {name}", DEBUG_GROUPS);
                foreach (var block in blocks)
                {
                    _program.Debug($"+  {block.CustomName}", DEBUG_GROUPS);
                }
            }

            return blocks;
        }

        private void AddVentsToRoom(Room room, List<IMyTerminalBlock> blocks)
        {
            foreach (IMyAirVent vent in blocks.Where(x => x is IMyAirVent))
            {
                _program.Debug($"  Found a vent [{vent.CustomName}].", DEBUG_DISCOVERY);
                room.Vents.Add(new Vent(vent) { Room = room });
            }
        }

        private void AddDisplaysToRoom(Room room, List<IMyTerminalBlock> blocks)
        {
            foreach (IMyTextPanel panel in blocks.Where(x => x is IMyTextPanel))
            {
                _program.Debug($"  Found a display [{panel.CustomName}].", DEBUG_DISCOVERY);
                room.Displays.Add(new Display(panel) { Room = room });
            }
        }

        private void AddLightsToRoom(Room room, List<IMyTerminalBlock> blocks)
        {
            foreach (IMyLightingBlock light in blocks.Where(x => x is IMyLightingBlock))
            {
                _program.Debug($"  Found a light [{light.CustomName}].", DEBUG_DISCOVERY);
                room.Lights.Add(new Light(light));
            }
        }

        private void AddSensorsToRoom(Room room, List<IMyTerminalBlock> blocks)
        {
            foreach (IMySensorBlock sensor in blocks.Where(x => x is IMySensorBlock))
            {
                _program.Debug($"  Found a sensor [{sensor.CustomName}].", DEBUG_DISCOVERY);
                room.Sensors.Add(new Sensor(sensor));
            }
        }

        private void AddDoorsToRoom(string name, Room room, List<IMyTerminalBlock> blocks)
        {
            foreach (IMyDoor door in blocks.Where(x => x is IMyDoor))
            {
                var number = door.EntityId.ToString("n0");
                var code = number.Substring(number.Length - 3);
                _program.Debug($"  Found a door {door.CustomName} ({code})", DEBUG_DOOR_DISCOVERY);

                // Doors might already exist in another room, in which case we
                // don't want to create a duplicate
                Door found_door = null;
                _program.Debug($"    Rooms: {_rooms.Count}.", DEBUG_DOOR_DISCOVERY);
                foreach (var other_room in _rooms.Values)
                {
                    _program.Debug($"    Comparing room {other_room.Name} with {name}.", DEBUG_DOOR_DISCOVERY);
                    if (other_room.Name == name)
                    {
                        _program.Debug($"    Skipping room {name} as it's the same room.", DEBUG_DOOR_DISCOVERY);
                    }
                    else
                    {
                        var found_doors = other_room.Doors.Where(x => x.EntityId == door.EntityId);
                        _program.Debug($"    Found {found_doors.Count()} matching doors in {name}!", DEBUG_DOOR_DISCOVERY);

                        if (found_doors.Count() > 0)
                        {
                            // door already exists, so add this new room as it's "second" room.
                            _program.Debug($"      Door is the same, update as new Room2", DEBUG_DOOR_DISCOVERY);
                            found_door = found_doors.First();
                            found_door.Room2 = room;
                            break;
                        }
                    }
                }

                if (found_door == null)
                {
                    // The door wasn't found anywhere else, so it's a new door
                    _program.Debug($"    Adding as a new door.", DEBUG_DOOR_DISCOVERY);

                    found_door = new Door(door)
                    {
                        Room1 = room
                    };
                }

                room.Doors.Add(found_door);
            }
            _program.Debug(new string('-', 25), DEBUG_DOOR_DISCOVERY);
        }

        private void PruneEmptyRooms()
        {
            foreach (var room in _rooms.Where(r => r.Value.IsEmpty).ToList())
            {
                _program.Debug($"Removing empty room [{room.Key}].", DEBUG_DISCOVERY || DEBUG_GROUPS);
                _rooms.Remove(room.Key);
            }
        }

        private void LinkSensorsToDoors()
        {
            foreach (var room in _rooms.Values)
            {
                foreach (var sensor in room.Sensors)
                {
                    double closest_distance = double.MaxValue;
                    Door closest_door = null;

                    foreach (var door in room.Doors)
                    {
                        double distance = Vector3D.Distance(sensor.Position, door.Position);
                        if (distance < closest_distance)
                        {
                            closest_distance = distance;
                            closest_door = door;
                        }
                    }

                    _program.Debug($"Linked sensor [{sensor}] to door {closest_door}.", DEBUG_DISCOVERY);
                    sensor.Door = closest_door;
                    closest_door.Sensor = sensor;
                }
            }
        }

        private static string RoomNameFromGroupName(IMyBlockGroup group)
        {
            int a = group.Name.LastIndexOf("(") + 1;
            int b = group.Name.LastIndexOf(")");
            string name = group.Name.Substring(a, b - a);
            return name;
        }

        public void Update(string argument, UpdateType updateSource)
        {
            // Force a manual rescan with 'update'
            if(updateSource == UpdateType.Trigger && argument == "update")
            {
                Discover();
            }

            // Periodically scan for new or changed rooms
            if ((DateTime.Now - _lastBlockScan).TotalSeconds > SECONDS_BETWEEN_REDISCOVERY)
            {
                Discover();
            }

            // Update every room
            foreach (var room in _rooms.Values)
            {
                room.Update();
            }

            if ((updateSource & UpdateType.Update10) != 0)
            {
                // Update all the signs that are full room displays
                // These display the detailed information about the whole grid.
                //foreach (var entity in Displays.Where(d => d.Mode == Display.DisplayType.ROOMS_SIGN))
                //{
                //    entity.UpdateRoomsDisplay(_rooms);
                //}
            }

            if ((DateTime.Now - _lastSensorScan).TotalMilliseconds > MILLISECONDS_BETWEEN_SENSORSCANS)
            {
                ScanForTriggeredDoors();
            }
        }

        private void ScanForTriggeredDoors()
        {
            foreach(var room in _rooms.Values)
            {
                // Doors can be triggered either by bad air, or by sensors
                foreach (var door in room.Doors)
                {
                    _program.Debug($"Checking {door.Name} in {room.Name}...", DEBUG_DOORS);
                    // If the door is not safe, check the sensors
                    var door_safe = door.IsSafe();
                    if (door_safe)
                    {
                        _program.Debug($"  door [{door}] is safe.", DEBUG_DOORS);
                        // Open and close based on any attached sensors
                        if (door.Sensor?.IsActive == true)
                        {
                            // Sensor is currently triggered, so open the door
                            _program.Debug($"    sensor [{door.Sensor}] is triggered!", DEBUG_DOORS);
                            door.Open();
                        }
                        else if(door.Sensor?.IsActive == false)
                        {
                            _program.Debug($"    sensor [{door.Sensor}] is not triggered.", DEBUG_DOORS);
                            // Only auto-close the door if the mode requires it
                            if (door.Mode == Door.DoorMode.AUTO_CLOSE)
                            {
                                door.Close();
                            }
                        }
                        door.WasSafe = true;
                    }
                    else
                    {
                        _program.Debug($"  door [{door}] is not safe!", DEBUG_DOORS);
                        // Door is not safe, so ignore sensors, but if it looks like it lost air, close it.
                        // This attempts to close the door once, but if someone manually opens it, it stays
                        // that way until they clear sensor range
                        if (door.WasSafe && door_safe == false)
                        {
                            door.WasSafe = false;
                            door.NeedsClosing = true;
                        }

                        // Door is not safe, but now needs closing, so check for sensors
                        if (door.Sensor?.IsActive == false)
                        {
                            door.Close();
                            door.NeedsClosing = false;
                        }
                    }
                }
            }
            _lastSensorScan = DateTime.Now;
        }
    }
}
