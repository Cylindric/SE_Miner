using System.Collections.Generic;

namespace IngameScript
{
    class Room
    {
        public Room(string name)
        {
            Name = name;
            Vents = new List<Vent>();
            Doors = new List<Door>();
            Displays = new List<Display>();
        }

        public string Name { get; set; }
        public List<Vent> Vents { get; internal set; }
        public List<Door> Doors { get; internal set; }
        public List<Display> Displays { get; internal set; }

    }
}
