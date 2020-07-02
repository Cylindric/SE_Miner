using Sandbox.ModAPI.Ingame;
using System;

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

        public abstract void Update();
    }
}