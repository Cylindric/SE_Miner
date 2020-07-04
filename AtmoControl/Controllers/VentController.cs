using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    internal class VentController : BaseController
    {
        private readonly string _tag = "[ATMO]";
        private DateTime _lastBlockScan = DateTime.MinValue;
        private int _secondsBetweenScans = 5;

        public VentController(Program program) : base(program)
        {
            Vents = new List<Vent>();
        }

        internal List<Vent> Vents { get; }

        /// <summary>
        /// Find all vents named with [ATMO] (room)
        /// </summary>
        public void Discover()
        {
            var all_vents = new List<IMyAirVent>();
            _grid.GetBlocksOfType(all_vents);
            Vents.Clear();
            foreach (var v in all_vents)
            {
                if (v.CustomName.Contains(_tag))
                {
                    var vent = new Vent(v);
                    if (Vents.Any(x => x.Room1 == vent.Room1))
                    {
                        // duplicate vent.
                    }
                    else
                    {
                        Vents.Add(vent);
                    }
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

        public bool RoomIsSafe(string roomName)
        {
            foreach(var v in Vents.Where(v => v.Room1 == roomName))
            {
                return v.Safe;
            }
            return false;
        }
    }
}
