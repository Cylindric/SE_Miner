using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using System;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

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

        MyIni _ini = new MyIni();
        MinerController _miner;
        List<IMyTextPanel> _debugDisplays = new List<IMyTextPanel>();
        List<IMyTextPanel> _oreDisplays = new List<IMyTextPanel>();

        State _current_state = State.Idle;
        string _last_button_action = string.Empty;
        
        float _depth_at_last_tick = 0F;
        float _depth_at_this_tick = 0F;
        double _angle_at_last_tick = 0F;
        double _angle_at_this_tick = 0F;
        long _time_at_last_tick = 0;
        long _time_at_this_tick = 0;
       
        IMyTextSurface _drawingSurface;
        RectangleF _viewport;
        Display _display;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            MyIniParseResult result;
            if (!_ini.TryParse(Me.CustomData, out result))
                throw new Exception(result.ToString());

            _miner = new MinerController(this);

            // Me is the programmable block which is running this script.
            // Retrieve the Large Display, which is the first surface
            _drawingSurface = Me.GetSurface(0);

            // Calculate the viewport offset by centering the surface size onto the texture size
            _viewport = new RectangleF(
                (_drawingSurface.TextureSize - _drawingSurface.SurfaceSize) / 2f,
                _drawingSurface.SurfaceSize
            );
            _display = new Display();


            // Find all the panels to use
            var all_panels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(all_panels);

            // Find all the debug panels
            _debugDisplays = all_panels.Where(p => p.CustomData.Contains("[AutoMinerDebug]")).ToList();
            foreach(var d in _debugDisplays)
            {
                d.ContentType = ContentType.TEXT_AND_IMAGE;
                Debug("Miner Loaded\n");
            }

            // Find all the inventory panels
            _oreDisplays.Clear();
            _oreDisplays = all_panels.Where(p => p.CustomData.Contains("[AutoMinerInventory]")).ToList();
            foreach (var d in _oreDisplays)
            {
                d.ContentType = ContentType.TEXT_AND_IMAGE;
            }

            // Find the main deployment components
            _miner.FindAllPistons();
            _miner.FindAllRotors();
            _miner.FindAllDrills();
            _miner.FindAllContainers();

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
        public void Debug(string msg)
        {
            Echo(msg);

            foreach(var d in _debugDisplays)
            {
                lines.Enqueue(msg);
                while(lines.Count > 15)
                {
                    lines.Dequeue();
                }

                bool first = true;
                foreach (var line in lines.ToArray())
                {
                    d.WriteText(line, first == false);
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
            if (updateSource == UpdateType.Update100)
            {
                _time_at_this_tick = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                _depth_at_this_tick = _miner.TotalPistonDistanceFromHome();
                _angle_at_this_tick = _miner.TotalRotorAngle();

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

                UpdateOreDisplays();

                _depth_at_last_tick = _depth_at_this_tick;
                _angle_at_last_tick = _angle_at_this_tick;
                _time_at_last_tick = _time_at_this_tick;

                
            }

            // The manual intervention update. This gets triggered by someone pushing
            // a button.
            if (updateSource == UpdateType.Trigger)
            {
                Debug("Got triggered!\n");
                _last_button_action = argument;
            }

            // Begin a new UI frame
            var frame = _drawingSurface.DrawFrame();
            DrawSprites(ref frame);
            frame.Dispose();
        }

        public void UpdateOreDisplays()
        {
            var ore = _miner.GetOreCounts();
            string ore_text = string.Empty;
            foreach (var o in ore.OrderByDescending(x => x.Value).ThenBy(x => x.Key))
            {
                string displayValue = $"{o.Value:N0}kg";
                if (o.Value < 1)
                {
                    displayValue = $"{o.Value:N2}kg";
                }

                ore_text += $"{displayValue,12} {o.Key}\n";
            }

            var ingots = _miner.GetIngotCounts();
            string ingot_text = string.Empty;
            foreach (var o in ingots.OrderByDescending(x => x.Value).ThenBy(x => x.Key))
            {
                string displayValue = $"{o.Value:N0}kg";
                if (o.Value < 1)
                {
                    displayValue = $"{o.Value:N2}kg";
                }

                ingot_text += $"{displayValue,12} {o.Key}\n";
            }

            foreach (var d in _oreDisplays)
            {
                d.FontColor = new Color(0.27f, 0f, 0f);
                d.WriteText(ore_text);
                d.WriteText("\n\n", true);
                d.FontColor = new Color(0f, 0.27f, 0f);
                d.WriteText(ingot_text, true);
            }
        }

        public void DrawSprites(ref MySpriteDrawFrame frame)
        {
            // Display a bar for the current storage capacity
            var position = new Vector2(20, 20) + _viewport.Position;
            var value = _miner.GetUsedStoragePercentage();
            _display.DrawBar(ref frame, position, value);

            // Display a bar for the current depth
            position += new Vector2(0, 80);
            value = _miner.TotalPistonDistanceFromHome() / _miner.MaxPistonDistanceFromHome();
            _display.DrawBar(ref frame, position, value);

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
                _last_button_action = string.Empty;
                return;
            }

            if (_last_button_action == "drill" && _current_state != State.Drilling)
            {
                Debug("Idle > Drilling\n");
                _current_state = State.Drilling;
                _last_button_action = string.Empty;
                return;
            }

            if (_last_button_action == "park" && _current_state != State.Parking)
            {
                Debug("Idle > Parking\n");
                _current_state = State.Parking;
                _last_button_action = string.Empty;
                return;
            }

            Debug($"Idle ({_miner.TotalPistonDistanceFromHome():F2}m)\n");
            _miner.LockRotors();
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
                _last_button_action = string.Empty;
                return;
            }

            if (_last_button_action == "park" && _current_state != State.Parking)
            {
                Debug("Drilling > Parking\n");
                _current_state = State.Parking;
                _last_button_action = string.Empty;
                return;
            }

            if (_miner.GetUsedStoragePercentage() >= 0.999 && _current_state != State.Full)
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

            float depthDelta = _depth_at_this_tick - _depth_at_last_tick;

            // calculate the angle of motion since the last update
            double angleDelta = 0;
            if (_angle_at_last_tick < _angle_at_this_tick)
            {
                angleDelta = _angle_at_this_tick - _angle_at_last_tick;
            } 
            else
            {
                // overflow due to passing 0
                angleDelta = _angle_at_this_tick + (360 - _angle_at_last_tick);
            }

            // If stalled, slow down?
            bool stalled = false;
            if(angleDelta < 5)
            {
                _miner.SlowRetractGantry();
                stalled = true;
            } else
            {
                _miner.StartDrilling();
            }

            Debug($"Drilling... {_miner.TotalPistonDistanceFromHome():F2}m {angleDelta:F2}° {(stalled ? "(stalled)" : "")}\n");

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
                _last_button_action = string.Empty;
                return;
            }

            if (_miner.GetUsedStoragePercentage() <= 0.9 && _current_state != State.Drilling)
            {
                Debug("Full > Drilling\n");
                _current_state = State.Drilling;
                _last_button_action = string.Empty;
                return;
            }

            // Just wait for the containers to be emptied by the refineries.
            _miner.DisableDrills();
            Debug($"Full ({_miner.GetUsedStoragePercentage():F2})\n");
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
                _last_button_action = string.Empty;
                return;
            }

            if (_miner.MinDepthReached() && _miner.RotorsAtEndstops() && _current_state != State.Idle)
            {
                if (_miner.MinDepthReached())
                {
                    Debug($"Parking > Idle (Min depth reached @ {_miner.TotalPistonDistanceFromHome()})\n");
                }

                if (_miner.RotorsAtEndstops())
                {
                    Debug("Parking > Idle (Rotors at endstops)\n");
                }
                _current_state = State.Idle;
                return;
            }

            string message = $"Parking... {_miner.TotalPistonDistanceFromHome():F1}m {_miner.RotorAngleFromEndstop():F0}°";
            Debug($"{message}\n");
            _miner.RetractDrillPistons();
            _miner.DisableDrills();
            _miner.StopDrillsAndLimitRotors();
        }

        /// <summary>
        /// State E-STOP will transition:
        ///   * to PARKING if the park button was pressed.
        /// </summary>
        private void ProcessStateStop()
        {
            if (_last_button_action == "park" && _current_state != State.Parking)
            {
                Debug("EStop > Parking\n");
                _current_state = State.Parking;
                _last_button_action = string.Empty;
                return;
            }

            _miner.EmergencyStop();
            _current_state = State.Idle;
            Debug($"ESTOP ({_miner.TotalPistonDistanceFromHome():F2}m)\n");
        }
    }
}
