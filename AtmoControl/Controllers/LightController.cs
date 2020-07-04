using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    internal class LightController : BaseController
    {
        private readonly string _tag = "[ATMO]";
        private DateTime _lastBlockScan = DateTime.MinValue;
        private int _secondsBetweenScans = 5;

        public LightController(Program program) : base(program)
        {
            Lights = new List<Light>();
        }

        internal List<Light> Lights { get; }

        public new void Update()
        {
            if ((DateTime.Now - _lastBlockScan).TotalSeconds > _secondsBetweenScans)
            {
                Discover();
            }
        }

        /// <summary>
        /// Find all lights named with [ATMO]
        /// </summary>
        public void Discover()
        {
            var all = new List<IMyLightingBlock>();
            _grid.GetBlocksOfType(all);
            Lights.Clear();
            foreach (var d in all)
            {
                if (d.CustomName.Contains(_tag))
                {
                    Lights.Add(new Light(d));
                }
            }
            _lastBlockScan = DateTime.Now;
        }
    }
}