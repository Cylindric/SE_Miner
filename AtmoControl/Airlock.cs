using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;

namespace IngameScript
{
    class Airlock
    {
        private int _inner_button;
        private int _outer_button;
        private int _inside_button;

        public string Id { get; set; }
        public List<Vent> Vents { get; set; }
        public List<Door> InnerDoors { get; set; }
        public List<Door> OuterDoors { get; set; }

        public Airlock()
        {
        }
    }
}
