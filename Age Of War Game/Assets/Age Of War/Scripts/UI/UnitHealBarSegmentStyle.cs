using AgeOfWar.Core;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace AgeOfWar.UI
{
    public class UnitHealBarSegmentStyle : UnitHealBarBase
    {
        [Header("Parameters")]
        [SerializeField]
        private int DesiredPointsPerSegment = 25;
        [SerializeField]
        private Color HealthBarColor;
        [SerializeField]
        private Color HealthEmptyBarColor;
        [SerializeField]
        private Color ArmorBarColor;
        [SerializeField]
        private Color ArmorEmptyBarColor;
        [SerializeField]
        private Color MagicArmorBarColor;
        [SerializeField]
        private Color MagicArmorEmptyBarColor;
        [SerializeField]
        private Color SegmentTrimColor = Color.clear;
        [Header("Setup")]
        [SerializeField]
        private HealthBarSegment SegmentPrefab;
        [SerializeField]
        private Transform HealthSegmentSpawn;
        [SerializeField]
        private Transform ArmorSegmentSpawn;
        [SerializeField]
        private Transform MagicArmorSegmentSpawn;
        [Header("Text")]
        [SerializeField]
        private TextMeshProUGUI HealthText;
        [SerializeField]
        private TextMeshProUGUI ArmorText;
        [SerializeField]
        private TextMeshProUGUI MagicArmorText;

        private List<HealthBarSegment> HealthBarSegmentsPool = new List<HealthBarSegment>();
        private List<HealthBarSegment> HealthBarSegmentsActive = new List<HealthBarSegment>();
        private List<HealthBarSegment> ArmorBarSegmentsPool = new List<HealthBarSegment>();
        private List<HealthBarSegment> ArmorBarSegmentsActive = new List<HealthBarSegment>();
        private List<HealthBarSegment> MagicArmorBarSegmentsPool = new List<HealthBarSegment>();
        private List<HealthBarSegment> MagicArmorBarSegmentsActive = new List<HealthBarSegment>();

        public int SegmentSize { get { return DesiredPointsPerSegment; } set { DesiredPointsPerSegment = value; ResetUi(); } }
        public HealthBarSegment FirstSegment { get; private set; }
        public HealthBarSegment LastSegment { get; private set; }

        public override void SetUnit(IHealth newUnit)
        {
            base.SetUnit(newUnit);

            ReturnAllSegments();

            int NumberOfHealSegments = Mathf.RoundToInt((float)newUnit.MaxHealth / DesiredPointsPerSegment);
            if (NumberOfHealSegments * DesiredPointsPerSegment < newUnit.MaxHealth)
            {
                NumberOfHealSegments++;
            }
            HealthBarSegment Following = null;
            int LastHealSegmentHealthAmount = DesiredPointsPerSegment - NumberOfHealSegments * DesiredPointsPerSegment + newUnit.MaxHealth;
            int HealthTally = 0;
            FirstSegment = null;
            for (int i = 0; i < NumberOfHealSegments; i++)
            {
                HealthBarSegment NewSegment = GetSegment(ArmorTypes.Health);
                NewSegment.SetColors(HealthBarColor, HealthEmptyBarColor, SegmentTrimColor, ArmorTypes.Health);
                NewSegment.transform.SetAsLastSibling();
                NewSegment.FollowingSegment = Following;
                if (NewSegment.FollowingSegment != null)
                {
                    NewSegment.FollowingSegment.PreceedingSegment = NewSegment;
                }

                if (i < NumberOfHealSegments - 1)
                {
                    NewSegment.HealthAmount = DesiredPointsPerSegment;
                    NewSegment.MaxHealthAmount = DesiredPointsPerSegment;
                    NewSegment.MinHealthSegmentAmount = HealthTally;
                    HealthTally += DesiredPointsPerSegment;
                    NewSegment.MaxHealthSegmentAmount = HealthTally;
                }
                else
                {
                    NewSegment.HealthAmount = LastHealSegmentHealthAmount;
                    NewSegment.MaxHealthAmount = LastHealSegmentHealthAmount;
                    NewSegment.MinHealthSegmentAmount = HealthTally;
                    HealthTally += LastHealSegmentHealthAmount;
                    NewSegment.MaxHealthSegmentAmount = HealthTally;
                }
                if (FirstSegment == null)
                {
                    FirstSegment = NewSegment;
                }
                LastSegment = NewSegment;


                Following = NewSegment;
            }

            if (newUnit.MaxArmor > 0.5f)
            {
                NumberOfHealSegments = Mathf.CeilToInt((float)newUnit.MaxArmor / DesiredPointsPerSegment);
                if (NumberOfHealSegments * DesiredPointsPerSegment < newUnit.MaxArmor)
                {
                    NumberOfHealSegments++;
                }
                LastHealSegmentHealthAmount = DesiredPointsPerSegment - NumberOfHealSegments * DesiredPointsPerSegment + newUnit.MaxArmor;
                HealthTally = 0;
                for (int i = 0; i < NumberOfHealSegments; i++)
                {
                    HealthBarSegment NewSegment = GetSegment(ArmorTypes.Physical);
                    NewSegment.SetColors(ArmorBarColor, ArmorEmptyBarColor, SegmentTrimColor, ArmorTypes.Physical);
                    NewSegment.transform.SetAsLastSibling();
                    NewSegment.FollowingSegment = Following;
                    if (NewSegment.FollowingSegment != null)
                    {
                        NewSegment.FollowingSegment.PreceedingSegment = NewSegment;
                    }

                    if (i < NumberOfHealSegments - 1)
                    {
                        NewSegment.HealthAmount = DesiredPointsPerSegment;
                        NewSegment.MaxHealthAmount = DesiredPointsPerSegment;
                        NewSegment.MinHealthSegmentAmount = HealthTally;
                        HealthTally += DesiredPointsPerSegment;
                        NewSegment.MaxHealthSegmentAmount = HealthTally;
                    }
                    else
                    {
                        NewSegment.HealthAmount = LastHealSegmentHealthAmount;
                        NewSegment.MaxHealthAmount = LastHealSegmentHealthAmount;
                        NewSegment.MinHealthSegmentAmount = HealthTally;
                        HealthTally += LastHealSegmentHealthAmount;
                        NewSegment.MaxHealthSegmentAmount = HealthTally;
                    }
                    NewSegment.UpdateSegment();
                    LastSegment = NewSegment;

                    Following = NewSegment;
                }
            }

            if (newUnit.MaxMagicArmor > 0.5f)
            {
                NumberOfHealSegments = Mathf.CeilToInt((float)newUnit.MaxMagicArmor / DesiredPointsPerSegment);
                if (NumberOfHealSegments * DesiredPointsPerSegment < newUnit.MaxMagicArmor)
                {
                    NumberOfHealSegments++;
                }
                LastHealSegmentHealthAmount = DesiredPointsPerSegment - NumberOfHealSegments * DesiredPointsPerSegment + newUnit.MaxMagicArmor;
                HealthTally = 0;
                for (int i = 0; i < NumberOfHealSegments; i++)
                {
                    HealthBarSegment NewSegment = GetSegment(ArmorTypes.Magic);
                    NewSegment.SetColors(MagicArmorBarColor, MagicArmorEmptyBarColor, SegmentTrimColor, ArmorTypes.Magic);
                    NewSegment.transform.SetAsLastSibling();
                    NewSegment.FollowingSegment = Following;
                    if (NewSegment.FollowingSegment != null)
                    {
                        NewSegment.FollowingSegment.PreceedingSegment = NewSegment;
                    }

                    if (i < NumberOfHealSegments - 1)
                    {
                        NewSegment.HealthAmount = DesiredPointsPerSegment;
                        NewSegment.MaxHealthAmount = DesiredPointsPerSegment;
                        NewSegment.MinHealthSegmentAmount = HealthTally;
                        HealthTally += DesiredPointsPerSegment;
                        NewSegment.MaxHealthSegmentAmount = HealthTally;
                    }
                    else
                    {
                        NewSegment.HealthAmount = LastHealSegmentHealthAmount;
                        NewSegment.MaxHealthAmount = LastHealSegmentHealthAmount;
                        NewSegment.MinHealthSegmentAmount = HealthTally;
                        HealthTally += LastHealSegmentHealthAmount;
                        NewSegment.MaxHealthSegmentAmount = HealthTally;
                    }
                    NewSegment.UpdateSegment();
                    LastSegment = NewSegment;

                    Following = NewSegment;
                }
            }
        }

        public override void UpdateUi()
        {
            base.UpdateUi();

            FirstSegment.CheckSegementPreceeding(HealthComponent.CurrentHealth, ArmorTypes.Health);
            LastSegment.CheckSegementFollowing(HealthComponent.CurrentArmor, ArmorTypes.Physical);
            FirstSegment.CheckSegementPreceeding(HealthComponent.CurrentMagicArmor, ArmorTypes.Magic);

            if (HealthText != null)
            {
                HealthText.text = $"{HealthComponent.CurrentHealth} / {HealthComponent.MaxHealth}";
            }

            if (ArmorText != null)
            {
                ArmorText.text = $"{HealthComponent.CurrentArmor} / {HealthComponent.MaxArmor}";
                if (HealthComponent.MaxArmor == 0)
                {
                    if (ArmorText.gameObject.activeInHierarchy)
                    {
                        ArmorText.gameObject.SetActive(false);
                    }
                }
                else if (!ArmorText.gameObject.activeInHierarchy)
                {
                    ArmorText.gameObject.SetActive(true);
                }
            }

            if (MagicArmorText != null)
            {
                MagicArmorText.text = $"{HealthComponent.CurrentMagicArmor} / {HealthComponent.MaxMagicArmor}";
                if (HealthComponent.MaxMagicArmor == 0)
                {
                    if (MagicArmorText.gameObject.activeInHierarchy)
                    {
                        MagicArmorText.gameObject.SetActive(false);
                    }
                }
                else if (!MagicArmorText.gameObject.activeInHierarchy)
                {
                    MagicArmorText.gameObject.SetActive(true);
                }
            }
        }

        private void ReturnAllSegments()
        {
            int count = HealthBarSegmentsActive.Count;
            for (int i = 0; i < count; i++)
            {
                HealthBarSegmentsActive[0].gameObject.SetActive(false);
                HealthBarSegmentsPool.Add(HealthBarSegmentsActive[0]);
                HealthBarSegmentsActive.RemoveAt(0);
            }
            HealthBarSegmentsActive.Clear();

            count = ArmorBarSegmentsActive.Count;
            for (int i = 0; i < count; i++)
            {
                ArmorBarSegmentsActive[0].gameObject.SetActive(false);
                ArmorBarSegmentsPool.Add(ArmorBarSegmentsActive[0]);
                ArmorBarSegmentsActive.RemoveAt(0);
            }
            ArmorBarSegmentsActive.Clear();

            count = MagicArmorBarSegmentsActive.Count;
            for (int i = 0; i < count; i++)
            {
                MagicArmorBarSegmentsActive[0].gameObject.SetActive(false);
                MagicArmorBarSegmentsPool.Add(MagicArmorBarSegmentsActive[0]);
                MagicArmorBarSegmentsActive.RemoveAt(0);
            }
            MagicArmorBarSegmentsActive.Clear();
        }

        private HealthBarSegment GetSegment(ArmorTypes Type)
        {
            HealthBarSegment ReturnedSegment;
            switch (Type)
            {
                case ArmorTypes.Health:
                    if (HealthBarSegmentsPool.Count > 0)
                    {
                        ReturnedSegment = HealthBarSegmentsPool[0];
                        ReturnedSegment.gameObject.SetActive(true);
                        HealthBarSegmentsPool.RemoveAt(0);
                        ReturnedSegment.Clear();
                    }
                    else
                    {
                        ReturnedSegment = Instantiate(SegmentPrefab, HealthSegmentSpawn);
                        ReturnedSegment.ParentHealthBar = this;
                    }
                    HealthBarSegmentsActive.Add(ReturnedSegment);
                    break;
                case ArmorTypes.Magic:
                    if (MagicArmorBarSegmentsPool.Count > 0)
                    {
                        ReturnedSegment = MagicArmorBarSegmentsPool[0];
                        ReturnedSegment.gameObject.SetActive(true);
                        MagicArmorBarSegmentsPool.RemoveAt(0);
                        ReturnedSegment.Clear();
                    }
                    else
                    {
                        ReturnedSegment = Instantiate(SegmentPrefab, MagicArmorSegmentSpawn);
                        ReturnedSegment.ParentHealthBar = this;
                    }
                    MagicArmorBarSegmentsActive.Add(ReturnedSegment);
                    break;
                case ArmorTypes.Physical:
                    if (ArmorBarSegmentsPool.Count > 0)
                    {
                        ReturnedSegment = ArmorBarSegmentsPool[0];
                        ReturnedSegment.gameObject.SetActive(true);
                        ArmorBarSegmentsPool.RemoveAt(0);
                        ReturnedSegment.Clear();
                    }
                    else
                    {
                        ReturnedSegment = Instantiate(SegmentPrefab, ArmorSegmentSpawn);
                        ReturnedSegment.ParentHealthBar = this;
                    }
                    ArmorBarSegmentsActive.Add(ReturnedSegment);
                    break;
                default:
                    Debug.LogWarning($"Armor Type {Type} Not Set Here");
                    ReturnedSegment = null;
                    break;
            }

            return ReturnedSegment;
        }
    }
}
