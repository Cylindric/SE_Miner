using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class Door
    {
        private IMyDoor _block;

        public Door(IMyDoor block)
        {
            _block = block;
        }

        public string Room1 { get; set; }

        public string Room2 { get; set; }

    }
}
