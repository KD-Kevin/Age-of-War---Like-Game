using AgeOfWar.Animation;
using AgeOfWar.Data;
using AgeOfWar.Networking;
using AgeOfWar.UI;
using System.Collections.Generic;
using UnityEngine;

namespace AgeOfWar.Core.Units
{
    public class BaseUnitBehaviour : MonoBehaviour, IHealth, ITeam
    {
        [Header("Parameters")]
        [SerializeField]
        protected string UnitDisplayName;
        [SerializeField]
        protected Sprite UnitDisplaySprite;

        #region Default Values

        [Header("Default Values")]
        [SerializeField]
        protected float BuildTime = 3;
        [SerializeField]
        protected float BuildCooldown = 3;
        [SerializeField]
        protected int BuildCost = 100;
        [SerializeField]
        protected int ExperienceGiven = 15;
        [SerializeField]
        protected int StartHealth = 100;
        [SerializeField]
        protected int MaxHP = 0;
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
        [SerializeField]
        protected float StopDistance = 3;
        [SerializeField]
        protected float AlliedUnitStopDistance = 1f;
        [SerializeField]
        protected float MovementSpeed = 4;
        [SerializeField]
        protected float AttackRate = 1;
        [SerializeField]
        protected int PhysicalDamage = 5;
        [SerializeField]
        protected int MagicalDamage = 0;
        [SerializeField]
        protected int TrueDamage = 0;

        #endregion

        [Header("Enviroment")]
        [SerializeField]
        protected LayerMask RaycastLayers;
        [SerializeField]
        protected LayerMask GroundLayers;
        [SerializeField]
        protected Transform LookTransform;
        [SerializeField]
        protected Transform FeetTransform;
        [SerializeField]
        protected Transform HealthBarPositionTransform;
        [SerializeField]
        protected UnitDataScriptableObject UnitDataScriptableObject;
        [SerializeField]
        protected List<EquipmentAsthetic> EquipmentAsthetics = new List<EquipmentAsthetic>();
        [SerializeField]
        protected UnitAnimationController UnitAnimator;

        public List<EquipmentAsthetic> Asthetics { get => EquipmentAsthetics; }
        public string DisplayName { get => UnitDisplayName; set => UnitDisplayName = value; }
        public Sprite DisplaySprite { get => UnitDisplaySprite; set => UnitDisplaySprite = value; }
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
        public float RateOfFire { get { return AttackRate; } }
        public float AttackPeriod { get; protected set; }
        public int DefaultPhyscialDamage { get { return PhysicalDamage; } }
        public int DefaultMagicalDamage { get { return MagicalDamage; } }
        public int DefaultTrueDamage { get { return TrueDamage; } }
        public Transform HealthBarTransform { get { return HealthBarPositionTransform; } }

        public UnitData UnitData { get; set; }
        public List<EquipmentChangeScriptableObject> CurrentListOfPossibleChanges { get; set; }
        public List<EquipmentChangeScriptableObject> CurrentEquipment { get; set; }
        public bool Moving { get; set; }
        public bool Attacking { get; set; }
        public bool AttackStarted { get; set; }
        public bool IdleStarted { get; set; }
        public bool WonGame { get; set; }
        public bool LostGame { get; set; }
        public bool DrawedGame { get; set; }

        protected int TeamID = -1;
        protected int PrefabSpawnID = -1;
        protected int CurrentHP = 0;
        protected int CurrentArmorValue = 0;
        protected int MaxArmorValue = 0;
        protected int CurrentMagicArmorValue = 0;
        protected int MaxMagicArmorValue = 0;
        protected int FrameCounter = 0;
        protected RaycastHit[] RayHits = new RaycastHit[0];
        protected IHealth HealthTarget = null;
        protected float LongestRaycastDistance;
        protected float RepairHealRestoreTimer = 0;
        protected float AttackRateTimer = 0;

        public static List<BaseUnitBehaviour> FighterPrefabList = new List<BaseUnitBehaviour>();
        public static Dictionary<int, List<BaseUnitBehaviour>> FighterPools = new Dictionary<int, List<BaseUnitBehaviour>>();
        /// <summary>
        /// Example
        /// DesiredUnitList = TeamUnits[(TeamID, PrefabID)]
        /// </summary>
        public static Dictionary<(int, int), List<BaseUnitBehaviour>> TeamUnits = new Dictionary<(int, int), List<BaseUnitBehaviour>>();
        public static Dictionary<int, List<BaseUnitBehaviour>> AllActiveTeamUnits = new Dictionary<int, List<BaseUnitBehaviour>>();

