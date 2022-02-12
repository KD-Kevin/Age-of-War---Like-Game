using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBuilding : MonoBehaviour, IHealth, ITeam
{
    [SerializeField]
    private int TeamBaseID = 0;
    [SerializeField]
    private Sprite DefaultForegroundSprite;
    [SerializeField]
    private Sprite DefaultBackgroundSprite;
    [SerializeField]
    private Transform UnitSpawnTransform;
    [SerializeField]
    private SpriteRenderer BaseForegroundSpriteRenderer;
    [SerializeField]
    private SpriteRenderer BaseBackgroundSpriteRenderer;
    [SerializeField]
    private LayerMask UnitRaycastLayer;

    public BaseBuildingData BuildingData { get; set; }
    public int Team { get => TeamBaseID; set => TeamBaseID = value; }

    public int CurrentHealth { get => BuildingData.Health; set => BuildingData.Health = value; }
    public int MaxHealth { get => BuildingData.MaxHealth; set => BuildingData.MaxHealth = value; }
    public int StartingHealth { get => StartHealth; set => StartHealth = value; }
    public int CurrentArmor { get => BuildingData.Armor; set => BuildingData.Armor = value; }
    public int MaxArmor { get => BuildingData.MaxArmor; set => BuildingData.MaxArmor = value; }
    public int StartingArmor { get => StartArmor; set => StartArmor = value; }
    public int CurrentXp { get => BuildingData.Experience; set => BuildingData.Experience = value; }
    public int MaxXp { get => BuildingData.MaxExperience; set => BuildingData.MaxExperience = value; }

    public static Dictionary<int, BaseBuilding> TeamBuildings = new Dictionary<int, BaseBuilding>();

    protected int StartHealth = 0;
    protected int StartArmor = 0;
    protected int FrameCounter = 0;
    protected RaycastHit2D[] RayHits = new RaycastHit2D[0];
    [HideInInspector]
    public List<BaseUnitBehaviour> BuyUnitBuffer = new List<BaseUnitBehaviour>();
    [HideInInspector]
    public List<Behaviour> HoldPopulationSpot = new List<Behaviour>(); 

    private void Awake()
    {
        if (TeamBuildings.ContainsKey(TeamBaseID))
        {
            Debug.LogError($"Two Bases of the same team exist {TeamBaseID}");
        }
        else
        {
            TeamBuildings.Add(TeamBaseID, this);
        }

        SetTeam(TeamBaseID);
        BuildingData = new BaseBuildingData();
    }

    private void Update()
    {
        FrameCounter++;

        if (BuyUnitBuffer.Count > 0)
        {
            if (FrameCounter > 6)
            {
                FrameCounter = 0;

                // Check to see if we can buy unit
                RayHits = Physics2D.RaycastAll(UnitSpawnTransform.position, UnitSpawnTransform.forward, 3, UnitRaycastLayer);
                if (RayHits.Length == 0)
                {
                    SpawnUnit(BuyUnitBuffer[0]);
                    BuyUnitBuffer.RemoveAt(0);
                }
            }
        }
    }

    public void BuyUnit(BaseUnitBehaviour UnitPrefab)
    {
        BuyUnitBuffer.Add(UnitPrefab);
    }

    public void SpawnUnit(BaseUnitBehaviour UnitPrefab)
    {
        BaseUnitBehaviour.SpawnUnit(UnitPrefab, UnitSpawnTransform.position).SetTeam(TeamBaseID);
    }

    public void SetNewData(BaseBuildingData Data)
    {
        if (Data.BaseForegroundSprite == null)
        {
            BaseForegroundSpriteRenderer.sprite = DefaultForegroundSprite;
        }
        else
        {
            BaseForegroundSpriteRenderer.sprite = Data.BaseForegroundSprite;
        }

        if (Data.BaseBaseBackgroundSpriteprite == null)
        {
            BaseBackgroundSpriteRenderer.sprite = DefaultBackgroundSprite;
        }
        else
        {
            BaseBackgroundSpriteRenderer.sprite = Data.BaseBaseBackgroundSpriteprite;
        }

        BuildingData = Data;
    }

    public int GetPopulation()
    {
        if (!BaseUnitBehaviour.AllActiveTeamUnits.ContainsKey(TeamBaseID))
        {
            return BuyUnitBuffer.Count + HoldPopulationSpot.Count;
        }

        return BuyUnitBuffer.Count + BaseUnitBehaviour.AllActiveTeamUnits[TeamBaseID].Count + HoldPopulationSpot.Count;
    }

    #region IHealth Interface

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

    public virtual void RecieveExperience(int Experience)
    {
        if (CurrentXp == MaxXp)
        {
            return;
        }

        CurrentXp += Experience;
        if (CurrentXp > MaxXp)
        {
            CurrentXp = MaxXp;
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

    public void SetTeam(int TeamID)
    {
        TeamBaseID = TeamID;

        // Maybe do other stuff? should be called early
    }

    #endregion
}

[System.Serializable]
public class BaseBuildingData
{
    public int Health = 1000;
    public int MaxHealth = 1000;
    public int Armor = 0;
    public int MaxArmor = 0;

    public int Experience = 0;
    public int MaxExperience = 1000;

    public Sprite BaseForegroundSprite = null;
    public Sprite BaseBaseBackgroundSpriteprite = null;

    public int MaxPopulation = 5;
}