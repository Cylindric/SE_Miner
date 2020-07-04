using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    class RoomController : BaseController
    {
        private DateTime _lastBlockScan = DateTime.MinValue;
        private int _secondsBetweenScans = 5;

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

                if ((DateTime.Now - _lastBlockScan).TotalSeconds > _secondsBetweenScans)
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
            foreach(var sensor in _sensors.Sensors)
            {
                foreach(var door in _doors.Doors.Where(d => d.Id == sensor.Link))
                {
                    // A sensor covers two rooms. 
                    // If either room is dangerous, don't use the sensor data.
                    var door_safe = (_vents.RoomIsSafe(door.Room1) && _vents.RoomIsSafe(door.Room2));
                    if (door_safe)
                    {
                        if (door_safe && sensor.IsActive)
                        {
                            door.Open();
                        }
                        else
                        {
                            door.Close();
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
                    //_prog.Echo($"Adding new room {v.Room1}");
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
                if (!_rooms.ContainsKey(v.Room))
                {
                    _rooms.Add(v.Room, new Room(v.Room));
                }

                _rooms[v.Room].Displays.Add(v);
            }
        }

        /// <summary>
        /// Update every display and light attached to each vent
        /// </summary>
        private void UpdateDisplays()
        {
            foreach (var entity in _displays.Displays)
            {
                foreach (var vent in _vents.Vents.Where(x => x.Room1.Equals(entity.Room)))
                {
                    entity.UpdateSafety(vent.Safe);
                }
            }
            
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
