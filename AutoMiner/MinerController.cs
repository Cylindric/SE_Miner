using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage;

namespace IngameScript
{
    class MinerController
    {
        readonly Program _prog;
        readonly IMyGridTerminalSystem _grid;
        readonly List<MiningPiston> _pistons = new List<MiningPiston>();
        readonly List<MiningRotor> _rotors = new List<MiningRotor>();
        List<IMyShipDrill> _drills = new List<IMyShipDrill>();
        List<IMyFunctionalBlock> _containers = new List<IMyFunctionalBlock>();

        float _drill_speed = 0.1F;
        float _retract_speed = 1F;
        string _forward_piston_flag = "extend_to_drill";
        string _reverse_piston_flag = "retract_to_drill";
        string _rotor_flag = "drill_rotor";

        public MinerController(Program program)
        {
            _prog = program;
            _grid = _prog.GridTerminalSystem;
        }

        /// <summary>
        /// Finds all containers on the currently-connected grid.
        /// </summary>
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
            _pistons.Clear();
            foreach (var p in all_pistons)
            {
                Dictionary<string, string> blockData = Helpers.GetCustomData(p);

                if (blockData.ContainsKey(_forward_piston_flag))
                {
                    _prog.Debug($"Found piston {p.CustomName} (forwards)\n");
                    _pistons.Add(new MiningPiston(_prog, p, false));
                }
                else if (blockData.ContainsKey(_reverse_piston_flag))
                {
                    _prog.Debug($"Found piston {p.CustomName} (reverse)\n");
                    _pistons.Add(new MiningPiston(_prog, p, true));
                }
            }

            // Now go and set the default speed based on the total number of found pistons
            foreach(var p in _pistons)
            {
                p.AdvanceSpeed = _drill_speed / _pistons.Count;
                p.RetractSpeed = -(_retract_speed / _pistons.Count);
            }
        }

        public void FindAllRotors()
        {
            var all_rotors = new List<IMyMotorAdvancedStator>();
            _rotors.Clear();
            _grid.GetBlocksOfType(all_rotors);
            foreach (var r in all_rotors)
            {
                Dictionary<string, string> blockData = Helpers.GetCustomData(r);
                if (blockData.ContainsKey(_rotor_flag))
                {
                    _rotors.Add(new MiningRotor(r));
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
                distance += p.MaxPossibleDistanceFromHome;
            }
            return distance;
        }

        public float TotalPistonDistanceFromHome()
        {
            float distance = 0F;
            foreach (var p in _pistons)
            {
                distance += p.DistanceFromHome;
            }
            return distance;
        }

        public bool MinDepthReached()
        {
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
                all_at_endstop = all_at_endstop || p.MaxEndstopReached;
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
            return angle;
        }

        /// <summary>
        /// Attempt to determine how far the main rotors are from their Home positions.
        /// I'm not convinced this is currently working correctly in all situations, such
        /// as when rotors have a negative rate-of-turn.
        /// </summary>
        /// <returns>Angle in degrees from home</returns>
        public double RotorAngleFromEndstop()
        {
            float delta = 0F;

            foreach (var r in _rotors)
            {
                delta += r.AngleFromHome;
            }
            return Helpers.RadiansToDegrees(delta);
        }

        /// <summary>
        /// Returns true if the rotors are all homed.
        /// </summary>
        /// <returns>True if the rotors are all close to home.</returns>
        public bool RotorsAtEndstops()
        {
            if (RotorAngleFromEndstop() < 0.01)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Set the Drill-advance speed.
        /// Automatically extends or retracts pistons depending on defined 
        /// orientation.
        /// </summary>
        /// <param name="piston_speed">Target speed for all pistons</param>
        private void SetPistonVelocity(float piston_speed)
        {
            _prog.Echo($"Setting all pistons to {piston_speed}.");

            foreach (var p in _pistons)
            {
                p.AdvancePiston(piston_speed);
            }
        }

        public void EmergencyStop()
        {
            // TODO: this should probably apply the brakes.
            StopDrillPistons();
            DisableDrills();
            StopDrillsAndLimitRotors();
        }

        /// <summary>
        /// Starts the main drillhead advancement movement at default speed.
        /// Turns on the pistons.
        /// </summary>
        public void AdvanceDrillPistons()
        {
            foreach(var p in _pistons)
            {
                p.AutoAdvance();
            }
        }

        /// <summary>
        /// Stops all drillhead advancement movement.
        /// Pistons remain 'on'.
        /// </summary>
        public void StopDrillPistons()
        {
            foreach (var p in _pistons)
            {
                p.StopPiston();
            }
        }

        /// <summary>
        /// Starts hte main drillhead retraction movement at default speed.
        /// </summary>
        public void RetractDrillPistons()
        {
            foreach (var p in _pistons)
            {
                p.AutoRetract();
            }
        }

        /// <summary>
        /// Starts the main drilling activity.
        /// Turns on all drills.
        /// Sets rotor speed of all rotors and unlocks end-stops.
        /// </summary>
        public void StartDrilling()
        {
            foreach (var d in _drills)
            {
                d.Enabled = true;
            }
            foreach (var r in _rotors)
            {
                r.StartRotation();
            }
        }

        /// <summary>
        /// Start the slow retraction of the mining gantry.
        /// Usually used in the case of a detected drill stall.
        /// </summary>
        public void SlowRetractGantry()
        {
            foreach (var r in _pistons)
            {
                r.AdvancePiston(r.AdvanceSpeed * -0.5F);
            }
        }

        /// <summary>
        /// Stops all Drills.
        /// </summary>
        public void DisableDrills()
        {
            foreach (var d in _drills)
            {
                d.Enabled = false;
            }
        }

        /// <summary>
        /// Disable drills and enable all rotor end-stops.
        /// </summary>
        public void StopDrillsAndLimitRotors()
        {
            DisableDrills();
            foreach (var r in _rotors)
            {
                r.StartHoming();
            }
        }

        /// <summary>
        /// Set all rotors to stopped and locked.
        /// </summary>
        public void LockRotors()
        {
            foreach (var r in _rotors)
            {
                r.Lock();
            }
        }

        /// <summary>
        /// Calculate the percentage of storage currently in use.
        /// </summary>
        /// <returns>Consumed percentage</returns>
        public float GetUsedStoragePercentage()
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
            if (float.IsNaN(usedPercentage))
            {
                return 0F;
            }
            return usedPercentage;
        }

        /// <summary>
        /// Return a collection of every stored item of the specified type and 
        /// the currently stored amount.
        /// </summary>
        /// <param name="typeId">The type of object to include.</param>
        /// <returns>A collection of types and quantities.</returns>
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

        /// <summary>
        /// Return a collection of every type of ore and the currently stored 
        /// amount.
        /// </summary>
        /// <returns>A collection of ores and quantities.</returns>
        public Dictionary<string, float> GetOreCounts()
        {
            return GetItemCounts("MyObjectBuilder_Ore");
        }

        /// <summary>
        /// Return a collection of every type of ingot and the currently stored 
        /// amount.
        /// </summary>
        /// <returns>A collection of ingots and quantities.</returns>
        public Dictionary<string, float> GetIngotCounts()
        {
            return GetItemCounts("MyObjectBuilder_Ingot");
        }
    }
}