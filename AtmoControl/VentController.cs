using EmptyKeys.UserInterface.Generated.DataTemplatesContracts_Bindings;
using Sandbox.ModAPI.Ingame;
using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    internal class VentController : BaseController
    {
        private readonly string _tag = "[ATMO]";
        private DateTime _lastBlockScan = DateTime.MinValue;
        private int _secondsBetweenScans = 5;

        private Dictionary<string, Vent> _vents = new Dictionary<string, Vent>();

        public VentController(Program program) : base(program)
        {
        }

        /// <summary>
        /// Find all vents named with [ATMO] (room)
        /// </summary>
        public void DiscoverVents()
        {
            var all_vents = new List<IMyAirVent>();
            _grid.GetBlocksOfType(all_vents);
            _vents.Clear();
            foreach (var v in all_vents)
            {
                if (v.CustomName.Contains(_tag))
                {
                    int a = v.CustomName.LastIndexOf("(")+1;
                    int b = v.CustomName.LastIndexOf(")");
                    string name = v.CustomName.Substring(a, b-a);
                    if (_vents.ContainsKey(name))
                    {
                        // duplicate vent.
                    }
                    else
                    {
                        Vent vent = new Vent(v)
                        {
                            Room = name
                        };
                        _vents.Add(name, vent);
                    }
                    _prog.Echo($"Found vent {name}");
                }
            }
            _lastBlockScan = DateTime.Now;
        }

        public override void Update()
        {
            if (_lastBlockScan.AddSeconds(_secondsBetweenScans) < DateTime.Now)
            {
                DiscoverVents();
            }
        }

    }
}
