using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    internal class SensorController : BaseController
    {
        private const string TAG = "[ATMO]";
        private const int SECONDS_BETWEEN_SCANS = 5;
        private DateTime _lastBlockScan = DateTime.MinValue;

        public SensorController(Program program) : base(program)
        {
            Sensors = new List<Sensor>();
        }

        internal List<Sensor> Sensors { get; }

        /// <summary>
        /// Find all sensors named with [ATMO]
        /// </summary>
        public void Discover()
        {
            var all_sensors = new List<IMySensorBlock>();
            _grid.GetBlocksOfType(all_sensors);
            Sensors.Clear();
            foreach (var s in all_sensors)
            {
                if (s.CustomName.Contains(TAG))
                {
                    Sensors.Add(new Sensor(s));
                }
            }
            _lastBlockScan = DateTime.Now;
        }

        public new void Update()
        {
            if (_lastBlockScan.AddSeconds(SECONDS_BETWEEN_SCANS) < DateTime.Now)
            {
                Discover();
            }
        }

    }
}
