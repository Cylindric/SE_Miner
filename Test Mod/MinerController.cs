using EmptyKeys.UserInterface.Generated.StoreBlockView_Bindings;
using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage;

namespace IngameScript
{
    class MinerController
    {
        Program _prog;
        IMyGridTerminalSystem _grid;

        List<IMyExtendedPistonBase> _pistons = new List<IMyExtendedPistonBase>();
        List<IMyMotorAdvancedStator> _rotors = new List<IMyMotorAdvancedStator>();
        List<IMyShipDrill> _drills = new List<IMyShipDrill>();
        List<IMyFunctionalBlock> _containers = new List<IMyFunctionalBlock>();

        float _drill_speed = 0.1F;
        float _retract_speed = 1F;
        float _main_rotor_speed = 4F;
        float _main_rotor_max_lock = 0F;

        public MinerController(Program program)
        {
            _prog = program;
            _grid = _prog.GridTerminalSystem;
        }

        public void FindAllContainers()
        {
            var all_containers = new List<IMyFunctionalBlock>();
            _grid.GetBlocksOfType(all_containers);
            _containers.Clear();
            foreach (var c in all_containers)
            {
                if (c.HasInventory)
                {
                    _containers.Add(c);
                }
            }
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

                //_prog.Debug($"Name: {p.CustomName}\nMinLimit: {p.MinLimit}\nMaxLimit: {p.MaxLimit}\nMinLimit: {p.MinLimit}\nMaxLimit: {p.MaxLimit}\n");
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

        public float MaxPistonDistanceFromHome()
        {
            float distance = 0F;

            foreach (var p in _pistons)
            {
                Dictionary<string, string> blockData = Helpers.GetCustomData(p);

                if (blockData.ContainsKey("extend_to_drill"))
                {
                    distance += p.MinLimit;
                }
                else
                {
                    distance += p.MaxLimit;
                }
            }
            return distance;
        }

        public float TotalPistonDistanceFromHome()
        {
            float distance = 0F;

            foreach (var p in _pistons)
            {
                Dictionary<string, string> blockData = Helpers.GetCustomData(p);

                if (blockData.ContainsKey("extend_to_drill"))
                {
                    distance += (p.CurrentPosition - p.MinLimit);
                }
                else
                {
                    distance += (p.MaxLimit - p.CurrentPosition);
                }
            }
            return distance;
        }

        public bool MinDepthReached()
        {
            //bool all_at_endstop = false;
            if (TotalPistonDistanceFromHome() == 0)
            {
                return true;
            }
            return false;
        }

        public bool MaxDepthReached()
        {
            bool all_at_endstop = false;
            foreach (var p in _pistons)
            {
                Dictionary<string, string> blockData = Helpers.GetCustomData(p);

                bool at_endstop;
                if (blockData.ContainsKey("extend_to_drill"))
                {
                    at_endstop = (p.CurrentPosition >= p.MaxLimit);
                }
                else
                {
                    at_endstop = (p.CurrentPosition <= p.MinLimit);
                }
                all_at_endstop = all_at_endstop || at_endstop;
            }

            return all_at_endstop;
        }

        public double TotalRotorAngle()
        {
            float angle = 0F;
            foreach (var r in _rotors)
            {
                angle += r.Angle;
            }
            return Helpers.RadiansToDegrees(angle);
        }

        public double RotorAngleFromEndstop()
        {
            float delta = 0F;

            foreach (var r in _rotors)
            {
                string message = $"{r.CustomName}: rpm:{r.TargetVelocityRPM:F1} a:{r.Angle:F2} ";

                if (r.UpperLimitRad <= 10000)
                {
                    // Upper rotation limit set, implies clockwise
                    //message += $"u:{r.UpperLimitRad:F2} ";
                    float delta1 = r.UpperLimitRad - r.Angle;
                    //message += $"d1:{delta1:F2} ";
                    delta += delta1;
                }
                else if (r.LowerLimitRad >= 1000)
                {
                    // Lower rotation limit set, implies anticlockwise
                    //message += $"l:{r.LowerLimitRad:F2} ";
                    float delta2 = r.Angle - r.LowerLimitRad;
                    //message += $"d2:{delta2:F2} ";
                    delta += delta2;
                }

                message += $"d:{delta:F2} {Helpers.RadiansToDegrees(delta):F2}°";

                //_prog.Debug(message);
            }
            return Helpers.RadiansToDegrees(delta);
        }

        public bool RotorsAtEndstops()
        {
            if (RotorAngleFromEndstop() < 0.01)
            {
                return true;
            }
            return false;
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
            DisableDrills();
            DisengageRotors();
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
                r.RotorLock = false;
                r.TargetVelocityRPM = _main_rotor_speed;
                r.LowerLimitDeg = float.MinValue;
                r.UpperLimitDeg = float.MaxValue;
                r.Enabled = true;
            }
        }

