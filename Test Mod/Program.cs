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
using System.Diagnostics;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        enum State
        {
            Idle,
            Drilling,
            Full,
            Parking,
            EStop
        }

        MinerController _miner;
        IMyTextPanel _debug;

        State _current_state = State.Idle;
        string _last_button_action = string.Empty;

        
        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            _debug = (IMyTextPanel)GridTerminalSystem.GetBlockWithName("DEBUG");
            if (_debug != null)
            {
                _debug.ContentType = ContentType.TEXT_AND_IMAGE;
                Debug("Miner Loaded\n", false);
            }

            _miner = new MinerController(this);

            // Find the main deployment piston(s)
            _miner.FindAllPistons();

            // Find the main rotor(s)
            _miner.FindAllRotors();

            // Find the main drill(s)
            _miner.FindAllDrills();

            // Find out what state we should be in, if known
            var data = Storage;
            if(data.Length > 0)
            {
                _current_state = (State) Enum.Parse(typeof(State), data, true);
            }
            else
            {
                _current_state = State.Idle;
            }
            Debug($"Loaded state {_current_state} from storage.\n");

        }

        Queue<string> lines = new Queue<string>();
        private void Debug(string msg, bool append=true)
        {
            Echo(msg);

            if (_debug != null)
            {
                if (append == false)
                {
                    lines.Clear();
                }
                lines.Enqueue(msg);
                if(lines.Count > 10)
                {
                    lines.Dequeue();
                }

                bool first = true;
                foreach(var line in lines.ToArray())
                {
                    _debug.WriteText(msg, first==false);
                    first = false;
                }
            }
        }

        public void Save()
        {
            Storage = _current_state.ToString();
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // The main update loop. This gets triggered by the game every 100 ticks
            if(updateSource == UpdateType.Update100)
            {
                switch (_current_state)
                {
                    case State.Idle:
                        ProcessStateIdle();
                        break;
                    case State.Drilling:
                        ProcessStateDrilling();
                        break;
                    case State.Parking:
                        ProcessStateParking();
                        break;
                    case State.Full:
                        ProcessStateFull();
                        break;
                    case State.EStop:
                        ProcessStateStop();
                        break;
                }
            }

            // The manual intervention update. This gets triggered by someone pushing
            // a button.
            if(updateSource == UpdateType.Trigger)
            {
                Debug($"Got triggered! {argument}\n");
                _last_button_action = argument;
            }
        }


        /// <summary>
        /// State IDLE will transition:
        ///   * to DRILL if a "drill" button was pressed.
        ///   * to PARKING if the "park" button was pressed.
        ///   * to ESTOP if an emergency button was pressed.
        /// </summary>
        private void ProcessStateIdle()
        {
            if (_last_button_action == "stop" && _current_state != State.EStop)
            {
                Debug("Idle > EStop\n");
                _current_state = State.EStop;
                return;
            }

            if (_last_button_action == "drill" && _current_state != State.Drilling)
            {
                Debug("Idle > Drilling\n");
                _current_state = State.Drilling;
                return;
            }

            if (_last_button_action == "park" && _current_state != State.Parking)
            {
                Debug("Idle > Parking\n");
                _current_state = State.Parking;
                return;
            }

            Debug("Idle\n");
        }


        /// <summary>
        /// State DRILLING will transition:
        ///   * to FULL if the containers are all full.
        ///   * to PARKING if the hole is fully dug out.
        ///   * to PARKING if the "park" button was pressed.
        ///   * to ESTOP if the "stop" button was pressed.
        /// </summary>
        private void ProcessStateDrilling()
        {
            if (_last_button_action == "stop" && _current_state != State.EStop)
            {
                Debug("Drilling > EStop\n");
                _current_state = State.EStop;
                return;
            }

            if (_last_button_action == "park" && _current_state != State.Parking)
            {
                Debug("Drilling > Parking\n");
                _current_state = State.Parking;
                return;
            }

            if (_miner.StorageUsage() >= 0.999 && _current_state != State.Full)
            {
                Debug("Drilling > Full\n");
                _current_state = State.Full;
                return;
            }

            if(_miner.MaxDepthReached() && _current_state != State.Parking)
            {
                Debug("Drilling > Parking\n");
                _current_state = State.Parking;
                return;
            }

            Debug("Drilling\n");
            _miner.StartDrilling();
            _miner.AdvanceDrillPistons();
        }

        /// <summary>
        /// State FULL will transition:
        ///   * to DRILLING if the containers have capacity.
        ///   * to ESTOP if an emergency button was pressed.
        /// </summary>
        private void ProcessStateFull()
        {
            if (_last_button_action == "stop" && _current_state != State.EStop)
            {
                Debug("Full > EStop\n");
                _current_state = State.EStop;
                return;
            }

            if (_miner.StorageUsage() <= 0.9 && _current_state != State.Drilling)
            {
                Debug("Full > Drilling\n");
                _current_state = State.Drilling;
                return;
            }

            // Just wait for the containers to be emptied by the refineries.
            Debug("Full\n");
        }

        /// <summary>
        /// State PARKING will transition:
        ///   * to IDLE once the pistons and rotors have homed.
        ///   * to ESTOP if an emergency button was pressed.
        /// </summary>
        private void ProcessStateParking()
        {
            if (_last_button_action == "stop" && _current_state != State.EStop)
            {
                Debug("Parking > EStop\n");
                _current_state = State.EStop;
                return;
            }

            if (_miner.MinDepthReached() && _miner.RotorHasHomed() && _current_state != State.Idle)
            {
                Debug("Parking > Idle\n");
                _current_state = State.Idle;
                return;
            }

            Debug($"Parking ({_miner.TotalPistonDistanceFromHome()})\n");
        }

        /// <summary>
        /// State E-STOP does not transition.
        /// </summary>
        private void ProcessStateStop()
        {
            if (_last_button_action == "park" && _current_state != State.Parking)
            {
                Debug("EStop > Parking\n");
                _current_state = State.Parking;
                return;
            }

            _miner.EmergencyStop();
            Debug("ESTOP\n");
        }
    }
}
