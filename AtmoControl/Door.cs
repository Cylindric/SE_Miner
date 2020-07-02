using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    class Door
    {
        private IMyDoor _block;
        private string _room1;
        private string _room2;

        public Door(IMyDoor block)
        {
            _block = block;
        }

        public string Room1
        {
            get
            {
                return _room1;
            }

            set
            {
                _room1 = value;
            }
        }

        public string Room2
        {
            get
            {
                return _room2;
            }

            set
            {
                _room2 = value;
            }
        }
    }
}
