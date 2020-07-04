using Sandbox.Common.ObjectBuilders.Definitions;
using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    internal class Light : BaseEntity
    {
        private IMyLightingBlock _block;

        public Light(IMyLightingBlock block) : base(block)
        {
            _block = block;
            Room = GetIniString("Room");
            SetUnsafe();
        }

        public string Room { get; set; }

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
            _block.Enabled = true;
        }

        public void SetSafe()
        {
            _block.Enabled = false;
        }
    }
}