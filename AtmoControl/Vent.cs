using SpaceEngineers.Game.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    class Vent
    {
        private IMyAirVent _block;
        private string _room;

        public Vent(IMyAirVent block)
        {
            _block = block;
        }

        public float GetOxygenLevel
        {
            get
            {
                return _block.GetOxygenLevel();
            }
        }

        public bool Safe
        {
            get
            {
                return GetOxygenLevel > 0.95;
            }
        }

        public bool Unsafe
        {
            get
            {
                return !Safe;
            }
        }

        public string Room
        {
            get
            {
                return _room;
            }

            set
            {
                _room = value;
            }
        }
    }
}
