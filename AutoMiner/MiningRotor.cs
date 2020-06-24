using Sandbox.ModAPI.Ingame;

namespace IngameScript
{
    class MiningRotor
    {
        public float AdvanceRPM = 4F;
        public float HomePosition = 0F;
        private static IMyMotorAdvancedStator _rotor;

        public MiningRotor(IMyMotorAdvancedStator rotor)
        {
            _rotor = rotor;
        }

        /// <summary>
        /// The current rotor angle in degrees.
        /// </summary>
        public float Angle => Helpers.RadiansToDegrees(_rotor.Angle);

        /// <summary>
        /// Returns the current rotor anglular distance from the home location.
        /// If there is no home set, returns NaN.
        /// </summary>
        /// <returns>Offset from home, or NaN if no home.</returns>
        public float AngleFromHome
        {
            get
            {
                if (_rotor.UpperLimitRad <= 10000)
                {
                    return Helpers.RadiansToDegrees(_rotor.UpperLimitRad - _rotor.Angle);
                }
                return float.NaN;
            }
        }

        public void StartRotation()
        {
            _rotor.RotorLock = false;
            _rotor.TargetVelocityRPM = AdvanceRPM;
            _rotor.LowerLimitDeg = float.MinValue;
            _rotor.UpperLimitDeg = float.MaxValue;
            _rotor.Enabled = true;
        }

        public void StartHoming()
        {
            _rotor.RotorLock = false;
            _rotor.TargetVelocityRPM = AdvanceRPM * 0.5F;
            _rotor.LowerLimitDeg = float.MinValue;
            _rotor.UpperLimitDeg = HomePosition;
            _rotor.Enabled = true;
        }

        public void Lock()
        {
            _rotor.TargetVelocityRPM = 0F;
            _rotor.RotorLock = true;
        }
    }
}
