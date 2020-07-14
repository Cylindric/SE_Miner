using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private readonly RoomController _rooms;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update10 | UpdateFrequency.Update100;

            _rooms = new RoomController(this, Me);
            _rooms.Update();
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _rooms.Update(updateSource);
        }

    }
}
