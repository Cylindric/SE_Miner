using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    class RoomController : BaseController
    {
        private DateTime _lastBlockScan = DateTime.MinValue;
        private int _secondsBetweenScans = 5;

        private readonly VentController _vents;
        private readonly DoorController _doors;
        private readonly DisplayController _displays;
        private Dictionary<string, Room> _rooms;

        public RoomController(Program program) : base(program)
        {
            _vents = new VentController(program);
            _vents.DiscoverVents();
            _doors = new DoorController(program);
            _doors.DiscoverDoors();
            _displays = new DisplayController(program);
            _displays.DiscoverDisplays();
            _rooms = new Dictionary<string, Room>();
        }

        public override void Update()
        {
            _vents.Update();
            _doors.Update();

            if (_lastBlockScan.AddSeconds(_secondsBetweenScans) < DateTime.Now)
            {
                ScanForNewRooms();
                UpdateDisplays();
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
                if (!_rooms.ContainsKey(v.Room1))
                {
                    _rooms.Add(v.Room1, new Room(v.Room1));
                }

                _rooms[v.Room1].Displays.Add(v);
            }
        }

        /// <summary>
        /// Update every display attached to each vent
        /// </summary>
        private void UpdateDisplays()
        {
            foreach(var display in _displays.Displays)
            {
                foreach (var vent in _vents.Vents.Where(x => x.Room1.Equals(display.Room1)))
                {
                    display.UpdateSafety(vent.Safe);
                }
            }
        }

        public string Debug()
        {
            var output = "Rooms found:\n";
            foreach(var room in _rooms)
            {
                output += $"{room.Key} ( {room.Value.Vents.Count} vents, {room.Value.Doors.Count} doors, {room.Value.Displays.Count} displays)\n";
            }
            return output;
        }
    }
}
