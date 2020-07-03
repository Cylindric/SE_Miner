using System.Collections.Generic;

namespace IngameScript
{
    class Room
    {
        private string _name;

        public Room(string name)
        {
            _name = name;
            Vents = new List<Vent>();
            Doors = new List<Door>();
            Displays = new List<Display>();
        }

        public List<Vent> Vents { get; internal set; }
        public List<Door> Doors { get; internal set; }
        public List<Display> Displays { get; internal set; }
    }
}
