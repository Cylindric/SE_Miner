using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    internal class DisplayController : BaseController
    {
        private const string TAG = "[ATMO]";
        private const int SECONDS_BETWEEN_SCANS = 5;
        private DateTime _lastBlockScan = DateTime.MinValue;

        public DisplayController(Program program, IMyCubeGrid homeGrid) : base(program, homeGrid)
        {
            Displays = new List<Display>();
        }

        internal List<Display> Displays { get; }

        public new void Update()
        {
            if ((DateTime.Now - _lastBlockScan).TotalSeconds > SECONDS_BETWEEN_SCANS)
            {
                Discover();
            }
        }

        /// <summary>
        /// Find all displays named with [ATMO] (room1) (room2)
        /// </summary>
        public void Discover()
        {
            var all_displays = new List<IMyTextPanel>();
            _grid.GetBlocksOfType(all_displays);
            Displays.Clear();
            foreach (var display in all_displays.Where(x => x.CubeGrid == _homeGrid))
            {
                if (display.CustomName.Contains(TAG))
                {
                    Displays.Add(new Display(display));
                }
            }
            _lastBlockScan = DateTime.Now;
        }
    }
}