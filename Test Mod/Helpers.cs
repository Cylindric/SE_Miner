using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class Helpers
    {
        public static double RadiansToDegrees(double radians)
        {
            return (180 / Math.PI) * radians;
        }

        public double DegreesToRadians(double degrees)
        {
            return (Math.PI / 180) * degrees;
        }

        public static Dictionary<string, string> GetCustomData(IMyTerminalBlock block)
        {
            var data = new Dictionary<string, string>();
            var customData = block.CustomData;

            if (string.IsNullOrEmpty(customData))
            {
                return data;
            }
            if (customData.Contains(";"))
            {
                foreach (string v in customData.Split(';'))
                {
                    if (v.Contains("="))
                    {
                        data.Add(v, "1");
                    }
                    else
                    {
                        data.Add(v.Substring(0, v.IndexOf("=")), v.Substring(v.IndexOf("=")));
                    }
                }
            }
            else
            {
                data.Add(customData, "1");
            }
            return data;
        }
    }
}
