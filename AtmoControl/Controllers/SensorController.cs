using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    internal class SensorController : BaseController
    {
        private readonly string _tag = "[ATMO]";
        private DateTime _lastBlockScan = DateTime.MinValue;
        private int _secondsBetweenScans = 5;

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
            var all_entities = new List<IMySensorBlock>();
            _grid.GetBlocksOfType(all_entities);
            Sensors.Clear();
            foreach (var v in all_entities)
            {
                if (v.CustomName.Contains(_tag))
                {
                    Sensors.Add(new Sensor(v));
                }
            }
            _lastBlockScan = DateTime.Now;
        }

        public new void Update()
        {
            if (_lastBlockScan.AddSeconds(_secondsBetweenScans) < DateTime.Now)
            {
                Discover();
            }
        }

    }
}
