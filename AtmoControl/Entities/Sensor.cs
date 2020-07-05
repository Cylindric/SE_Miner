using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    internal class Sensor : BaseEntity
    {
        private IMySensorBlock _block;

        public Sensor(IMySensorBlock block) : base(block)
        {
            _block = block;
            Link = GetIniString("Link");
            Position = GetIniString("Position");
            
            _block.DetectAsteroids = false;
            _block.DetectEnemy = false;
            _block.DetectFloatingObjects = false;
            _block.DetectLargeShips = false;
            _block.DetectNeutral = GetIniBool("DetectNeutral", true);
            _block.DetectPlayers = true;
            _block.DetectSmallShips = false;
            _block.DetectStations = false;
            _block.DetectSubgrids = false;

            switch (Position)
            {
                case "Right":
                    _block.LeftExtend = 0.1f;
                    _block.RightExtend = 5.0f;
                    _block.BottomExtend = 3.0f;
                    _block.TopExtend = 0.1f;
                    _block.BackExtend = 4.0f;
                    _block.FrontExtend = 3.0f;
                    break;
                case "Above Right":
                    _block.LeftExtend = 0.1f;
                    _block.RightExtend = 5.0f;
                    _block.BottomExtend = 4.0f;
                    _block.TopExtend = 0.1f;
                    _block.BackExtend = 4.0f;
                    _block.FrontExtend = 3.0f;
                    break;
                case "Left Front":
                    _block.LeftExtend = 5.0f;
                    _block.RightExtend = 1.0f;
                    _block.BottomExtend = 1.0f;
                    _block.TopExtend = 1.0f;
                    _block.BackExtend = 0.1f;
                    _block.FrontExtend = 3.0f;
                    break;
                case "Custom":
                    _block.LeftExtend = GetIniFloat("Left", 1.0f);
                    _block.RightExtend = GetIniFloat("Right", 1.0f);
                    _block.BottomExtend = GetIniFloat("Bottom", 1.0f);
                    _block.TopExtend = GetIniFloat("Top", 1.0f);
                    _block.BackExtend = GetIniFloat("Back", 1.0f);
                    _block.FrontExtend = GetIniFloat("Front", 1.0f);
                    break;
                case "Manual":
                    // Just leave it as it is.
                    break;

            }
        }

        public string Link { get; set; }

        public string Position { get; set; }

        public bool IsActive
        {
            get
            {
                return _block.IsActive;
            }
        }

    }
}