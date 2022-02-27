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

    public int CurrentMagicArmor { get; set; }
    public int MaxMagicArmor { get; set; }
    public int StartingMagicArmor { get; set; }

    public abstract void Initialize();

    /// <summary>
    /// Heals the Health of a Unit
    /// </summary>
    /// <param name="HealAmount"> Amount of Health Attempted to Heal </param>
    public abstract void Heal(int HealAmount);

    /// <summary>
    /// Repairs the Armor of a Unit
    /// </summary>
    /// <param name="RepairAmount"> Amount of Armor Attempted to Repair </param>
    public abstract void Repair(int RepairAmount);

    /// <summary>
    /// Restores the Magic Armor of a Unit
    /// </summary>
    /// <param name="RestoreAmount"> Amount of Magic Armor Attempted to Restore </param>
    public abstract void Restore(int RestoreAmount);

    public abstract void HealAndRepair(int Amount);

    public abstract void HealAndRestore(int Amount);

    public abstract void RepairAndRestore(int Amount);

    public abstract void RestoreAndRepair(int Amount);

    public abstract void HealRepairAndRestore(int Amount);

    public abstract void HealRestoreAndRepair(int Amount);

    public abstract void Damage(int DamageAmount, string DamageReason, DamageTypes DamageType);

    public abstract void DieOff(string DeathReason);
}

public enum DamageTypes
{
    Physical,
    Magical,
    True,
}
