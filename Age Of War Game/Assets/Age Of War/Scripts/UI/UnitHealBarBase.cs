using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        }
    }
}
