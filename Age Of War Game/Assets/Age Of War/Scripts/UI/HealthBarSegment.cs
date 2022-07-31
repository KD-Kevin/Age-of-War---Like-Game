using AgeOfWar.Core;
using UnityEngine;
using UnityEngine.UI;

namespace AgeOfWar.UI
{
    public class HealthBarSegment : MonoBehaviour
    {
        [SerializeField]
        private Image FillImage;
        [SerializeField]
        private Image BackgroundImage;
        [SerializeField]
        private Image TrimImage;

        public HealthBarSegment PreceedingSegment { get; set; } // Closer to full health
        public HealthBarSegment FollowingSegment { get; set; } // Closser to no health
        public int HealthAmount { get; set; }
        public int MaxHealthAmount { get; set; }
        public ArmorTypes ArmorType { get; set; }
        public int MinHealthSegmentAmount { get; set; }
        public int MaxHealthSegmentAmount { get; set; }

        public UnitHealBarSegmentStyle ParentHealthBar { get; set; }

        public void Clear()
        {
            PreceedingSegment = null;
            FollowingSegment = null;
        }

        public void SetColors(Color Fill, Color Background, Color Trim, ArmorTypes HealthType)
        {
            FillImage.color = Fill;
            BackgroundImage.color = Background;
            TrimImage.color = Trim;
            ArmorType = HealthType;
        }

        public void Damage(int DamageAmount, DamageTypes DamageType)
        {
            if ((DamageType == DamageTypes.Magical && ArmorType == ArmorTypes.Physical) || (DamageType == DamageTypes.Physical && ArmorType == ArmorTypes.Magic) || (DamageType == DamageTypes.True && ArmorType != ArmorTypes.Health))
            {
                // This segment is not Valid for this damage type
                if (FollowingSegment != null)
                {
                    FollowingSegment.Damage(DamageAmount, DamageType);
                }
                return;
            }

            if (HealthAmount == 0)
            {
                if (FollowingSegment != null)
                {
                    FollowingSegment.Damage(DamageAmount, DamageType);
                }
                return;
            }

            int RemainingAmount = DamageAmount - HealthAmount;

            if (RemainingAmount > 0)
            {
                if (FollowingSegment != null)
                {
                    FollowingSegment.Damage(RemainingAmount, DamageType);
                }
                HealthAmount = 0;
            }
            else
            {
                HealthAmount = -RemainingAmount;
            }

            FillImage.fillAmount = (float)HealthAmount / MaxHealthAmount;
        }

        public void Heal(int HealAmount, HealTypes HealType)
        {
            if (HealType != HealTypes.All && HealType != HealTypes.ReverseAll)
            {
                // This segment is not Valid for this heal types
                if (ArmorType == ArmorTypes.Health && (HealType == HealTypes.Repair || HealType == HealTypes.RepairAndRestore || HealType == HealTypes.Restore))
                {
                    if (PreceedingSegment != null)
                    {
                        PreceedingSegment.Heal(HealAmount, HealType);
                    }
                }
                else if (ArmorType == ArmorTypes.Magic && (HealType == HealTypes.Heal || HealType == HealTypes.Repair || HealType == HealTypes.RepairAndHeal))
                {
                    if (PreceedingSegment != null)
                    {
                        PreceedingSegment.Heal(HealAmount, HealType);
                    }
                }
                else if (ArmorType == ArmorTypes.Physical && (HealType == HealTypes.Heal || HealType == HealTypes.Restore || HealType == HealTypes.HealAndRestore))
                {
                    if (PreceedingSegment != null)
                    {
                        PreceedingSegment.Heal(HealAmount, HealType);
                    }
                }
            }

            if (HealType == HealTypes.ReverseAll)
            {
                if (ParentHealthBar != null)
                {
                    ParentHealthBar.UpdateUi();
                    return;
                }
            }

            if (HealthAmount == MaxHealthAmount)
            {
                if (PreceedingSegment != null)
                {
                    PreceedingSegment.Heal(HealAmount, HealType);
                }
                return;
            }

            HealthAmount += HealAmount;
            int Overheal = HealthAmount - MaxHealthAmount;

            if (Overheal > 0)
            {
                if (PreceedingSegment != null)
                {
                    PreceedingSegment.Heal(Overheal, HealType);
                }
                HealthAmount = MaxHealthAmount;
            }

            FillImage.fillAmount = (float)HealthAmount / MaxHealthAmount;
        }

        public void UpdateSegment()
        {
            FillImage.fillAmount = (float)HealthAmount / MaxHealthAmount;
        }

        public void CheckSegement(int HealBarPointValue, ArmorTypes HealthType)
        {
            if (ArmorType != HealthType)
            {
                return;
            }

            if (HealBarPointValue < MinHealthSegmentAmount)
            {
                HealthAmount = 0;
                UpdateSegment();
            }
            else if (HealBarPointValue > MaxHealthSegmentAmount)
            {
                HealthAmount = MaxHealthAmount;
                UpdateSegment();
            }
            else
            {
                int CheckAmount = HealBarPointValue - MinHealthSegmentAmount;
                if (CheckAmount != HealthAmount)
                {
                    HealthAmount = CheckAmount;
                    UpdateSegment();
                }
            }
        }

        public void CheckSegementPreceeding(int HealBarPointValue, ArmorTypes HealthType)
        {
            if (ArmorType != HealthType)
            {
                PreceedingSegment?.CheckSegementPreceeding(HealBarPointValue, HealthType);
                return;
            }

            if (HealBarPointValue <= MinHealthSegmentAmount)
            {
                HealthAmount = 0;
                UpdateSegment();
            }
            else if (HealBarPointValue >= MaxHealthSegmentAmount)
            {
                HealthAmount = MaxHealthAmount;
                UpdateSegment();
            }
            else
            {
                int CheckAmount = HealBarPointValue - MinHealthSegmentAmount;
                if (CheckAmount != HealthAmount)
                {
                    HealthAmount = CheckAmount;
                    UpdateSegment();
                }
            }

            PreceedingSegment?.CheckSegementPreceeding(HealBarPointValue, HealthType);
        }

        public void CheckSegementFollowing(int HealBarPointValue, ArmorTypes HealthType)
        {
            if (ArmorType != HealthType)
            {
                FollowingSegment?.CheckSegementFollowing(HealBarPointValue, HealthType);
                return;
            }

            if (HealBarPointValue < MinHealthSegmentAmount)
            {
                HealthAmount = 0;
                UpdateSegment();
            }
            else if (HealBarPointValue > MaxHealthSegmentAmount)
            {
                HealthAmount = MaxHealthAmount;
                UpdateSegment();
            }
            else
            {
                int CheckAmount = HealBarPointValue - MinHealthSegmentAmount;
                if (CheckAmount != HealthAmount)
                {
                    HealthAmount = CheckAmount;
                    UpdateSegment();
                }
            }

            FollowingSegment?.CheckSegementFollowing(HealBarPointValue, HealthType);
        }
    }

    public enum ArmorTypes
    {
        Health,
        Magic,
        Physical,
    }

    public enum HealTypes
    {
        Heal,
        Repair,
        Restore,
        RepairAndRestore,
        HealAndRestore,
        RepairAndHeal,
        ReverseAll,
        All,
    }
}