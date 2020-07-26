using Sandbox.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRageMath;

namespace IngameScript
{
    class BaseEntity<T> where T: IMyTerminalBlock
    {
        protected readonly T _block;

        protected MyIni Ini { get; }
        public long EntityId
        {
            get
            {
                return _block.EntityId;
            }
        }

        public string EntityCode
        {
            get
            {
                var number = _block.EntityId.ToString("n0");
                return number.Substring(number.Length - 3);
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

        public BaseEntity(T block)
        {
            _block = block;
            Ini = new MyIni();

            MyIniParseResult result;
            if (!Ini.TryParse(block.CustomData, out result))
            {
                // No custom data found
            }
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
