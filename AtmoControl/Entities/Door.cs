using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class Door : BaseEntity<IMyDoor>
    {
        public enum DoorMode
        {
            AUTO_CLOSE,
            NORMALLY_OPEN
        }

        public Door(IMyDoor block) : base(block)
        {
            NeedsClosing = true;
            WasSafe = true;

            switch (GetIniString("Mode", "AutoClose").ToLower())
            {
                case "open":
                    Mode = DoorMode.NORMALLY_OPEN;
                    NeedsClosing = false;
                    break;
                default:
                    Mode = DoorMode.AUTO_CLOSE;
                    break;
            }
        }

        public Room Room1 { get; set; }

        public Room Room2 { get; set; }

        public DoorMode Mode { get; set; }

        public bool NeedsClosing { get; set; }
        public bool WasSafe { get; set; }
        public Sensor Sensor { get; set; }

        public float OpenRatio
        {
            get
            {
                return _block.OpenRatio;
            }
        }

        /// <summary>
        /// Returns true if both sides of the door are safe, otherwise returns false.
        /// If either side of the door is not defined, it is assumed to be unsafe.
        /// </summary>
        public bool IsSafe()
        {
            return (Room1?.IsSafe() ?? false) && (Room2?.IsSafe() ?? false);
        }

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
