using Sandbox.ModAPI.Ingame;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IngameScript
{
    class MiningPiston
    {
        private readonly IMyExtendedPistonBase _piston;
        public bool IsReversePiston = false;
        public bool IsForwardPiston = true;
        public float AdvanceSpeed = 0.1F;
        public float RetractSpeed = -1F;
        readonly Program _prog;

        public MiningPiston(Program program, IMyExtendedPistonBase piston, bool IsReverse = false)
        {
            _prog = program;
            _piston = piston;
            IsReversePiston = IsReverse;
            IsForwardPiston = !IsReverse;
        }

        public float MaxPossibleDistanceFromHome
        {
            get
            {
                if (IsReversePiston)
                {
                    return _piston.MinLimit;
                }
                else
                {
                    return _piston.MaxLimit;
                }
            }
        }

        /// <summary>
        /// The distance from the minimum end-stop in metres.
        /// </summary>
        /// <returns>distance in metres</returns>
        public float DistanceFromHome
        {
            get
            {
                if (IsReversePiston)
                {
                    return _piston.MaxLimit - _piston.CurrentPosition;
                }
                else
                {
                    return _piston.CurrentPosition - _piston.MinLimit;
                }
            }
        }

        public float ExtensionPercentage
        {
            get
            {
                if (IsForwardPiston)
                {
                    // Extention pistons are considered "zero" extension when at their lowest allowed position
                    return _piston.CurrentPosition - _piston.MinLimit;
                }
                else
                {
                    // Contraction pistons are considered "zero" when at their maximum allowed position
                    return _piston.MaxLimit - _piston.CurrentPosition;
                }
            }
        }

        public bool MaxEndstopReached
        {
            get
            {
                if (IsReversePiston)
                {
                    return _piston.CurrentPosition <= _piston.MinLimit;
                }
                else
                {
                    return _piston.CurrentPosition >= _piston.MaxLimit;
                }
            }
        }

        public void AutoAdvance()
        {
            AdvancePiston(AdvanceSpeed);
        }


        public void AutoRetract()
        {
            AdvancePiston(RetractSpeed);
        }

        /// <summary>
        /// Advances the piston at the specified speed
        /// </summary>
        /// <param name="speed"></param>
        public void AdvancePiston(float speed)
        {
            float target_speed = 0F;
            if (IsForwardPiston)
            {
                target_speed = speed;
            }
            else
            {
                target_speed = speed * -1;
            }

            //if (Math.Abs(_piston.Velocity - target_speed) > 0.01) {
            //    _prog.Debug($"Piston {_piston.CustomName} set to {_piston.Velocity:F2}m/s.\n");
            //}
            _piston.Velocity = target_speed;
            _piston.Enabled = true;
        }

        public void StopPiston()
        {
            _piston.Velocity = 0F;
        }
    }
}
