using Sandbox.ModAPI.Ingame;
using System.Collections.Generic;
using System.Linq;
using VRage;

namespace IngameScript
{
    class InventoryController
    {
        readonly Program _prog;
        readonly IMyGridTerminalSystem _grid;
        List<IMyFunctionalBlock> _containers = new List<IMyFunctionalBlock>();

        public InventoryController(Program program)
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
                        _prog.Debug($"InvCon: {ore_type}={item.Amount}");
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