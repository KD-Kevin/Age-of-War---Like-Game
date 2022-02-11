using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MeleeBox : MonoBehaviour
{
    public string DisplayDamageText = "Damage";
    public int Damage = 10;
    public bool TeamAttackDamage = false;
    [SerializeField]
    private FighterUnitBehaviour UsingFigher;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        OnContact(collision);
    }

    // What Happens on contact with a target
    public virtual void OnContact(Collider2D collision)
    {
        IHealth Health = collision.GetComponent<IHealth>();
        BaseUnitBehaviour OtherUnit = null;
        if (Health is BaseUnitBehaviour)
        {
            OtherUnit = Health as BaseUnitBehaviour;
        }

        if (OtherUnit == UsingFigher)
        {
            return;
        }

        if (OtherUnit != null)
        {
            if (OtherUnit.Team == UsingFigher.Team && !TeamAttackDamage)
            {
                return;
            }
        }

        if (Health != null)
        {
            if (UsingFigher != null)
            {
                Health.Damage(UsingFigher.GetAttackDamage(), DisplayDamageText);
            }
            else
            {
                Health.Damage(Damage, DisplayDamageText);
            }
        }
    }
}
