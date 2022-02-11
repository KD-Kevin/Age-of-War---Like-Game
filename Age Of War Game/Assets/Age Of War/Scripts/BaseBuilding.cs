using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBuilding : MonoBehaviour
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

    public BaseBuildingData BuildingData { get; set; }

    public static Dictionary<int, BaseBuilding> TeamBuildings = new Dictionary<int, BaseBuilding>();

    private void Awake()
    {
        if (TeamBuildings.ContainsKey(TeamBaseID))
        {
            Debug.LogError("Two Bases of the same team exist");
        }
        else
        {
            TeamBuildings.Add(TeamBaseID, this);
        }

        BuildingData = new BaseBuildingData();
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
}

[System.Serializable]
public class BaseBuildingData
{
    public int Health = 1000;
    public int MaxHealth = 1000;

    public int Experience = 0;
    public int MaxExperience = 1000;

    public Sprite BaseForegroundSprite = null;
    public Sprite BaseBaseBackgroundSpriteprite = null;
}
