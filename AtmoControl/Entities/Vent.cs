using SpaceEngineers.Game.ModAPI.Ingame;

namespace IngameScript
{
    class Vent : BaseEntity
    {
        private IMyAirVent _block;

        public Vent(IMyAirVent block) : base(block)
        {
            _block = block;
            Room1 = GetIniString("Room");
        }

        public string Room1 { get; set; }

        public float OxygenLevel
        {
            get
            {
                return _block.GetOxygenLevel();
            }
        }

        public bool Safe
        {
            get
            {
                return OxygenLevel > 0.95;
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
