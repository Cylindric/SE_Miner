using SpaceEngineers.Game.ModAPI.Ingame;


namespace IngameScript
{
    class Vent : BaseEntity<IMyAirVent>
    {
        private const float SAFE_O2_THRESHOLD = 0.98f;
        private const string SAFE_ROOM = "atmosphere";

        public Vent(IMyAirVent block) : base(block)
        {
        }

        public Room Room { get; set; }

        public float OxygenLevel
        {
            get
            {
                return (Room.Name == SAFE_ROOM ? 1.0f : _block.GetOxygenLevel());
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
                if (Room.Name == SAFE_ROOM)
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
