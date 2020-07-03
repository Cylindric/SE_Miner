using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private RoomController _rooms;
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _rooms = new RoomController(this);
            _rooms.Update();
            Echo(_rooms.Debug());
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main update loop. This gets triggered by the game every 100 ticks
            if (updateSource == UpdateType.Update100)
            {
                _rooms.Update();
            }
        }

    }
}
