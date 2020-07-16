using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        public const string TAG = "[ATMO]";

        private readonly RoomController _rooms;
        private readonly Debug _debug;

        public Program()
        {
            _debug = new Debug(this);
            Runtime.UpdateFrequency = UpdateFrequency.Update10;
            _rooms = new RoomController(this, Me);
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            _rooms.Update(argument, updateSource);
        }
        
        public void Debug(string str, bool show = true)
        {
            if (show)
            {
                _debug.WriteLine(str);
            }
        }

    }
}