        public void RotorStallDetected()
        {
            //var min_speed = _drill_speed * 0.1;
            //var speed_step = _drill_speed * 0.1;
            float reverse_speed = _drill_speed * -0.25F;

            foreach (var r in _pistons)
            {
                r.Velocity = reverse_speed; // (float)Math.Max((_drill_speed - speed_step), min_speed);
            }
        }
        public void RotorStallCleared()
        {
            foreach (var r in _pistons)
            {
                r.Velocity = _drill_speed;
            }
        }

        public void DisableDrills()
        {
            foreach (var d in _drills)
            {
                d.Enabled = false;
            }
        }

        public void DisengageRotors()
        {
            DisableDrills();
            foreach (var r in _rotors)
            {
                r.LowerLimitDeg = float.MinValue;
                r.UpperLimitDeg = _main_rotor_max_lock;
                r.Enabled = true;
            }
        }

        public void LockRotors()
        {
            foreach (var r in _rotors)
            {
                r.TargetVelocityRPM = 0;
                r.RotorLock = true;
            }
        }

        public float GetAvailableStorageSpace()
        {
            MyFixedPoint free_volume = 0;
            MyFixedPoint total_volume = 0;
            MyFixedPoint current_volume = 0;

            foreach (var c in _containers)
            {
                for (int i = 0; i < c.InventoryCount; i++)
                {
                    var inv = c.GetInventory(i);
                    total_volume += inv.MaxVolume;
                    current_volume += inv.CurrentVolume;
                    free_volume += (inv.MaxVolume - inv.CurrentVolume);
                }
            }
            float usedPercentage = ((float)current_volume / (float)total_volume);
            if (usedPercentage != usedPercentage)
            {
                return 0F;
            }
            return usedPercentage;
        }

        public float GetPistonRatio(IMyExtendedPistonBase p)
        {
            var blockData = Helpers.GetCustomData(p);

            if (blockData.ContainsKey("extend_to_drill"))
            {
                // Extention pistons are considered "zero" extension when at their lowest allowed position
                return p.CurrentPosition - p.MinLimit;
            }
            else
            {
                // Contraction pistons are considered "zero" when at their maximum allowed position
                return p.MaxLimit - p.CurrentPosition;
            }
        }

        private Dictionary<string, float> GetItemCounts(string typeId)
        {
            Dictionary<string, float> itemlist = new Dictionary<string, float>();

            foreach (var c in _containers)
            {
                for (int i = 0; i < c.InventoryCount; i++)
                {
                    var inv = c.GetInventory(i);
                    List<VRage.Game.ModAPI.Ingame.MyInventoryItem> items = new List<VRage.Game.ModAPI.Ingame.MyInventoryItem>();
                    inv.GetItems(items);
                    foreach (var item in items.Where(x => x.Type.TypeId == typeId))
                    {
                        string ore_type = item.Type.SubtypeId;

                        if (!itemlist.ContainsKey(ore_type))
                        {
                            itemlist.Add(item.Type.SubtypeId, 0);
                        }
                        itemlist[ore_type] += (float)item.Amount;
                    }
                }
            }
            return itemlist;
        }

        public Dictionary<string, float> GetOreCounts()
        {
            return GetItemCounts("MyObjectBuilder_Ore");
        }

        public Dictionary<string, float> GetIngotCounts()
        {
            return GetItemCounts("MyObjectBuilder_Ingot");
        }
    }
}