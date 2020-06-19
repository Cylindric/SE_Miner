using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    class MinerController
    {
        Program _prog;
        IMyGridTerminalSystem _grid;
        
        List<IMyExtendedPistonBase> _pistons = new List<IMyExtendedPistonBase>();
        List<IMyMotorAdvancedStator> _rotors = new List<IMyMotorAdvancedStator>();
        List<IMyShipDrill> _drills = new List<IMyShipDrill>();

        float _drill_speed = 0.2F;
        float _retract_speed = 1F;
        float _main_rotor_speed = 1F;
        float _main_rotor_max_lock = 0F;

        public MinerController(Program program)
        {
            _prog = program;
            _grid = _prog.GridTerminalSystem;
        }

        public void FindAllPistons()
        {
            var all_pistons = new List<IMyExtendedPistonBase>();
            _grid.GetBlocksOfType(all_pistons);
            foreach (var p in all_pistons)
            {
                Dictionary<string, string> blockData = Helpers.GetCustomData(p);
                if (blockData.ContainsKey("extend_to_drill") || blockData.ContainsKey("retract_to_drill"))
                {
                    _pistons.Add(p);
                }
            }
        }

        public void FindAllRotors()
        {
            var all_rotors = new List<IMyMotorAdvancedStator>();
            _grid.GetBlocksOfType(all_rotors);
            foreach (var r in all_rotors)
            {
                Dictionary<string, string> blockData = Helpers.GetCustomData(r);
                if (blockData.ContainsKey("drill_rotor"))
                {
                    _rotors.Add(r);
                }
            }
        }

        public void FindAllDrills()
        {
            _grid.GetBlocksOfType(_drills);
        }

        public float TotalPistonDistanceFromHome()
        {
            float distance = 0F;

            foreach (var p in _pistons)
            {
                Dictionary<string, string> blockData = Helpers.GetCustomData(p);

                if (blockData.ContainsKey("extend_to_drill"))
                {
                    distance += (p.CurrentPosition - p.LowestPosition);
                }
                else
                {
                    distance += (p.HighestPosition - p.CurrentPosition);
                }
            }
            return distance;
        }

        public bool MinDepthReached()
        {
            bool at_endstop = false;

            foreach (var p in _pistons)
            {
                Dictionary<string, string> blockData = Helpers.GetCustomData(p);

                if (blockData.ContainsKey("extend_to_drill"))
                {
                    at_endstop = (p.CurrentPosition <= p.MinLimit);
                    if (at_endstop)
                    {
                        _prog.Debug($"Piston {p.CustomName} is at low endstop {p.MinLimit} ({p.CurrentPosition}).");
                    }
                }
                else
                {
                    at_endstop = (p.CurrentPosition >= p.MaxLimit);
                    if (at_endstop)
                    {
                        _prog.Debug($"Piston {p.CustomName} is at high endstop {p.MaxLimit} ({p.CurrentPosition}).");
                    }
                }
            }

            return at_endstop;
        }

        public bool MaxDepthReached()
        {
            bool at_endstop = false;

            foreach (var p in _pistons)
            {
                Dictionary<string, string> blockData = Helpers.GetCustomData(p);

                if (blockData.ContainsKey("extend_to_drill"))
                {
                    at_endstop = (p.CurrentPosition >= p.MaxLimit);
                    if (at_endstop)
                    {
                        _prog.Debug($"Piston {p.CustomName} is at high endstop {p.MaxLimit} ({p.CurrentPosition}).");
                    }
                }
                else
                {
                    at_endstop = (p.CurrentPosition <= p.MinLimit);
                    if (at_endstop)
                    {
                        _prog.Debug($"Piston {p.CustomName} is at low endstop {p.MinLimit} ({p.CurrentPosition}).");
                    }
                }
            }

            return at_endstop;
        }

        public float StorageUsage()
        {
            // TODO: Implement storage checking
            return 0F;
        }

        public bool RotorHasHomed()
        {
            // TODO: check if the rotor is at/near home.
            return true;
        }

        private void SetPistonVelocity(float piston_speed)
        {
            _prog.Echo($"Setting all pistons to {piston_speed}.");

            foreach (var p in _pistons)
            {
                Dictionary<string, string> blockData = Helpers.GetCustomData(p);

                if (blockData.ContainsKey("extend_to_drill"))
                {
                    p.Velocity = piston_speed;
                }
                else
                {
                    p.Velocity = piston_speed * -1;
                }

                p.Enabled = true;
            }
        }

        public void EmergencyStop()
        {
            // TODO: this should probably apply the brakes.
            StopDrillPistons();
            StopDrilling();
        }

        /// <summary>
        /// Starts the main drillhead advancement movement.
        /// Turns on the pistons.
        /// </summary>
        public void AdvanceDrillPistons()
        {
            var piston_speed = _drill_speed / _pistons.Count();
            _prog.Echo($"Setting {_pistons.Count} pistons to {piston_speed} to reach {_drill_speed}.");
            SetPistonVelocity(piston_speed);
        }

        public void StopDrillPistons()
        {
            var piston_speed = 0;
            _prog.Echo($"Setting {_pistons.Count} pistons to {piston_speed} to reach {_drill_speed}.");
            SetPistonVelocity(piston_speed);
        }

        public void RetractDrillPistons()
        {
            var piston_speed = (_retract_speed / _pistons.Count()) * -1;
            _prog.Echo($"Setting {_pistons.Count} pistons to {piston_speed} to reach {_drill_speed}.");
            SetPistonVelocity(piston_speed);
        }

        /// <summary>
        /// Starts the main drilling activity.
        /// Turns on rotors and drills
        /// </summary>
        public void StartDrilling()
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

        public void StopDrilling()
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
