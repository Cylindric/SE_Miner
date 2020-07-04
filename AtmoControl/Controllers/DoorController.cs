using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    internal class DoorController : BaseController
    {
        private readonly string _tag = "[ATMO]";
        private DateTime _lastBlockScan = DateTime.MinValue;
        private int _secondsBetweenScans = 5;

        public DoorController(Program program) : base(program)
        {
            Doors = new List<Door>();
        }

        internal List<Door> Doors { get; }

        public new void Update()
        {
            if (_lastBlockScan.AddSeconds(_secondsBetweenScans) < DateTime.Now)
            {
                Discover();
            }
        }

        /// <summary>
        /// Find all doors named with [ATMO] (room1) (room2)
        /// </summary>
        public void Discover()
        {
            var all_doors = new List<IMyDoor>();
            _grid.GetBlocksOfType(all_doors);
            Doors.Clear();
            foreach (var d in all_doors)
            {
                if (d.CustomName.Contains(_tag))
                {
                    Doors.Add(new Door(d));
                }
            }
            _lastBlockScan = DateTime.Now;
        }
    }
}