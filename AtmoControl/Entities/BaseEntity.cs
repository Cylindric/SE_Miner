using Sandbox.ModAPI.Ingame;
using System;
using VRage.Game.ModAPI.Ingame.Utilities;

namespace IngameScript
{
    class BaseEntity
    {
        public BaseEntity(IMyFunctionalBlock block)
        {
            Ini = new MyIni();

            MyIniParseResult result;
            if (!Ini.TryParse(block.CustomData, out result))
                throw new Exception(result.ToString());
        }

        protected string GetIniString(string key, string defaultValue = "")
        {
            string iniValue;
            if (Ini.Get("Atmo", key).TryGetString(out iniValue))
            {
                return iniValue;
            }
            else
            {
                return defaultValue;
            }
        }

        protected bool GetIniBool(string key, bool defaultValue = false)
        {
            bool iniValue;
            if (Ini.Get("Atmo", key).TryGetBoolean(out iniValue))
            {
                return iniValue;
            }
            else
            {
                return defaultValue;
            }
        }

        protected MyIni Ini { get; }
    }
}
