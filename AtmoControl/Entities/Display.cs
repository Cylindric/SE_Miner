using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRageMath;

namespace IngameScript
{
    internal class Display : BaseEntity
    {
        private readonly IMyTextPanel _block;
        private Color _safe_bg = new Color(0, 40, 0);
        private Color _safe_fg = new Color(0, 140, 0);
        private Color _unsafe_bg = new Color(40, 0, 0);
        private Color _unsafe_fg = new Color(255, 255, 0);

        public enum DisplayType
        {
            DOOR_SIGN,
            ROOMS_SIGN
        }

        public Display(IMyTextPanel block) : base(block)
        {
            _block = block;

            UnsafeTitle = GetIniString("UnsafeMessage", "DANGER!");
            FontSize = GetIniFloat("FontSize", 10f);
            SafeTitle = GetIniString("SafeMessage", string.Empty);

            switch (GetIniString("Mode", "DoorSign"))
            {
                case "RoomsDisplay":
                    Mode = DisplayType.ROOMS_SIGN;
                    break;
                default:
                    Mode = DisplayType.DOOR_SIGN;
                    break;
            }

            IgnoreRooms = new List<string>();
            List<string> ignoreList = GetIniString("IgnoreRooms", "").Split(',').ToList();
            foreach(var name in ignoreList)
            {
                IgnoreRooms.Add(name.Trim().ToLower());
            }


            _block.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
            switch (Mode)
            {
                case DisplayType.ROOMS_SIGN:
                    _block.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.LEFT;
                    break;
                case DisplayType.DOOR_SIGN:
                    _block.FontSize = FontSize;
                    _block.Alignment = VRage.Game.GUI.TextPanel.TextAlignment.CENTER;
                    SetUnsafe();
                    break;
            }
        }

        public string Room { get; set; }

        public DisplayType Mode { get; set; }

        public string SafeTitle { get; set; }

        public string UnsafeTitle { get; set; }

        public List<string> IgnoreRooms { get; set; }

        public float FontSize { get; set; }

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
            SetDoorDisplay(UnsafeTitle ?? Room, _unsafe_bg, _unsafe_fg);
        }

        public void SetSafe()
        {
            SetDoorDisplay(SafeTitle ?? Room, _safe_bg, _safe_fg);
        }

        public void UpdateRoomsDisplay(Dictionary<string, Room> rooms)
        {
            int longestRoomName = 0;
            int longestStatus = 6;

            // loop once to get the layout info we need
            foreach(var room in rooms.Where(r => IgnoreRooms.All(p2 => p2 != r.Key.ToLower())))
            {
                if (room.Key.Length > longestRoomName)
                    longestRoomName = room.Key.Length;
            }

            // loop again to format the output
            string str = "";
            bool allRoomSafe = true;
            foreach (var room in rooms.Where(r => IgnoreRooms.All(p2 => p2 != r.Key.ToLower())).OrderBy(r => r.Key))
            {
                string safety;
                if (room.Value.IsSafe())
                {
                    safety = "OK".PadLeft(longestStatus, ' ');
                }
                else
                {
                    // Space is always unsafe, so don't make that mark the whole screen as bad
                    if (room.Key.ToLower() != "space")
                        allRoomSafe = false;

                    safety = "Unsafe".PadLeft(longestStatus, ' ');
                }
                str += $"{room.Key.PadRight(longestRoomName, ' ')} {safety}\n";
            }
            if (allRoomSafe)
            {
                _block.BackgroundColor = _safe_bg;
            }
            else
            {
                _block.BackgroundColor = _unsafe_bg;
            }
            _block.WriteText(str);
        }

        private void SetDoorDisplay(string str, Color bg, Color fg)
        {
            _block.WriteText(str);
            _block.BackgroundColor = bg;
            _block.FontColor = fg;
        }
    }
}