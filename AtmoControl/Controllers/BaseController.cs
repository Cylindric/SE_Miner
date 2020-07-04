using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    internal abstract class BaseController
    {
        protected readonly Program _prog;
        protected readonly IMyGridTerminalSystem _grid;

        public BaseController(Program program)
        {
            _prog = program;
            _grid = _prog.GridTerminalSystem;
        }

        public void Update() { }

        public void Update(UpdateType updateSource) { }
    }
}