﻿using Sandbox.ModAPI.Ingame;
using VRageMath;

namespace IngameScript
{
    internal class Display
    {
        private IMyTextPanel _block;
        private Color _safe_bg = new Color(0, 40, 0);
        private Color _safe_fg = new Color(0, 140, 0);
        private Color _unsafe_bg = new Color(40, 0, 0);
        private Color _unsafe_fg = new Color(255, 255, 0);

        public Display(IMyTextPanel block)
        {
            _block = block;
            _block.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
            _block.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            _block.FontSize = 10;
            SetUnsafe();
        }

        public string Room1 { get; set; }

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
            SetDisplay("DANGER", _unsafe_bg, _unsafe_fg);
        }

        public void SetSafe()
        {
            SetDisplay("Safe", _safe_bg, _safe_fg);
        }

        private void SetDisplay(string str, Color bg, Color fg)
        {
            _block.WriteText(str);
            _block.BackgroundColor = bg;
            _block.ScriptForegroundColor = fg;
        }
    }
}