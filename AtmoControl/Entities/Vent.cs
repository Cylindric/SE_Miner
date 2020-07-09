using SpaceEngineers.Game.ModAPI.Ingame;


namespace IngameScript
{
    class Vent : BaseEntity
    {
        private const float SAFE_O2_THRESHOLD = 0.98f;
        private const string SAFE_ROOM = "atmosphere";
        private IMyAirVent _block;

        public enum VentMode
        {
            NORMAL,
            AIRLOCK
        }

        public Vent(IMyAirVent block) : base(block)
        {
            _block = block;
            Room1 = GetIniString("Room");

            switch (GetIniString("Mode", "Normal").ToLower())
            {
                case "airlock":
                    Mode = VentMode.AIRLOCK;
                    break;
                default:
                    Mode = VentMode.NORMAL;
                    break;
            }
        }

        public string Room1 { get; set; }
        public VentMode Mode { get; set; }

        public float OxygenLevel
        {
            get
            {
                return (Room1 == SAFE_ROOM ? 1.0f : _block.GetOxygenLevel());
            }
        }

        /// <summary>
        /// A vent is considered "Safe" if the O2 level is above 95%
        /// and it is not set to depressurise.
        /// </summary>
        public bool Safe
        {
            get
            {
                if (Room1 == SAFE_ROOM)
                {
                    return true;
                }
                else
                {
                    return _block.Depressurize == false && OxygenLevel > SAFE_O2_THRESHOLD;
                }
            }
        }

        public bool Unsafe
        {
            get
            {
                return !Safe;
            }
        }
    }
}
