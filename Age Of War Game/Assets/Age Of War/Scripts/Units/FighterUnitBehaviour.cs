using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FighterUnitBehaviour : BaseUnitBehaviour
{
    // Parameters
    [SerializeField]
    protected int MeleeDamage = 25;

    // Objects
    [SerializeField]
    protected MeleeBox MeleeAttackObject;

    public override int GetAttackDamage()
    {
        int AttackDamage = MeleeDamage;

        // Modifiers here

        return AttackDamage;
    }
}
