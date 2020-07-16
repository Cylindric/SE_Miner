using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    class Room
    {
        private const string SAFE_ROOM = "atmosphere";
        private const int SECONDS_BETWEEN_DISPLAY_UPDATES = 1;
        private const bool DEBUG_UPDATES = false;

        private DateTime _lastDisplayUpdate = DateTime.MinValue;
        private readonly Program _program;

        public Room(Program program, string name)
        {
            _program = program;
            Name = name;
            Vents = new List<Vent>();
            Doors = new List<Door>();
            Displays = new List<Display>();
            Sensors = new List<Sensor>();
            Lights = new List<Light>();
        }

        // Vents report the atmosphere as being unpressurised,
        // so if a room is marked as being connected to the atmoshpere
        // always assume it is safe.
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                AlwaysSafe = (_name == SAFE_ROOM);
            }
        }

        public List<Vent> Vents { get; internal set; }
        public List<Door> Doors { get; internal set; }
        public List<Display> Displays { get; internal set; }
        public List<Sensor> Sensors { get; internal set; }
        public List<Light> Lights { get; internal set; }
        public bool AlwaysSafe { get; set; }

        public override string ToString()
        {
            return Name;
        }

        public void Update()
        {
            if ((DateTime.Now - _lastDisplayUpdate).TotalSeconds > SECONDS_BETWEEN_DISPLAY_UPDATES)
            {
                UpdateSafetyIndicators();
            }
        }

        /// <summary>
        /// Update every display and light attached to each vent
        /// </summary>
        private void UpdateSafetyIndicators()
        {
            _program.Debug($"Updating safety indicators for room [{Name}]...", DEBUG_UPDATES);
            
            // Update all the signs that are door displays
            foreach (var display in Displays?.Where(d => d.Mode == Display.DisplayType.DOOR_SIGN))
            {
                _program.Debug($"  Checking display [{display}]...", DEBUG_UPDATES);
                display.UpdateSafety(IsSafe());
            }

            // Update all the signs that are detailed room displays
            foreach (var display in Displays?.Where(d => d.Mode == Display.DisplayType.ROOM_SIGN))
            {
                _program.Debug($"  Checking display [{display}]...", DEBUG_UPDATES);
                display.UpdateRoomDisplay();
            }

            // Update any lights that need turning on
            foreach (var light in Lights)
            {
                _program.Debug($"  Checking light [{light}]...", DEBUG_UPDATES);
                light.UpdateSafety(IsSafe());
            }

            _lastDisplayUpdate = DateTime.Now;
        }

        public bool IsSafe()
        {
            if (AlwaysSafe)
                return true;

            foreach (var vent in Vents)
            {
                if (!vent.Safe)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
