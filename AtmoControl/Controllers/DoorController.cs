using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    internal class DoorController : BaseController
    {
        private const string TAG = "[ATMO]";
        private const int SECONDS_BETWEEN_SCANS = 5;
        private DateTime _lastBlockScan = DateTime.MinValue;
        
        public DoorController(Program program) : base(program)
        {
            Doors = new List<Door>();
        }

        internal List<Door> Doors { get; }

        public new void Update()
        {
            if (_lastBlockScan.AddSeconds(SECONDS_BETWEEN_SCANS) < DateTime.Now)
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
            foreach (var door in all_doors)
            {
                if (door.CustomName.Contains(TAG))
                {
                    Doors.Add(new Door(door));
                }
            }
            _lastBlockScan = DateTime.Now;
        }
    }
}