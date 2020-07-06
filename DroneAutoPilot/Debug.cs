using Sandbox.ModAPI.Ingame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.GUI.TextPanel;

namespace IngameScript
{
    class Debug
    {
        private Program _prog;
        private string _tag;
        private List<IMyTextPanel> _debugDisplays;
        private readonly Queue<string> _lines = new Queue<string>();

        private bool _enabled = true;

        public Debug(Program program, string tag = "[DBG]")
        {
            _tag = tag;
            _prog = program;
            FindAllPanels();
        }

        public void Write(string msg)
        {
            _prog.Echo(msg);
            if (!_enabled)
            {
                return;
            }

            foreach (var d in _debugDisplays)
            {
                _lines.Enqueue(msg);
                while (_lines.Count > 15)
                {
                    _lines.Dequeue();
                }

                bool first = true;
                foreach (var line in _lines.ToArray())
                {
                    d.WriteText(line, first == false);
                    first = false;
                }
            }
        }

        public void WriteLine(string msg)
        {
            Write($"{msg}\n");
        }

        private void FindAllPanels()
        {
            var all_panels = new List<IMyTextPanel>();
            _prog.GridTerminalSystem.GetBlocksOfType(all_panels);

            _debugDisplays = all_panels.Where(p => p.CustomData.Contains(_tag)).ToList();
            foreach (var d in _debugDisplays)
            {
                d.ContentType = ContentType.TEXT_AND_IMAGE;
                _prog.Echo($"Debugger connected to {d.CustomName}\n");
            }
        }
    }
}
