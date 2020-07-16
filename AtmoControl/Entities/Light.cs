using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    internal class Light : BaseEntity<IMyLightingBlock>
    {
        private Color _safe_colour = new Color(255, 255, 255);
        private Color _unsafe_colour = new Color(255, 0, 0);

        public enum LightMode
        {
            ON_OFF,
            WHITE_RED
        }

        public Light(IMyLightingBlock block) : base(block)
        {
            switch (GetIniString("Mode", "Red/White").ToLower())
            {
                case "red/white":
                case "white/red":
                    Mode = LightMode.WHITE_RED;
                    break;
                default:
                    Mode = LightMode.ON_OFF;
                    break;
            }

            SetUnsafe();
        }

        public LightMode Mode { get; set; }

        public bool Status { get; private set; }

        public void UpdateSafety(bool isSafe)
        {
            if (isSafe)
            {
                SetSafe();
            }
            else
            {
                SetUnsafe();
            }
        }

        public void SetUnsafe()
        {
            Status = true;
            switch (Mode)
            {
                case LightMode.ON_OFF:
                    _block.Enabled = true;
                    break;
                case LightMode.WHITE_RED:
                    _block.Color = _unsafe_colour;
                    break;
            }
        }

        public void SetSafe()
        {
            Status = false;
            switch (Mode)
            {
                case LightMode.ON_OFF:
                    _block.Enabled = false;
                    break;
                case LightMode.WHITE_RED:
                    _block.Color = _safe_colour;
                    break;
            }
        }
    }
}