        #region IHealth

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
            gameObject.SetActive(false);
            FighterPools[PrefabID].Add(this);

            if (TeamUnits.ContainsKey((TeamID, PrefabID)))
            {
                if (TeamUnits[(TeamID, PrefabID)].Contains(this))
                {
                    TeamUnits[(TeamID, PrefabID)].Remove(this);
                }
            }

            if (AllActiveTeamUnits.ContainsKey(TeamID))
            {
                if (AllActiveTeamUnits[TeamID].Contains(this))
                {
                    AllActiveTeamUnits[TeamID].Remove(this);
                }
            }

            if (TeamID != 1)
            {
                BaseBuilding.TeamBuildings[1].RecieveExperience(GetExperienceDropped());
            }
        }

        public virtual void Heal(int HealAmount)
        {
            if (CurrentHealth <= 0 || HealAmount == 0)
            {
                return;
            }

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
            if (CurrentHealth <= 0 || RepairAmount == 0)
            {
                return;
            }
            // Modifiers Here


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
            if (CurrentHealth <= 0 || RestoreAmount == 0)
            {
                return;
            }

            // Modifiers Here


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
            if (CurrentHealth <= 0 || RepairAmount == 0)
            {
                return;
            }

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
            if (CurrentHealth <= 0 || RestoreAmount == 0)
            {
                return;
            }

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
            if (CurrentHealth <= 0 || Amount == 0)
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
            if (CurrentHealth <= 0 || Amount == 0)
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
            CurrentMagicArmor = StartMagicArmor;

            LongestRaycastDistance = StopDistance > AlliedUnitStopDistance ? StopDistance : AlliedUnitStopDistance;

            if (!AllActiveTeamUnits.ContainsKey(TeamID))
            {
                // To make sure tht both team pools are initialized
                if (!AllActiveTeamUnits.ContainsKey(1))
                {
                    AllActiveTeamUnits.Add(1, new List<BaseUnitBehaviour>());
                }
                if (!AllActiveTeamUnits.ContainsKey(2))
                {
                    AllActiveTeamUnits.Add(2, new List<BaseUnitBehaviour>());
                }

                if (TeamID != 1 && TeamID != 2)
                {
                    Debug.Log($"Trying to play with more then 2 teams, get out of here team {TeamID}");
                }
            }

            if (!AllActiveTeamUnits[TeamID].Contains(this))
            {
                AllActiveTeamUnits[TeamID].Add(this);
            }

            UnitData = UnitDataScriptableObject.Data.Clone();
            CurrentListOfPossibleChanges = UnitData.PossibleEquipmentChanges;
            CurrentEquipment = new List<EquipmentChangeScriptableObject>();
            AttackPeriod = 1 / AttackRate;
            UnitHealthBarManager.Instance.GetHealthBar(this);

            if (TeamID == 1)
            {
                transform.LookAt(BaseBuilding.TeamBuildings[2].SpawnPoint);
            }
            else if (TeamID == 2)
            {
                transform.LookAt(BaseBuilding.TeamBuildings[1].SpawnPoint);
            }
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

        #endregion

        public virtual float GetTimeToCreate()
        {
            float TimeToCreate = BuildTime;

            // Modifiers Here


            return TimeToCreate;
        }

        public virtual float GetCooldown()
        {
            float Cooldown = BuildCooldown;

            // Modifiers Here


            return Cooldown;
        }

        public virtual int GetBuildCost()
        {
            int CostToCreate = BuildCost;

            // Modifiers Here


            return CostToCreate;
        }

        public virtual int GetAttackDamage()
        {
            int Damage = DefaultPhyscialDamage;

            // Multipliers Here

            return Damage;
        }

        public virtual int GetMagicAttackDamage()
        {
            int Damage = DefaultMagicalDamage;

            // Multipliers Here

            return Damage;
        }

        public virtual int GetTrueAttackDamage()
        {
            int Damage = DefaultTrueDamage;

            // Multipliers Here

            return Damage;
        }

        public virtual int GetOverallDamage()
        {
            return GetAttackDamage() + GetMagicAttackDamage() + GetTrueAttackDamage();
        }

        public virtual int GetExperienceDropped()
        {
            int Experience = ExperienceGiven;

            // Modifiers Here


            return Experience;
        }

        // MAKE SURE TO ALWAYS SPAWN IT HERE
        public static BaseUnitBehaviour SpawnUnit(BaseUnitBehaviour Prefab, Transform ParentSpawn)
        {
            if (!FighterPrefabList.Contains(Prefab))
            {
                Prefab.PrefabID = FighterPrefabList.Count;
                FighterPools.Add(Prefab.PrefabID, new List<BaseUnitBehaviour>());
                FighterPrefabList.Add(Prefab);
            }

            BaseUnitBehaviour Fighter;
            if (FighterPools[Prefab.PrefabID].Count > 0)
            {
                Fighter = FighterPools[Prefab.PrefabID][0];
                FighterPools[Prefab.PrefabID].RemoveAt(0);
                Fighter.transform.SetParent(ParentSpawn);
                Fighter.gameObject.SetActive(Prefab.gameObject.activeSelf);
            }
            else
            {
                Fighter = Instantiate(Prefab, ParentSpawn);
            }

            Fighter.PrefabSpawnID = Prefab.PrefabID;

            return Fighter;
        }

        public static BaseUnitBehaviour SpawnUnit(BaseUnitBehaviour Prefab, Vector3 Position)
        {
            if (!FighterPrefabList.Contains(Prefab))
            {
                Prefab.PrefabID = FighterPrefabList.Count;
                FighterPools.Add(Prefab.PrefabID, new List<BaseUnitBehaviour>());
                FighterPrefabList.Add(Prefab);
            }

            BaseUnitBehaviour Fighter;
            if (FighterPools[Prefab.PrefabID].Count > 0)
            {
                Fighter = FighterPools[Prefab.PrefabID][0];
                FighterPools[Prefab.PrefabID].RemoveAt(0);
                Fighter.transform.SetParent(null);
                Fighter.gameObject.SetActive(Prefab.gameObject.activeSelf);
            }
            else
            {
                Fighter = Instantiate(Prefab, Position, Quaternion.identity);
            }

            Fighter.PrefabSpawnID = Prefab.PrefabID;

            return Fighter;
        }

        public static void UpdateUnits()
        {
            foreach (List<BaseUnitBehaviour> unitList in AllActiveTeamUnits.Values)
            {
                foreach (BaseUnitBehaviour unit in unitList)
                {
                    unit.UpdateUnit();
                }
            }
        }

        protected IHealth PotentialTarget;
        protected virtual void UpdateUnit()
        {
            #region Post Game

            if (DrawedGame)
            {
                // Drawed game Animation if its not startedyet
                DrawedGameUpdate();
                return;
            }

            if (WonGame)
            {
                // Start Win Animation if its not started yet
                WonGameUpdate();
                return;
            }

            if (LostGame)
            {
                // Start Lost Animation if its not started yet
                LostGameUpdate();
                return;
            }

            #endregion

            // Healing, etc
            if (AutoHeal != 0 || AutoRepair != 0 || AutoRestore != 0)
            {
                if (CurrentHealth < MaxHealth || CurrentArmor < MaxArmor || CurrentMagicArmor < MaxMagicArmor)
                {
                    RepairHealRestoreTimer += LockstepManager.Instance.HalfStepTime;
                    if (RepairHealRestoreTimer >= 1f)
                    {
                        RepairHealRestoreTimer -= 1;
                        Heal(AutoHeal);
                        Repair(AutoRepair);
                        Restore(AutoRestore);
                    }
                }
            }

            if (Attacking)
            {
                if (HealthTarget == null || HealthTarget.CurrentHealth <= 0)
                {
                    Attacking = false;
                    if (HealthTarget != null)
                    {
                        if (HealthTarget is BaseBuilding)
                        {
                            // YOU WON!
                            WonGame = true;
                        }
                    }
                    return;
                }

                MonoBehaviour Target = HealthTarget as MonoBehaviour;
                if (Target != null)
                {
                    transform.LookAt(Target.transform);
                }
            }
            else
            {
                if (TeamID == 1)
                {
                    transform.LookAt(BaseBuilding.TeamBuildings[2].SpawnPoint);
                }
                else if (TeamID == 2)
                {
                    transform.LookAt(BaseBuilding.TeamBuildings[1].SpawnPoint);
                }


                RayHits = Physics.RaycastAll(LookTransform.position, LookTransform.forward, LongestRaycastDistance, RaycastLayers, QueryTriggerInteraction.Collide);

                bool NewMovingValue = false;
                if (RayHits.Length == 0)
                {
                    NewMovingValue = true;
                }
                else
                {
                    BaseUnitBehaviour ClosestUnitHit = null;
                    BaseBuilding EnemyBase = null;
                    foreach (RaycastHit hit in RayHits)
                    {
                        //Debug.Log($"Object In Front {hit.collider.gameObject.name}");
                        PotentialTarget = hit.collider.gameObject.GetComponent<IHealth>();
                        if (PotentialTarget != null)
                        {
                            if (PotentialTarget is BaseBuilding)
                            {
                                //Debug.Log("Found base");
                                BaseBuilding unit = PotentialTarget as BaseBuilding;
                                if (unit.Team == TeamID)
                                {
                                    NewMovingValue = true;
                                    break;
                                }
                                else
                                {
                                    EnemyBase = unit;
                                    NewMovingValue = false;
                                    break;
                                }
                            }

                            if (PotentialTarget is BaseUnitBehaviour)
                            {
                                BaseUnitBehaviour unit = PotentialTarget as BaseUnitBehaviour;

                                if (ClosestUnitHit == null && unit != this)
                                {
                                    ClosestUnitHit = unit;
                                    break;
                                }
                            }
                        }
                    }

                    if (ClosestUnitHit != null)
                    {
                        float Distance = Vector3.Distance(ClosestUnitHit.transform.position, transform.position);
                        if (ClosestUnitHit.Team == TeamID)
                        {
                            NewMovingValue = Distance > AlliedUnitStopDistance;
                            Debug.Log($"Stop At {Distance} for Allies ({AlliedUnitStopDistance})");
                        }
                        else
                        {
                            NewMovingValue = Distance > StopDistance;
                        }
                    }

                    if (!NewMovingValue && EnemyBase != null)
                    {
                        float Distance = Vector3.Distance(EnemyBase.transform.position, transform.position);
                        NewMovingValue = Distance < StopDistance;
                    }
                }

                if (NewMovingValue != Moving)
                {
                    if (NewMovingValue)
                    {
                        StartingToMoving();
                    }
                    else
                    {
                        StoppedMoving();
                    }
                }

                Moving = NewMovingValue;
            }

            if (Moving)
            {
                if (Attacking)
                {
                    Attacking = false;
                    AttackStarted = false;
                    IdleStarted = false;
                    AttackEnd();
                }

                transform.position += LookTransform.forward * MovementSpeed * LockstepManager.Instance.HalfStepTime;
                if (HealthTarget != null)
                {
                    HealthTarget = null;
                }
            }
            else
            {
                if (Attacking)
                {
                    if (HealthTarget == null || HealthTarget.CurrentHealth <= 0)
                    {
                        Attacking = false;
                        return;
                    }
                }
                else
                {
                    Attacking = false;
                    AttackStarted = false;
                    if (RayHits.Length == 0)
                    {
                        RayHits = Physics.SphereCastAll(LookTransform.position, 1, LookTransform.forward, LongestRaycastDistance, RaycastLayers);
                    }

                    if (HealthTarget == null)
                    {
                        foreach (RaycastHit hit in RayHits)
                        {
                            PotentialTarget = hit.collider.gameObject.GetComponent<IHealth>();
                            if (PotentialTarget is BaseUnitBehaviour)
                            {
                                BaseUnitBehaviour unit = PotentialTarget as BaseUnitBehaviour;
                                if (unit.Team != TeamID && unit.Team != 0)
                                {
                                    HealthTarget = PotentialTarget;
                                }
                                break;
                            }

                            if (PotentialTarget is BaseBuilding)
                            {
                                BaseBuilding baseBuilding = PotentialTarget as BaseBuilding;
                                if (baseBuilding.Team != TeamID && baseBuilding.Team != 0)
                                {
                                    HealthTarget = PotentialTarget;
                                }
                                break;
                            }
                        }
                    }
                }

                if (HealthTarget != null)
                {
                    // Attack
                    Attacking = true;
                }
                else
                {
                    Attacking = false;
                }


                if (Attacking)
                {
                    if (!AttackStarted)
                    {
                        AttackStarted = true;
                        AttackStart();
                    }
                    AttackUpdate();
                }
                else
                {
                    if (!IdleStarted)
                    {
                        IdleStarted = true;
                        IdleStart();
                    }
                    IdleUpdate();
                }
            }

            if (Physics.Raycast(FeetTransform.position + Vector3.up * 20, Vector3.down, out RaycastHit groundHit, 40, GroundLayers))
            {
                float HeightDiff = FeetTransform.position.y - groundHit.point.y;

                transform.Translate(Vector3.down * HeightDiff);
            }
        }

        protected virtual void WonGameUpdate()
        {
            if (UnitAnimator.CurrentAnimation != UnitAnimationKey.Win.ToString())
            {
                UnitAnimator.ChangeAnimation(UnitAnimationKey.Win);
            }
        }

        protected virtual void LostGameUpdate()
        {
            if (UnitAnimator.CurrentAnimation != UnitAnimationKey.Lose.ToString())
            {
                UnitAnimator.ChangeAnimation(UnitAnimationKey.Lose);
            }
        }

        protected virtual void DrawedGameUpdate()
        {
            if (UnitAnimator.CurrentAnimation != UnitAnimationKey.Draw.ToString())
            {
                UnitAnimator.ChangeAnimation(UnitAnimationKey.Draw);
            }
        }

        protected virtual void AttackEnd()
        {

        }

        protected virtual void AttackStart()
        {
            if (UnitAnimator.CurrentAnimation != UnitAnimationKey.Attack1.ToString())
            {
                UnitAnimator.ChangeAnimation(UnitAnimationKey.Attack1);
            }
        }

        protected virtual void AttackUpdate()
        {
            AttackRateTimer += LockstepManager.Instance.HalfStepTime;
            if (AttackRateTimer >= AttackPeriod)
            {
                AttackRateTimer -= AttackPeriod;

                Attack();
            }
        }

        protected virtual void IdleStart()
        {
            if (UnitAnimator.CurrentAnimation != UnitAnimationKey.Attack1.ToString())
            {
                UnitAnimator.ChangeAnimation(UnitAnimationKey.Idle);
            }
        }

        protected virtual void IdleUpdate()
        {

        }

        // Melee Combat by default, change this to launch a projectile for ranged combat or mixed combat or non combat abilities
        public virtual void Attack()
        {
            if (HealthTarget != null && HealthTarget.CurrentHealth > 0)
            {
                HealthTarget.Damage(GetTrueAttackDamage(), $"Attacked From {DisplayName}", DamageTypes.True);
                HealthTarget.Damage(GetMagicAttackDamage(), $"Attacked From {DisplayName}", DamageTypes.Magical);
                HealthTarget.Damage(GetAttackDamage(), $"Attacked From {DisplayName}", DamageTypes.Physical);

                if (HealthTarget.CurrentHealth <= 0)
                {
                    HealthTarget = null;
                    Attacking = false;
                }
            }
            else
            {
                HealthTarget = null;
                Attacking = false;
            }
        }

        protected virtual void StoppedMoving()
        {
            UnitAnimator.ChangeAnimation(UnitAnimationKey.Idle);
        }

        protected virtual void StartingToMoving()
        {
            UnitAnimator.ChangeAnimation(UnitAnimationKey.Walk);
        }

        public virtual void SetTeam(int NewTeamID)
        {
            TeamID = NewTeamID;
            if (Team == 1)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
            else if (Team == 2)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (Team == 0)
            {
                // Null Team
                DieOff("Null Team");
                return;
            }
            else
            {
                // Add stuff here later if more then two players become a feature
                DieOff("Invalid Team");
                return;
            }

            if (!TeamUnits.ContainsKey((TeamID, PrefabID)))
            {
                TeamUnits.Add((TeamID, PrefabID), new List<BaseUnitBehaviour>());
            }

            if (!TeamUnits[(TeamID, PrefabID)].Contains(this))
            {
                TeamUnits[(TeamID, PrefabID)].Add(this);
            }

            Initialize();
        }

        public virtual void EquipmentChange(EquipmentChangeScriptableObject EquipmentChange)
        {
            // Turn off / on pertaining asthetics per equipment change
            foreach(int EquipmentID in EquipmentChange.AstheticChangeOutIDs)
            {
                EquipmentAsthetic TurnedOfEquipment = EquipmentAsthetics.Find(x => x.ID == EquipmentID);
                TurnedOfEquipment?.TurnOff();
            }

            foreach (int EquipmentID in EquipmentChange.AstheticChangeInIDs)
            {
                EquipmentAsthetic TurnedOfEquipment = EquipmentAsthetics.Find(x => x.ID == EquipmentID);
                TurnedOfEquipment?.TurnOn();
            }

            // Change Stats
            CurrentEquipment.Add(EquipmentChange);
        }
    }
}
