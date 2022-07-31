using UnityEngine;
using AgeOfWar.Core;
using AgeOfWar.Core.Units;

namespace AgeOfWar.UI
{
    public class UnitHealBarBase : MonoBehaviour
    {
        public IHealth HealthComponent { get; private set; }

        public virtual void ResetUi()
        {
            SetUnit(HealthComponent);
        }

        public virtual void SetUnit(IHealth newUnit)
        {
            HealthComponent = newUnit;
        }

        public virtual void UpdateUi()
        {
            // Set Position
            if (HealthComponent is BaseUnitBehaviour)
            {
                BaseUnitBehaviour Unit = HealthComponent as BaseUnitBehaviour;
                transform.position = Unit.HealthBarTransform.position;
                transform.forward = -Camera.main.transform.forward;
            }
        }
    }
}
