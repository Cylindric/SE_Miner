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

        /// <summary>
        /// Finds every piston on the grid with custom data value of "extend_to_drill" or "retract_to_drill".
        /// Finds every rotor on the grid with custom data value of "drill_rotor".
        /// Finds every drill on the grid.
        /// When drilling, will retract all "retract_to_drill" pistons, and extend all "extend_to_drill" pistons, at "drill_speed".
        /// When retracting will retract all "extend_to_drill" pistons, then extend "retract_to_drill" pistons, at "drill_speed".
        /// Stops drilling at "max_depth".
        /// </summary>

        List<IMyExtendedPistonBase> _pistons = new List<IMyExtendedPistonBase>();
        List<IMyShipDrill> _drills = new List<IMyShipDrill>();
        List<IMyMotorAdvancedStator> _rotors = new List<IMyMotorAdvancedStator>();

        float _drill_speed = 1F;
        float _retract_speed = 1F;
        //float _max_depth = 1F;
        float _main_rotor_speed = 1F;
        float _main_rotor_max_lock = 0F;

        String _state = "idle";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="block"></param>
        /// <returns></returns>
        Dictionary<string, string> GetCustomData(IMyTerminalBlock block)
        {
            var data = new Dictionary<string, string>();
            var customData = block.CustomData;

            if(string.IsNullOrEmpty(customData))
            {
                return data;
            }
            if (customData.Contains(";"))
            {
                foreach (string v in customData.Split(';'))
                {
                    if (v.Contains("="))
                    {
                        data.Add(v, "1");
                    }
                    else
                    {
                        data.Add(v.Substring(0, v.IndexOf("=")), v.Substring(v.IndexOf("=")));
                    }
                }
            }
            else
            {
                data.Add(customData, "1");
            }
            return data;
        }
        
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            // Find the main deployment piston(s)
            var all_pistons = new List<IMyExtendedPistonBase>();
            GridTerminalSystem.GetBlocksOfType(all_pistons);
            foreach (var p in all_pistons)
            {
                Dictionary<string, string> blockData = GetCustomData(p);
                if(blockData.ContainsKey("extend_to_drill") || blockData.ContainsKey("retract_to_drill"))
                {
                    Echo($"Piston {p.CustomName}. Max {p.MaxVelocity}. Vel {p.Velocity}");
                    _pistons.Add(p);
                }
            }

            // Find the main rotor(s)
            var all_rotors = new List<IMyMotorAdvancedStator>();
            GridTerminalSystem.GetBlocksOfType(all_rotors);
            foreach (var r in all_rotors)
            {
                Dictionary<string, string> blockData = GetCustomData(r);
                if (blockData.ContainsKey("drill_rotor"))
                {
                    _rotors.Add(r);
                }
            }

            // Find the main drill(s)
            GridTerminalSystem.GetBlocksOfType(_drills);

            // Find out what state we should be in, if known
            var data = Storage;
            if(data.Length > 0)
            {
                _state = data;
            }
            else
            {
                _state = "idle";
            }
            Echo($"Current Mining state is {_state}.");
        }

        public void Save()
        {
            Storage = _state;
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if(updateSource == UpdateType.Update100)
            {
                switch (_state)
                {
                    case "idle":
                        break;
                    case "drilling":
                        break;
                    case "retracting":
                        break;
                }
            }

            if(updateSource == UpdateType.Trigger)
            {
                Echo($"Got triggered! {argument}");
                switch (argument)
                {
                    case "drill":
                        StartDrilling();
                        AdvanceDrillPistons();
                        break;

                    case "stop":
                        StopDrilling();
                        StopDrillPistons();
                        break;

                    case "retract":
                        StopDrilling();
                        RetractDrillPistons();
                        break;
                }
            }
        }


        private void SetPistonVelocity(float piston_speed)
        {
            Echo($"Setting all pistons to {piston_speed}.");

            foreach (var p in _pistons)
            {
                Dictionary<string, string> blockData = GetCustomData(p);

                if (blockData.ContainsKey("extend_to_drill"))
                {
                    p.Velocity = piston_speed;
                } else
                {
                    p.Velocity = piston_speed * -1;
                }

                p.Enabled = true;
            }
        }
        private void AdvanceDrillPistons()
        {
            var piston_speed = _drill_speed / _pistons.Count();
            Echo($"Setting {_pistons.Count} pistons to {piston_speed} to reach {_drill_speed}.");
            SetPistonVelocity(piston_speed);
        }

        private void StopDrillPistons()
        {
            var piston_speed = 0;
            Echo($"Setting {_pistons.Count} pistons to {piston_speed} to reach {_drill_speed}.");
            SetPistonVelocity(piston_speed);
        }

        private void RetractDrillPistons()
        {
            var piston_speed = (_retract_speed / _pistons.Count()) * -1;
            Echo($"Setting {_pistons.Count} pistons to {piston_speed} to reach {_drill_speed}.");
            SetPistonVelocity(piston_speed);
        }

        private void StartDrilling()
        {
            foreach (var d in _drills)
            {
                d.Enabled = true;
            }
            foreach (var r in _rotors)
            {
                r.TargetVelocityRPM = _main_rotor_speed;
                r.LowerLimitDeg = float.MinValue;
                r.UpperLimitDeg = float.MaxValue;
                r.Enabled = true;
            }
        }

        private void StopDrilling()
        {
            foreach (var d in _drills)
            {
                d.Enabled = false;
            }
            foreach (var r in _rotors)
            {
                r.LowerLimitDeg = float.MinValue;
                r.UpperLimitDeg = _main_rotor_max_lock;
                r.Enabled = true;
            }
        }
    }
}
