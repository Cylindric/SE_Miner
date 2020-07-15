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
        private const int SECONDS_BETWEEN_REDISCOVERY = 60;
        private const int MILLISECONDS_BETWEEN_SENSORSCANS= 1000;

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

            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
            _grid.GetBlockGroups(groups, g => g.Name.Contains(Program.TAG));
            foreach(var group in groups)
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
                _program.Debug($"Added room [{name}]");
                _rooms.Add(name, room);
            }

            // Now that we have a list of known rooms, attach the various entities to them
            // Add all the vents to the room
            foreach (var group in groups)
            {
                // Get the room name out of the group name
                string name = RoomNameFromGroupName(group);
                _program.Debug($"Looking for blocks in room [{name}]...");
                

                // We're only interested in blocks in the same grid as the script for now
                // Sub-grid stuff gets tricky.
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                group.GetBlocks(blocks, x => x.IsSameConstructAs(_me));

                // Add all the vents to the room
                foreach (IMyAirVent vent in blocks.Where(x => x is IMyAirVent))
                {
                    _program.Debug($"  Found a vent [{vent.CustomName}].");
                    _rooms[name].Vents.Add(new Vent(vent));
                }

                // Add all the displays to the room
                foreach (IMyTextPanel panel in blocks.Where(x => x is IMyTextPanel))
                {
                    _program.Debug($"  Found a display [{panel.CustomName}].");
                    _rooms[name].Displays.Add(new Display(panel) { Room = name});
                }

                // Add all the lights to the room
                foreach (IMyLightingBlock light in blocks.Where(x => x is IMyLightingBlock))
                {
                    _program.Debug($"  Found a light [{light.CustomName}].");
                    _rooms[name].Lights.Add(new Light(light));
                }

                // Add all the sensors to the room
                foreach (IMySensorBlock sensor in blocks.Where(x => x is IMySensorBlock))
                {
                    _program.Debug($"  Found a sensor [{sensor.CustomName}].");
                    _rooms[name].Sensors.Add(new Sensor(sensor));
                }

                // Add all the doors to the rooms they are attached to
                foreach (IMyDoor door in blocks.Where(x => x is IMyDoor))
                {
                    //_program.Debug($"Attemptying to add door [{door.CustomName}] to room [{name}]...");

                    // Doors might already exist in another room, in which case we
                    // don't want to create a duplicate
                    Door new_door = null;
                    foreach (var other_room in _rooms.Where(x => x.Key != name))
                    {
                        new_door = other_room.Value.Doors.FirstOrDefault(x => x.EntityId == door.EntityId);
                        if (new_door != null)
                        {
                            // door already exists, so add this new room as it's "second" room.
                            new_door.Room2 = _rooms[name];
                        }
                        break;
                    }
                    if (new_door == null)
                    {
                        _program.Debug($"  Found a door [{door.CustomName}].");

                        new_door = new Door(door)
                        {
                            Room1 = _rooms[name]
                        };
                    }
                    _rooms[name].Doors.Add(new_door);
                }
            }

            // Now that we have all the entities, attempt to link the sensors to their nearest door
            foreach (var room in _rooms.Values)
            {
                foreach (var sensor in room.Sensors)
                {
                    double closest_distance = double.MaxValue;
                    Door closest_door = null;
                    
                    foreach(var door in room.Doors)
                    {
                        double distance = Vector3D.Distance(sensor.Position, door.Position);
                        if(distance < closest_distance)
                        {
                            closest_distance = distance;
                            closest_door = door;
                        }
                    }

                    _program.Debug($"Linked sensor [{sensor}] to door {closest_door}.");
                    sensor.Door = closest_door;
                    closest_door.Sensor = sensor;
                }
            }

            _lastBlockScan = DateTime.Now;
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
                    //_program.Debug($"Checking {door.Name} in {room.Name}...");
                    // If the door is not safe, check the sensors
                    var door_safe = door.IsSafe();
                    if (door_safe)
                    {
                        //_program.Debug($"  door [{door}] is safe.");
                        // Open and close based on any attached sensors
                        if (door.Sensor?.IsActive == true)
                        {
                            // Sensor is currently triggered, so open the door
                            //_program.Debug($"    sensor [{door.Sensor}] is triggered!");
                            door.Open();
                        }
                        else
                        {
                            //_program.Debug($"    sensor [{door.Sensor}] is not triggered.");
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
                        //_program.Debug($"  door [{door}] is not safe!");
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
