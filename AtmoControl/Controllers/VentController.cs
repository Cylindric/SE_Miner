using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    internal class VentController : BaseController
    {
        private const string TAG = "[ATMO]";
        private const int SECONDS_BETWEEN_SCANS = 5;
        private DateTime _lastBlockScan = DateTime.MinValue;

        public VentController(Program program, IMyCubeGrid homeGrid) : base(program, homeGrid)
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
            foreach (var v in all_vents.Where(x => x.CubeGrid == _homeGrid))
            {
                if (v.CustomName.Contains(TAG))
                {
                    // Don't include inactive vents, so if there are multiples
                    // and one is damage the system still works
                    if (v.IsWorking)
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
            }
            _lastBlockScan = DateTime.Now;
        }

        public new void Update()
        {
            if (_lastBlockScan.AddSeconds(SECONDS_BETWEEN_SCANS) < DateTime.Now)
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
