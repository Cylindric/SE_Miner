using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class Door : BaseEntity
    {
        private IMyDoor _block;

        public enum DoorMode
        {
            AUTO_CLOSE,
            NORMALLY_OPEN
        }

        public Door(IMyDoor block) : base(block)
        {
            _block = block;
            Room1 = GetIniString("Room1");
            Room2 = GetIniString("Room2");
            Id = GetIniString("Id");
            NeedsClosing = true;
            WasSafe = true;

            switch (GetIniString("Mode", "AutoClose"))
            {
                case "Open":
                    Mode = DoorMode.NORMALLY_OPEN;
                    NeedsClosing = false;
                    break;
                default:
                    Mode = DoorMode.AUTO_CLOSE;
                    break;
            }
        }

        public string Id { get; set; }

        public string Room1 { get; set; }

        public string Room2 { get; set; }

        public DoorMode Mode { get; set; }

        public bool NeedsClosing { get; set; }
        public bool WasSafe { get; set; }

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
