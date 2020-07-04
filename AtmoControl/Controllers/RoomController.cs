using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    class RoomController : BaseController
    {
        private const int SECONDS_BETWEEN_SCANS = 5;

        private readonly DateTime _lastBlockScan = DateTime.MinValue;
        private readonly Dictionary<string, Room> _rooms;
        private readonly VentController _vents;
        private readonly DoorController _doors;
        private readonly DisplayController _displays;
        private readonly LightController _lights;
        private readonly SensorController _sensors;


        public RoomController(Program program) : base(program)
        {
            _rooms = new Dictionary<string, Room>();
            _vents = new VentController(program);
            _vents.Discover();
            _doors = new DoorController(program);
            _doors.Discover();
            _displays = new DisplayController(program);
            _displays.Discover();
            _lights = new LightController(program);
            _lights.Discover();
            _sensors = new SensorController(program);
            _sensors.Discover();
        }

        public new void Update(UpdateType updateSource)
        {
            if ((updateSource & UpdateType.Update100) != 0)
            {
                _vents.Update();
                _doors.Update();
                _displays.Update();
                _lights.Update();
                _sensors.Update();

                if ((DateTime.Now - _lastBlockScan).TotalSeconds > SECONDS_BETWEEN_SCANS)
                {
                    ScanForNewRooms();
                    UpdateDisplays();
                }
            }
            if ((updateSource & UpdateType.Update10) != 0)
            {
                ScanForTriggeredDoors();
            }
        }

        private void ScanForTriggeredDoors()
        {
            // Doors can be triggered either by bad air, or by sensors
            foreach (var door in _doors.Doors)
            {
                // If the door is not safe, ignore the sensors
                var door_safe = (_vents.RoomIsSafe(door.Room1) && _vents.RoomIsSafe(door.Room2));
                if (door_safe)
                {
                    // Open and close based on any attached sensors
                    foreach (var sensor in _sensors.Sensors.Where(s => s.Link == door.Id))
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
                    foreach (var sensor in _sensors.Sensors.Where(s => s.Link == door.Id))
                    {
                        if(sensor.IsActive == false)
                        {
                            door.Close();
                            door.NeedsClosing = false;
                        }
                    }
                }
            }
        }

        private void ScanForNewRooms()
        {
            _rooms.Clear();

            // Create a new Room for every found Vent
            foreach (var v in _vents.Vents)
            {
                if (!_rooms.ContainsKey(v.Room1))
                {
                    _rooms.Add(v.Room1, new Room(v.Room1));
                }

                _rooms[v.Room1].Vents.Add(v);
            }

            // Create a new Room for every found Door
            foreach (var v in _doors.Doors)
            {
                if (!_rooms.ContainsKey(v.Room1))
                {
                    _rooms.Add(v.Room1, new Room(v.Room1));
                }
                if (!_rooms.ContainsKey(v.Room2))
                {
                    _rooms.Add(v.Room2, new Room(v.Room2));
                }

                // Add this door to both rooms it connects to
                _rooms[v.Room1].Doors.Add(v);
                _rooms[v.Room2].Doors.Add(v);
            }

            // Create a new Room for every found Display
            foreach(var v in _displays.Displays)
            {
                if (string.IsNullOrEmpty(v.Room))
                {
                    // display without a room
                }
                else
                {
                    if (!_rooms.ContainsKey(v.Room))
                    {
                        _rooms.Add(v.Room, new Room(v.Room));
                    }
                    _rooms[v.Room].Displays.Add(v);
                }

            }
        }

        /// <summary>
        /// Update every display and light attached to each vent
        /// </summary>
        private void UpdateDisplays()
        {
            // Update all the signs that are full room displays
            foreach (var entity in _displays.Displays.Where(d => d.Mode == Display.DisplayType.ROOMS_SIGN))
            {
                entity.UpdateRoomsDisplay(_rooms);
            }

            // Update all the signs that are door displays
            foreach (var entity in _displays.Displays.Where(d => d.Mode == Display.DisplayType.DOOR_SIGN))
            {
                foreach (var vent in _vents.Vents.Where(x => x.Room1.Equals(entity.Room)))
                {
                    entity.UpdateSafety(vent.Safe);
                }
            }

            // Update any lights that need turning on
            foreach (var entity in _lights.Lights)
            {
                foreach (var vent in _vents.Vents.Where(x => x.Room1.Equals(entity.Room)))
                {
                    entity.UpdateSafety(vent.Safe);
                }
            }
        }
    }
}
