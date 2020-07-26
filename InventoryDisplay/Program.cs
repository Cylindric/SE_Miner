using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.GUI.TextPanel;
using VRageMath;

namespace IngameScript
{
    partial class Program : MyGridProgram
    {
        List<IMyTextPanel> _oreDisplays = new List<IMyTextPanel>();
        InventoryController _inventory;

        public Program()
        {
            Runtime.UpdateFrequency = UpdateFrequency.Update100;

            Debug("loading...");
            _inventory = new InventoryController(this);
            _inventory.FindAllContainers();

            // Find all the panels to use
            var all_panels = new List<IMyTextPanel>();
            GridTerminalSystem.GetBlocksOfType(all_panels);

            // Find all the inventory panels
            _oreDisplays.Clear();
            _oreDisplays = all_panels.Where(p => p.CustomData.Contains("[OreInventory]")).ToList();
            foreach (var d in _oreDisplays)
            {
                d.ContentType = ContentType.TEXT_AND_IMAGE;
            }
        }

        public void Save()
        {
        }

        public void Main(string argument, UpdateType updateSource)
        {
            if (updateSource == UpdateType.Update100)
            {
                UpdateOreDisplays();
            }
        }

        public void UpdateOreDisplays()
        {
            Debug("updating...");
            var ore = _inventory.GetOreCounts();
            string ore_text = string.Empty;
            foreach (var o in ore.OrderByDescending(x => x.Value).ThenBy(x => x.Key))
            {
                string displayValue = $"{o.Value:N0}kg";
                Debug(ore_text);
                if (o.Value < 1)
                {
                    displayValue = $"{o.Value:N2}kg";
                }

                ore_text += $"{displayValue,12} {o.Key}\n";
            }

            var ingots = _inventory.GetIngotCounts();
            string ingot_text = string.Empty;
            foreach (var o in ingots.OrderByDescending(x => x.Value).ThenBy(x => x.Key))
            {
                string displayValue = $"{o.Value:N0}kg";
                Debug(displayValue);
                if (o.Value < 1)
                {
                    displayValue = $"{o.Value:N2}kg";
                }

                ingot_text += $"{displayValue,12} {o.Key}\n";
            }

            Debug($"updating {_oreDisplays.Count} displays...");

            foreach (var d in _oreDisplays)
            {
                d.FontColor = new Color(0.27f, 0f, 0f);
                d.WriteText(ore_text);
                d.WriteText("\n\n", true);
                d.FontColor = new Color(0f, 0.27f, 0f);
                d.WriteText(ingot_text, true);
            }
        }


        public void Debug(string msg)
        {
            Echo(msg);

            //foreach (var d in _debugDisplays)
            //{
            //    lines.Enqueue(msg);
            //    while (lines.Count > 15)
            //    {
            //        lines.Dequeue();
            //    }

            //    bool first = true;
            //    foreach (var line in lines.ToArray())
            //    {
            //        d.WriteText(line, first == false);
            //        first = false;
            //    }
            //}
        }

    }

}
