using EmptyKeys.UserInterface.Generated.DataTemplatesContracts_Bindings;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IngameScript
{
    class Room
    {
        private const string SAFE_ROOM = "atmosphere";
        private const int SECONDS_BETWEEN_SCANS = 5;
        private const int SECONDS_BETWEEN_DISPLAY_UPDATES = 1;

        private DateTime _lastBlockScan = DateTime.MinValue;
        private DateTime _lastDisplayUpdate = DateTime.MinValue;

        public Room(string name)
        {
            Name = name;
            Vents = new List<Vent>();
            Doors = new List<Door>();
            Displays = new List<Display>();
            Sensors = new List<Sensor>();
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
            // Update all the signs that are door displays
            foreach (var entity in Displays.Where(d => d.Mode == Display.DisplayType.DOOR_SIGN))
            {
                foreach (var vent in Vents.Where(x => x.Room1.Equals(entity.Room)))
                {
                    entity.UpdateSafety(vent.Safe);
                }
            }

            // Update any lights that need turning on
            foreach (var entity in Lights)
            {
                foreach (var vent in Vents.Where(x => x.Room1.Equals(entity.Room)))
                {
                    entity.UpdateSafety(vent.Safe);
                }
            }
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
