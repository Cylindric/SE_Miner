using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    internal class LightController : BaseController
    {
        private const string TAG = "[ATMO]";
        private const int SECONDS_BETWEEN_SCANS = 5;
        private DateTime _lastBlockScan = DateTime.MinValue;

        public LightController(Program program) : base(program)
        {
            Lights = new List<Light>();
        }

        internal List<Light> Lights { get; }

        public new void Update()
        {
            if ((DateTime.Now - _lastBlockScan).TotalSeconds > SECONDS_BETWEEN_SCANS)
            {
                Discover();
            }
        }

        /// <summary>
        /// Find all lights named with [ATMO]
        /// </summary>
        public void Discover()
        {
            var all_lights = new List<IMyLightingBlock>();
            _grid.GetBlocksOfType(all_lights);
            Lights.Clear();
            foreach (var light in all_lights)
            {
                if (light.CustomName.Contains(TAG))
                {
                    Lights.Add(new Light(light));
                }
            }
            _lastBlockScan = DateTime.Now;
        }
    }
}