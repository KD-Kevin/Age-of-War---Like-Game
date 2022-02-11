using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHealth
{
    public int CurrentHealth {get; set;}
    public int MaxHealth { get; set; }
    public int StartingHealth { get; set; }

    public int CurrentArmor { get; set; }
    public int MaxArmor { get; set; }
    public int StartingArmor { get; set; }

    public abstract void Initialize();

    public abstract void Heal(int HealAmount);

    public abstract void Repair(int RepairAmount);

    public abstract void HealAndRepair(int RepairAmount);

    public abstract void Damage(int DamageAmount, string DamageReason);

    public abstract void DieOff(string DeathReason);
}
