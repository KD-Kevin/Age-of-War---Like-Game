using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Core
{
    public class FloorWeapon : MonoBehaviour, IHealth, ITeam
    {
        [SerializeField]
        private float BuildTime = 3;
        [SerializeField]
        private float BuildCooldown = 3;
        [SerializeField]
        private int BuildCost = 100;
        [SerializeField]
        private int ExperienceGiven = 15;
        [SerializeField]
        protected int StartHealth = 100;
        [SerializeField]
        protected int StartArmor = 0;
        [SerializeField]
        protected int StartMagicArmor = 0;
        [SerializeField]
        protected float DamageMitigationMultiplier = 1f;
        [SerializeField]
        protected int AutoHealAmount = 0;
        [SerializeField]
        protected int AutoRepairAmount = 0;
        [SerializeField]
        protected int AutoRestoreAmount = 0;

        public int CurrentHealth { get => CurrentHP; set => CurrentHP = value; }
        public int MaxHealth { get => MaxHP; set => MaxHP = value; }
        public int StartingHealth { get => StartHealth; set => StartHealth = value; }
        public int CurrentArmor { get => CurrentArmorValue; set => CurrentArmorValue = value; }
        public int MaxArmor { get => MaxArmorValue; set => MaxArmorValue = value; }
        public int StartingArmor { get => StartArmor; set => StartArmor = value; }
        public int CurrentMagicArmor { get => CurrentMagicArmorValue; set => CurrentMagicArmorValue = value; }
        public int MaxMagicArmor { get => MaxMagicArmorValue; set => MaxMagicArmorValue = value; }
        public int StartingMagicArmor { get => StartMagicArmor; set => StartMagicArmor = value; }
        public float DamgeMitigationMult { get => DamageMitigationMultiplier; set => DamageMitigationMultiplier = value; }
        public int AutoHeal { get => AutoHealAmount; set => AutoHealAmount = value; }
        public int AutoRepair { get => AutoRepairAmount; set => AutoRepairAmount = value; }
        public int AutoRestore { get => AutoRestoreAmount; set => AutoRestoreAmount = value; }
        public int PrefabID { get => PrefabSpawnID; set => PrefabSpawnID = value; }
        public int Team { get => TeamID; set => SetTeam(value); }

        protected int TeamID = -1;
        protected int PrefabSpawnID = -1;
        protected int CurrentHP = 0;
        protected int MaxHP = 0;
        protected int CurrentArmorValue = 0;
        protected int MaxArmorValue = 0;
        protected int CurrentMagicArmorValue = 0;
        protected int MaxMagicArmorValue = 0;

        #region IHealth Interface

        public virtual void Damage(int DamageAmount, string DamageReason, DamageTypes DamageType)
        {
            // Multipliers here

            // Defenses Here
            DamageAmount = Mathf.FloorToInt(DamageMitigationMultiplier * DamageAmount);
            int Remainder = DamageAmount;
            if (DamageType == DamageTypes.Physical)
            {
                // Modifiers Here
                if (CurrentArmor > 0)
                {
                    if (DamageAmount > 10)
                    {
                        DamageAmount -= 10;
                    }
                    else
                    {
                        DamageAmount /= 2;
                    }
                }
                Remainder = DamageAmount - CurrentArmor;
                CurrentArmor -= DamageAmount;
                if (CurrentArmor < 0)
                {
                    CurrentArmor = 0;
                }
            }
            else if (DamageType == DamageTypes.Magical)
            {
                if (CurrentMagicArmor > 0)
                {
                    if (DamageAmount > 10)
                    {
                        DamageAmount -= 10;
                    }
                    else
                    {
                        DamageAmount /= 2;
                    }
                }
                Remainder = DamageAmount - CurrentMagicArmor;
                CurrentMagicArmor -= DamageAmount;
                if (CurrentMagicArmor < 0)
                {
                    CurrentMagicArmor = 0;
                }
            }

            // Damage Here
            if (Remainder > 0)
            {
                CurrentHealth -= Remainder;

                if (CurrentHealth < 0)
                {
                    CurrentHealth = 0;
                    DieOff(DamageReason);
                }
            }
        }

        public virtual void DieOff(string DeathReason)
        {
            // Game Over - You Lose
        }

        public virtual void Heal(int HealAmount)
        {
            // Modifiers Here

            if (CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth += HealAmount;
            if (CurrentHealth > MaxHealth)
            {
                CurrentHealth = MaxHealth;
            }
        }

        public virtual void HealAndRepair(int RepairAmount)
        {
            // Modifiers Here

            if (CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth += RepairAmount;
            int Remainder = 0;
            if (CurrentHealth > MaxHealth)
            {
                Remainder = CurrentHealth - MaxHealth;
                CurrentHealth = MaxHealth;
            }

            if (Remainder > 0)
            {
                CurrentArmor += Remainder;
                if (CurrentArmor > MaxArmor)
                {
                    CurrentArmor = MaxArmor;
                }
            }
        }

        public virtual void HealAndRestore(int RestoreAmount)
        {
            // Modifiers Here

            if (CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth += RestoreAmount;
            int Remainder = 0;
            if (CurrentHealth > MaxHealth)
            {
                Remainder = CurrentHealth - MaxHealth;
                CurrentHealth = MaxHealth;
            }

            if (Remainder > 0)
            {
                CurrentMagicArmor += Remainder;
                if (CurrentMagicArmor > MaxMagicArmor)
                {
                    CurrentMagicArmor = MaxMagicArmor;
                }
            }
        }

        public virtual void RepairAndRestore(int RepairAmount)
        {
            // Modifiers Here

            CurrentArmor += RepairAmount;
            int Remainder = 0;
            if (CurrentArmor > MaxArmor)
            {
                Remainder = CurrentArmor - MaxArmor;
                CurrentArmor = MaxArmor;
            }

            if (Remainder > 0)
            {
                CurrentMagicArmor += Remainder;
                if (CurrentMagicArmor > MaxMagicArmor)
                {
                    CurrentMagicArmor = MaxMagicArmor;
                }
            }
        }

        public virtual void RestoreAndRepair(int RestoreAmount)
        {
            // Modifiers Here

            CurrentMagicArmor += RestoreAmount;
            int Remainder = 0;
            if (CurrentMagicArmor > MaxMagicArmor)
            {
                Remainder = CurrentMagicArmor - MaxMagicArmor;
                CurrentMagicArmor = MaxMagicArmor;
            }

            if (Remainder > 0)
            {
                CurrentArmor += Remainder;
                if (CurrentArmor > MaxArmor)
                {
                    CurrentArmor = MaxArmor;
                }
            }
        }

        public virtual void HealRepairAndRestore(int Amount)
        {
            if (CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth += Amount;
            int Remainder = 0;
            if (CurrentHealth > MaxHealth)
            {
                Remainder = CurrentHealth - MaxHealth;
                CurrentHealth = MaxHealth;
            }

            if (Remainder > 0)
            {
                CurrentArmor += Remainder;
                if (CurrentArmor > MaxArmor)
                {
                    Remainder = CurrentArmor - MaxArmor;
                    CurrentArmor = MaxArmor;
                }
                else
                {
                    Remainder = 0;
                }
            }

            if (Remainder > 0)
            {
                CurrentMagicArmor += Remainder;
                if (CurrentMagicArmor > MaxMagicArmor)
                {
                    Remainder = CurrentMagicArmor - MaxMagicArmor;
                    CurrentMagicArmor = MaxMagicArmor;
                }
                else
                {
                    Remainder = 0;
                }
            }
        }

        public virtual void HealRestoreAndRepair(int Amount)
        {
            if (CurrentHealth <= 0)
            {
                return;
            }

            CurrentHealth += Amount;
            int Remainder = 0;
            if (CurrentHealth > MaxHealth)
            {
                Remainder = CurrentHealth - MaxHealth;
                CurrentHealth = MaxHealth;
            }

            if (Remainder > 0)
            {
                CurrentMagicArmor += Remainder;
                if (CurrentMagicArmor > MaxMagicArmor)
                {
                    Remainder = CurrentMagicArmor - MaxMagicArmor;
                    CurrentMagicArmor = MaxMagicArmor;
                }
                else
                {
                    Remainder = 0;
                }
            }

            if (Remainder > 0)
            {
                CurrentArmor += Remainder;
                if (CurrentArmor > MaxArmor)
                {
                    Remainder = CurrentArmor - MaxArmor;
                    CurrentArmor = MaxArmor;
                }
                else
                {
                    Remainder = 0;
                }
            }
        }

        public virtual void Initialize()
        {
            CurrentArmor = StartArmor;
            CurrentHealth = StartHealth;
        }

        public virtual void Repair(int RepairAmount)
        {
            // Modifiers Here

            CurrentArmor += RepairAmount;
            if (CurrentArmor > MaxArmor)
            {
                CurrentArmor = MaxArmor;
            }
        }

        public virtual void Restore(int RestoreAmount)
        {
            // Modifiers Here

            CurrentMagicArmor += RestoreAmount;
            if (CurrentMagicArmor > MaxMagicArmor)
            {
                CurrentMagicArmor = MaxMagicArmor;
            }
        }

        public void SetTeam(int ID)
        {
            TeamID = ID;

            // Maybe do other stuff? should be called early
        }

        #endregion
    }
}