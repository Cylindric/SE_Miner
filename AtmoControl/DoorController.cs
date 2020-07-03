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

        public override void Update()
        {
            if (_lastBlockScan.AddSeconds(_secondsBetweenScans) < DateTime.Now)
            {
                DiscoverDoors();
            }
        }

        /// <summary>
        /// Find all doors named with [ATMO] (room1) (room2)
        /// </summary>
        public void DiscoverDoors()
        {
            var all_doors = new List<IMyDoor>();
            _grid.GetBlocksOfType(all_doors);
            Doors.Clear();
            foreach (var d in all_doors)
            {
                if (d.CustomName.Contains(_tag))
                {
                    Door door = new Door(d);

                    string name = d.CustomName;
                    int a = name.LastIndexOf("(") + 1;
                    int b = name.LastIndexOf(")");
                    door.Room2 = name.Substring(a, b - a);

                    name = name.Substring(0, a-1);
                    a = name.LastIndexOf("(") + 1;
                    b = name.LastIndexOf(")");
                    door.Room1 = name.Substring(a, b - a);

                    Doors.Add(door);

                    //_prog.Echo($"Found door {door.Room1} - {door.Room2}");
                }
            }
            _lastBlockScan = DateTime.Now;
        }
    }
}