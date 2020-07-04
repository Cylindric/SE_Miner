using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    internal class DisplayController : BaseController
    {
        private readonly string _tag = "[ATMO]";
        private DateTime _lastBlockScan = DateTime.MinValue;
        private int _secondsBetweenScans = 5;

        public DisplayController(Program program) : base(program)
        {
            Displays = new List<Display>();
        }

        internal List<Display> Displays { get; }

        public new void Update()
        {
            if ((DateTime.Now - _lastBlockScan).TotalSeconds > _secondsBetweenScans)
            {
                Discover();
            }
        }

        /// <summary>
        /// Find all displays named with [ATMO] (room1) (room2)
        /// </summary>
        public void Discover()
        {
            var all = new List<IMyTextPanel>();
            _grid.GetBlocksOfType(all);
            Displays.Clear();
            foreach (var d in all)
            {
                if (d.CustomName.Contains(_tag))
                {
                    Displays.Add(new Display(d));
                }
            }
            _lastBlockScan = DateTime.Now;
        }
    }
}