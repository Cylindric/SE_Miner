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
using Sandbox.Game.GUI;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        readonly string VerticalPistonTag = "[OreDockVertical]";
        readonly string HorizontalPistonTag = "[OreDockHorizontal]";
        readonly string ConnectorTag = "[OreDockConnector]";
        readonly string DebugTag = "[OreDockDebug]";
        readonly float DockClearance = 2;

        readonly List<IMyExtendedPistonBase> _VerticalPistons = new List<IMyExtendedPistonBase>();
        readonly List<IMyExtendedPistonBase> _HorizontalPistons = new List<IMyExtendedPistonBase>();
        readonly List<IMyShipConnector> _Connectors = new List<IMyShipConnector>();
        private readonly Debug _debug;
        private State _CurrentState = State.IDLE;

        private enum State
        {
            IDLE,
            DEPLOY_START,
            DEPLOY_OUT,
            // DEPLOY_UP,
            DEPLOY_CONNECT,
            RETRACT_DISCONNECT,
            RETRACT_INITIAL,
            RETRACT_BACK,
            RETRACT_DOWN
        }


        public Program()
        {
            _debug = new Debug(this, DebugTag);
            _debug.WriteLine("Starting Docking Control...");
            Runtime.UpdateFrequency = UpdateFrequency.Update100;
            FindAllFunctionalBlocks();
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            // DEPLOY
            // 1. EXTEND Vertical piston to min+2m to clear the dock
            // 2. EXTEND Horizontal piston to max
            // 3. EXTEND Vertical piston to max
            // 4. CONNECT docking connector

            // UNDEPLOY
            // 1. DISCONNECT docking connector
            // 2. RETRACT Vertical piston to min+2m
            // 3. RETRACT Horizontal piston to min
            // 4. RETRACT Vertical piston to min

            if(updateSource == UpdateType.Trigger)
            {
                _debug.WriteLine($"Triggered with parameter [{argument}] while [{_CurrentState}]");

                if(_CurrentState == State.IDLE && argument == "deploy")
                {
                    _CurrentState = State.DEPLOY_START;
                }
            }

            if (updateSource == UpdateType.Update100)
            {
                switch (_CurrentState)
                {
                    case State.IDLE:
                        break;
                    case State.DEPLOY_START:
                        _CurrentState = StateDeployStart();
                        break;
                    case State.DEPLOY_OUT:
                        _CurrentState = StateDeployOut();
                        break;
                    case State.DEPLOY_CONNECT:
                        _CurrentState = StateDeployConnect();
                        break;
                }
            }
        }

        private State StateDeployStart()
        {
            // 1. EXTEND Vertical piston to min+2m to clear the dock
            foreach (var p in _VerticalPistons) {
                _debug.WriteLine($"Piston ({p.CurrentPosition} < {p.MinLimit + DockClearance})");
                if (p.CurrentPosition < p.MinLimit + DockClearance) {
                    _debug.WriteLine($"Piston clearing dock... ({p.CurrentPosition} < {p.MinLimit + DockClearance})");
                    p.Velocity = Math.Abs(p.Velocity);
                }
                else
                {
                    _debug.WriteLine($"Piston cleared dock... ({p.CurrentPosition} >= {p.MinLimit + DockClearance})");
                    return State.DEPLOY_OUT;
                }
            }
            return State.DEPLOY_START;
        }

        private State StateDeployOut()
        {
            // 2. EXTEND Horizontal piston to max
            // 3. EXTEND Vertical piston to max

            bool ReachedV = false;
            bool ReachedH = false;

            foreach (var p in _VerticalPistons)
            {
                p.Velocity = Math.Abs(p.Velocity);
                if (p.CurrentPosition >= p.MaxLimit)
                {
                    ReachedV = true;
                }
            }
            foreach (var p in _VerticalPistons)
            {
                p.Velocity = Math.Abs(p.Velocity);
                if (p.CurrentPosition >= p.MaxLimit)
                {
                    ReachedH = true;
                }
            }

            if(ReachedV && ReachedH)
            {
                return State.DEPLOY_CONNECT;
            }

            return State.DEPLOY_OUT;
        }

        private State StateDeployConnect()
        {
            foreach(var c in _Connectors)
            {
                if (c.CheckConnectionAllowed)
                {
                    c.Connect();
                    return State.IDLE;
                }
            }
            return State.DEPLOY_CONNECT;
        }

        private void FindAllFunctionalBlocks()
        {
            var all_pistons = new List<IMyExtendedPistonBase>();
            GridTerminalSystem.GetBlocksOfType(all_pistons);
            _VerticalPistons.Clear();
            _HorizontalPistons.Clear();
            foreach (var p in all_pistons)
            {
                if (p.CustomName.Contains(VerticalPistonTag))
                {
                    _debug.WriteLine($"Found vertical piston {p.CustomName}");
                    _VerticalPistons.Add(p);
                }
                else if (p.CustomName.Contains(HorizontalPistonTag))
                {
                    _debug.WriteLine($"Found horizontal piston {p.CustomName}");
                    _HorizontalPistons.Add(p);
                }
            }

            var all_connectors = new List<IMyShipConnector>();
            _Connectors.Clear();
            GridTerminalSystem.GetBlocksOfType(all_connectors);
            foreach(var c in all_connectors)
            {
                if (c.CustomName.Contains(ConnectorTag))
                {
                    _debug.WriteLine($"Found docking connector {c.CustomName}");
                    _Connectors.Add(c);
                }
            }

        }
    }
}
