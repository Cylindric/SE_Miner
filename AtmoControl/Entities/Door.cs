using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class Door : BaseEntity
    {
        private IMyDoor _block;

        public Door(IMyDoor block) : base(block)
        {
            _block = block;
            Room1 = GetIniString("Room1");
            Room2 = GetIniString("Room2");
            Id = GetIniString("Id");
        }

        public string Id { get; set; }

        public string Room1 { get; set; }

        public string Room2 { get; set; }

        public void Open()
        {
            _block.OpenDoor();
        }

        public void Close()
        {
            _block.CloseDoor();
        }

    }
}
