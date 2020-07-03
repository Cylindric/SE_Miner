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

        public override void Update()
        {
            if (_lastBlockScan.AddSeconds(_secondsBetweenScans) < DateTime.Now)
            {
                DiscoverDisplays();
            }
        }

        /// <summary>
        /// Find all displays named with [ATMO] (room1) (room2)
        /// </summary>
        public void DiscoverDisplays()
        {
            var all = new List<IMyTextPanel>();
            _grid.GetBlocksOfType(all);
            Displays.Clear();
            foreach (var d in all)
            {
                if (d.CustomName.Contains(_tag))
                {
                    Display display = new Display(d);

                    string name = d.CustomName;
                    int a = name.LastIndexOf("(") + 1;
                    int b = name.LastIndexOf(")");
                    display.Room1 = name.Substring(a, b - a);

                    Displays.Add(display);
                }
            }
            _lastBlockScan = DateTime.Now;
        }
    }
}