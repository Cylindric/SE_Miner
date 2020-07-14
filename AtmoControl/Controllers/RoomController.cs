using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    class RoomController : BaseController
    {
        private const int SECONDS_BETWEEN_SCANS = 5;

        private readonly IMyTerminalBlock _me;
        private readonly DateTime _lastBlockScan = DateTime.MinValue;
        private readonly Dictionary<string, Room> _rooms;
        //private readonly VentController _vents;
        //private readonly DoorController _doors;
        //private readonly DisplayController _displays;
        //private readonly LightController _lights;
        //private readonly SensorController _sensors;

        public RoomController(Program program, IMyTerminalBlock Me) : base(program, Me.CubeGrid)
        {
            _me = Me;
            _rooms = new Dictionary<string, Room>();
            Discover();

            //_vents = new VentController(program, _homeGrid);
            //_vents.Discover();
            //_doors = new DoorController(program, _homeGrid);
            //_doors.Discover();
            //_displays = new DisplayController(program, _homeGrid);
            //_displays.Discover();
            //_lights = new LightController(program, _homeGrid);
            //_lights.Discover();
            //_sensors = new SensorController(program, _homeGrid);
            //_sensors.Discover();
        }

        public void Discover()
        {
            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
            _grid.GetBlockGroups(groups, g => g.Name.Contains("[ATMO]"));
            foreach(var group in groups)
            {
                // Reset the room list
                _rooms.Clear();

                // Get the room name out of the group name
                string name = RoomNameFromGroupName(group);

                // Can't have duplicate room names
                if (_rooms.ContainsKey(name))
                {
                    continue;
                }

                // Create a new room
                Room room = new Room(name);
                //room.Discover(blocks);

                _rooms.Add(name, room);
            }

            // Now that we have a list of known rooms, attach the various entities to them
            // Add all the vents to the room
            foreach (var group in groups)
            {
                // Get the room name out of the group name
                string name = RoomNameFromGroupName(group);

                // We're only interested in blocks in the same grid as the script for now
                // Sub-grid stuff gets tricky.
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                group.GetBlocks(blocks, x => x.IsSameConstructAs(_me));

                // Add all the vents to the room
                foreach (IMyAirVent vent in blocks.Where(x => x is IMyAirVent))
                {
                    _rooms[name].Vents.Add(new Vent(vent));
                }

                // Add all the displays to the room
                foreach (IMyTextPanel panel in blocks.Where(x => x is IMyTextPanel))
                {
                    _rooms[name].Displays.Add(new Display(panel));
                }

                // Add all the lights to the room
                foreach (IMyLightingBlock light in blocks.Where(x => x is IMyLightingBlock))
                {
                    _rooms[name].Lights.Add(new Light(light));
                }

                // Add all the sensors to the room
                foreach (IMySensorBlock sensor in blocks.Where(x => x is IMySensorBlock))
                {
                    _rooms[name].Sensors.Add(new Sensor(sensor));
                }

                // Add all the doors to the rooms they are attached to
                foreach (IMyDoor door in blocks.Where(x => x is IMyDoor))
                {
                    // Doors might already exist in another room, in which case we
                    // don't want to create a duplicate
                    Door new_door = null;
                    foreach(var other_room in _rooms.Where(x => x.Key != name))
                    {
                        new_door = other_room.Value.Doors.FirstOrDefault(x => x.EntityId == door.EntityId);
                        new_door.Room2 = name;
                        break;
                    }
                    if(new_door == null)
                    {
                        new_door = new Door(door);
                        new_door.Room1 = name;
                    }
                    _rooms[name].Doors.Add(new_door);
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

        public new void Update(UpdateType updateSource)
        {
            // Periodically scan for new or changed rooms
            if ((DateTime.Now - _lastBlockScan).TotalSeconds > SECONDS_BETWEEN_SCANS)
            {
                Discover();
            }

            // Update every room
            foreach (var room in _rooms)
            {
                room.Value.Update();
            }

            if ((updateSource & UpdateType.Update100) != 0)
            {
                // Update all the signs that are full room displays
                // These display the detailed information about the whole grid.
                //foreach (var entity in Displays.Where(d => d.Mode == Display.DisplayType.ROOMS_SIGN))
                //{
                //    entity.UpdateRoomsDisplay(_rooms);
                //}
            }

            if ((updateSource & UpdateType.Update10) != 0)
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
                    // If the door is not safe, ignore the sensors
                    var door_safe = room.IsSafe();
                    if (door_safe)
                    {
                        // Open and close based on any attached sensors
                        foreach (var sensor in room.Sensors.Where(s => s.Link == door.Id))
                        {
                            if (sensor.IsActive)
                            {
                                // Sensor is currently triggered, so open the door
                                door.Open();
                            }
                            else
                            {
                                // Only auto-close the door if the mode requires it
                                if (door.Mode == Door.DoorMode.AUTO_CLOSE)
                                {
                                    door.Close();
                                }
                            }
                        }
                        door.WasSafe = true;
                    }
                    else
                    {
                        // Door is not safe, so ignore sensors, but if it looks like it lost air, close it.
                        // This attempts to close the door once, but if someone manually opens it, it stays
                        // that way until they clear sensor range
                        if (door.WasSafe && door_safe == false)
                        {
                            door.WasSafe = false;
                            door.NeedsClosing = true;
                        }

                        // Door is not safe, but now needs closing, so check for sensors
                        foreach (var sensor in room.Sensors.Where(s => s.Link == door.Id))
                        {
                            if (sensor.IsActive == false)
                            {
                                door.Close();
                                door.NeedsClosing = false;
                            }
                        }
                    }
                }
            }
        }

    }
}
