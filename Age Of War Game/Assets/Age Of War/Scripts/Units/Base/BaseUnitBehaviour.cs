using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnitBehaviour : MonoBehaviour, IHealth, ITeam
{
    [SerializeField]
    private string UnitDisplayName;
    [SerializeField]
    private Sprite UnitDisplaySprite;
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
    protected float StopDistance = 3;
    [SerializeField]
    protected float AlliedUnitStopDistance = 1;
    [SerializeField]
    private float MovementSpeed = 4;
    [SerializeField]
    protected LayerMask RaycastLayers;
    [SerializeField]
    private Transform LookTransform;

    public string DisplayName { get => UnitDisplayName; set => UnitDisplayName = value; }
    public Sprite DisplaySprite { get => UnitDisplaySprite; set => UnitDisplaySprite = value; }
    public int CurrentHealth { get => CurrentHP; set => CurrentHP = value; }
    public int MaxHealth { get => MaxHP; set => MaxHP = value; }
    public int StartingHealth { get => StartHealth; set => StartHealth = value; }
    public int CurrentArmor { get => CurrentArmorValue; set => CurrentArmorValue = value; }
    public int MaxArmor { get => MaxArmorValue; set => MaxArmorValue = value; }
    public int StartingArmor { get => StartArmor; set => StartArmor = value; }
    public int PrefabID { get => PrefabSpawnID; set => PrefabSpawnID = value; }
    public int Team { get => TeamID; set => TeamID = value; }
    public bool Moving { get; set; }
    public bool Attack { get; set; }

    protected int TeamID = -1;
    protected int PrefabSpawnID = -1;
    protected int CurrentHP = 0;
    protected int MaxHP = 0;
    protected int CurrentArmorValue = 0;
    protected int MaxArmorValue = 0;
    protected int FrameCounter = 0;
    protected RaycastHit2D[] RayHits = new RaycastHit2D[0];
    protected IHealth HealthTarget = null;
    protected float LongestRaycastDistance;

    public static List<BaseUnitBehaviour> FighterPrefabList = new List<BaseUnitBehaviour>();
    public static Dictionary<int, List<BaseUnitBehaviour>> FighterPools = new Dictionary<int, List<BaseUnitBehaviour>>();
    /// <summary>
    /// Example
    /// DesiredUnitList = TeamUnits[(TeamID, PrefabID)]
    /// </summary>
    public static Dictionary<(int, int), List<BaseUnitBehaviour>> TeamUnits = new Dictionary<(int, int), List<BaseUnitBehaviour>>();
    public static Dictionary<int, List<BaseUnitBehaviour>> AllActiveTeamUnits = new Dictionary<int, List<BaseUnitBehaviour>>();

    public virtual void Damage(int DamageAmount, string DamageReason)
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

        int Remainder = DamageAmount - CurrentArmor;

        CurrentArmor -= DamageAmount;
        if (CurrentArmor < 0)
        {
            CurrentArmor = 0;
        }

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

    public virtual void Initialize()
    {
        CurrentArmor = StartArmor;
        CurrentHealth = StartHealth;

        LongestRaycastDistance = StopDistance > AlliedUnitStopDistance ? StopDistance : AlliedUnitStopDistance;

        if (!AllActiveTeamUnits.ContainsKey(TeamID))
        {
            AllActiveTeamUnits.Add(TeamID, new List<BaseUnitBehaviour>());
        }

        if (!AllActiveTeamUnits[TeamID].Contains(this))
        {
            AllActiveTeamUnits[TeamID].Add(this);
        }
    }

    public virtual void Repair(int RepairAmount)
    {
        // Modifiers Here

        if (CurrentArmor <= 0)
        {
            return;
        }

        CurrentArmor += RepairAmount;
        if (CurrentArmor > MaxArmor)
        {
            CurrentArmor = MaxArmor;
        }
    }

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
        return 0;
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

    protected void Update()
    {
        FrameCounter++;

        if (FrameCounter == 9)
        {
            FrameCounter = 0;
            RayHits = Physics2D.RaycastAll(LookTransform.position, LookTransform.forward, LongestRaycastDistance, RaycastLayers);

            bool NewMovingValue = false;
            if (RayHits.Length == 0)
            {
                NewMovingValue = true;
            }
            else
            {
                BaseUnitBehaviour ClosestUnitHit = null;
                BaseBuilding EnemyBase = null;
                foreach (RaycastHit2D hit in RayHits)
                {
                    //Debug.Log($"Object In Front {hit.collider.gameObject.name}");
                    IHealth TempHit = hit.collider.gameObject.GetComponent<IHealth>();
                    if (TempHit is BaseBuilding)
                    {
                        //Debug.Log("Found base");
                        BaseBuilding unit = TempHit as BaseBuilding;
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

                    if (TempHit is BaseUnitBehaviour)
                    {
                        BaseUnitBehaviour unit = TempHit as BaseUnitBehaviour;
                        if (ClosestUnitHit == null)
                        {
                            ClosestUnitHit = unit;
                        }
                    }
                }

                if (ClosestUnitHit != null)
                {
                    float Distance = Vector3.Distance(ClosestUnitHit.transform.position, transform.position);
                    if (ClosestUnitHit.Team == TeamID)
                    {
                        NewMovingValue = Distance > AlliedUnitStopDistance;
                        //Debug.Log($"Stop At {Distance} for Allies ({AlliedUnitStopDistance})");
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
            transform.position += LookTransform.forward * MovementSpeed * Time.deltaTime;
            if (HealthTarget != null)
            {
                HealthTarget = null;
            }
        }
        else
        {
            if (FrameCounter == 5)
            {
                Attack = false;
                if (RayHits.Length == 0)
                {
                    return;
                }

                if (HealthTarget == null)
                {
                    foreach (RaycastHit2D hit in RayHits)
                    {
                        IHealth TempHit = hit.collider.gameObject.GetComponent<IHealth>();
                        if (TempHit is BaseUnitBehaviour)
                        {
                            BaseUnitBehaviour unit = TempHit as BaseUnitBehaviour;
                            if (unit.Team != TeamID && unit.Team != 0)
                            {
                                HealthTarget = TempHit;
                            }
                            break;
                        }

                        if (TempHit is BaseBuilding)
                        {
                            BaseBuilding baseBuilding = TempHit as BaseBuilding;
                            if (baseBuilding.Team != TeamID && baseBuilding.Team != 0)
                            {
                                HealthTarget = TempHit;
                            }
                            break;
                        }
                    }
                }

                if (HealthTarget != null)
                {
                    // Attack
                    Attack = true;
                }
            }

            if (Attack)
            {
                AttackUpdate();
            }
        }
    }

    protected virtual void AttackUpdate()
    {

    }

    protected virtual void StoppedMoving()
    {

    }

    protected virtual void StartingToMoving()
    {

    }

    public virtual void SetTeam(int TeamID)
    {
        Team = TeamID;
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
}