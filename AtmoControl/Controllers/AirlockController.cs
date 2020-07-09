using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Screens;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using VRage.Game.ModAPI.Ingame;

namespace IngameScript.Controllers
{
    class AirlockController : BaseController
    {
        internal List<Airlock> Airlocks { get; }

        public AirlockController(Program program, IMyCubeGrid homeGrid) : base(program, homeGrid)
        {
            Airlocks = new List<Airlock>();
        }

        public void Discover(List<Vent> vents, List<Door> doors)
        {
            // Vents are the main indicator that a room is an airlock
            foreach(var v in vents)
            {
                if (v.Mode != Vent.VentMode.AIRLOCK)
                {
                    // Ignore any vents that aren't airlock vents
                    continue;
                }

                var airlock = FindOrCreate(v.Room1);
                airlock.Vents.Add(v);

                // Find any doors linked to this room, and make them airlock doors
                foreach(var d in doors)
                {
                    if(d.Room1 == airlock.Id || d.Room2 == airlock.Id)
                    {
                        if (d.Mode == Door.DoorMode.AIRLOCK_INSIDE)
                        {
                            airlock.InnerDoors.Add(d);
                        }
                        else if (d.Mode == Door.DoorMode.AIRLOCK_OUTSIDE)
                        {
                            airlock.OuterDoors.Add(d);
                        }
                    }
                }
            }
        }

        private Airlock FindOrCreate(string id)
        {
            var airlock = Airlocks.FirstOrDefault(a => a.Id == id);
            if (airlock == null)
            {
                airlock = new Airlock();
                Airlocks.Add(airlock);
            }
            return airlock;
        }
    }
}
