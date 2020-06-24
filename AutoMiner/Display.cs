using System;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    class Display
    {
        public void DrawBar(ref MySpriteDrawFrame frame, Vector2 position, float value)
        {
            // Display 10 lines of text by 20 columns
            int bar_width = 21;
            var filled_blocks = (int)(bar_width * value);
            var empty_blocks = Math.Max(bar_width - filled_blocks, 0);

            var bar_top = new string('═', bar_width);
            var bar_mid = new string(' ', bar_width);
            var bar_bot = new string('═', bar_width);
            var bar_filled = new string('▓', filled_blocks);
            var bar_empty = new string(' ', empty_blocks);

            var line1 = $"╔{bar_top}╗\n";
            var line2 = $"║{bar_filled}{bar_empty}║ {value:P}\n";
            var line3 = $"╚{bar_top}╝";

            // Create our first bar
            var bar_outline = new MySprite()
            {
                Type = SpriteType.TEXT,
                Data = $"{line1}{line2}{line3}",
                Position = position,
                RotationOrScale = 0.8f /* 80 % of the font's default size */,
                Color = Color.Red,
                Alignment = TextAlignment.LEFT /* Center the text on the position */,
                FontId = "Monospace"
            };
            // Add the sprite to the frame
            frame.Add(bar_outline);
        }
    }
}
