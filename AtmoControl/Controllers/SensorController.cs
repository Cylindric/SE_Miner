using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    internal class SensorController : BaseController
    {
        private const string TAG = "[ATMO]";
        private const int SECONDS_BETWEEN_SCANS = 5;
        private DateTime _lastBlockScan = DateTime.MinValue;

        public SensorController(Program program, IMyCubeGrid homeGrid) : base(program, homeGrid)
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
            foreach (var s in all_sensors.Where(x => x.CubeGrid == _homeGrid))
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
