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

    public override void Initialize()
    {
        base.Initialize();
        MeleeAttackObject.Damage = MeleeDamage;
    }
}
