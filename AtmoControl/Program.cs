using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        private VentController _vents;
        private DoorController _doors;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _vents = new VentController(this);
            _vents.DiscoverVents();

            _doors = new DoorController(this);
            _doors.DiscoverDoors();
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main update loop. This gets triggered by the game every 100 ticks
            if (updateSource == UpdateType.Update100)
            {
                _vents.Update();
                _doors.Update();
            }
        }

    }
}
