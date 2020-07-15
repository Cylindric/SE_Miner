using Sandbox.ModAPI.Ingame;
using System;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    class BaseEntity
    {
        private readonly IMyFunctionalBlock _block;

        protected MyIni Ini { get; }
        public float EntityId
        {
            get
            {
                return _block.EntityId;
            }
        }

        public string Name
        {
            get
            {
                return _block.CustomName;
            }
        }

        public Vector3D Position
        {
            get
            {
                return _block.GetPosition();
            }
        }

        public BaseEntity(IMyFunctionalBlock block)
        {
            _block = block;
            Ini = new MyIni();

            MyIniParseResult result;
            if (!Ini.TryParse(block.CustomData, out result))
                throw new Exception(result.ToString());
        }

        public override string ToString()
        {
            return Name;
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

        protected float GetIniFloat(string key, float defaultValue = 0.0f)
        {
            double iniValue;
            if (Ini.Get("Atmo", key).TryGetDouble(out iniValue))
            {
                return (float)iniValue;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
