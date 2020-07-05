using EmptyKeys.UserInterface.Generated.DataTemplatesContracts_Bindings;
using System.Collections.Generic;
using System.Linq;

namespace IngameScript
{
    class Room
    {
        private const string SAFE_ROOM = "atmosphere";

        public Room(string name)
        {
            Name = name;
            Vents = new List<Vent>();
            Doors = new List<Door>();
            Displays = new List<Display>();
        }

        // Vents report the atmosphere as being unpressurised,
        // so if a room is marked as being connected to the atmoshpere
        // always assume it is safe.
        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                AlwaysSafe = (_name == SAFE_ROOM);
            }
        }

        public List<Vent> Vents { get; internal set; }
        public List<Door> Doors { get; internal set; }
        public List<Display> Displays { get; internal set; }
        public bool AlwaysSafe { get; set; }

        public bool IsSafe()
        {
            if (AlwaysSafe)
                return true;

            foreach (var vent in Vents)
            {
                if (!vent.Safe)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
