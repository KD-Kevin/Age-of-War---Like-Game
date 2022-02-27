using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class MeleeBox : MonoBehaviour
{
    public string DisplayDamageText = "Damage";
    public int Damage = 10;
    public int MagicDamage = 0;
    public int TrueDamage = 0;
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
                Health.Damage(UsingFigher.GetTrueAttackDamage(), DisplayDamageText, DamageTypes.True);
                Health.Damage(UsingFigher.GetMagicAttackDamage(), DisplayDamageText, DamageTypes.Magical);
                Health.Damage(UsingFigher.GetAttackDamage(), DisplayDamageText, DamageTypes.Physical);
            }
            else
            {
                Health.Damage(TrueDamage, DisplayDamageText, DamageTypes.True);
                Health.Damage(MagicDamage, DisplayDamageText, DamageTypes.Magical);
                Health.Damage(Damage, DisplayDamageText, DamageTypes.Physical);
            }
        }
    }
}
