using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript
{
    internal abstract class BaseController
    {
        protected readonly Program _prog;
        protected readonly IMyGridTerminalSystem _grid;
        protected readonly IMyCubeGrid _homeGrid;

        public BaseController(Program program, IMyCubeGrid homeGrid)
        {
            _prog = program;
            _grid = _prog.GridTerminalSystem;
            _homeGrid = homeGrid;
        }

        public void Update() { }

        public void Update(UpdateType updateSource) { }

        public void Trigger(string argument) { }
    }
}