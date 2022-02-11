using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Projectiles : MonoBehaviour
{
    public float MaxLifeSpan = 15;
    public Vector2 LaunchingForce = new Vector2(10, 10);
    public int PrefabID { get => PrefabSpawnID; set => PrefabSpawnID = value; }

    protected int PrefabSpawnID = -1;
    public static List<Projectiles> FighterPrefabList = new List<Projectiles>();
    public static Dictionary<int, List<Projectiles>> FighterPools = new Dictionary<int, List<Projectiles>>();
    protected float LifeSpawnTimer = 0;
    protected Rigidbody2D ProjectileRigidbody = null;
    protected Collider2D ProjectileCollider = null;


    // MAKE SURE TO ALWAYS SPAWN IT HERE
    public static Projectiles SpawnProjectile(Projectiles Prefab, Transform ParentSpawn)
    {
        if (!FighterPrefabList.Contains(Prefab))
        {
            Prefab.PrefabID = FighterPrefabList.Count;
            FighterPools.Add(Prefab.PrefabID, new List<Projectiles>());
            FighterPrefabList.Add(Prefab);
        }

        Projectiles Fighter;
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

        return Fighter;
    }

    public static Projectiles SpawnProjectile(Projectiles Prefab, Vector3 Position)
    {
        if (!FighterPrefabList.Contains(Prefab))
        {
            Prefab.PrefabID = FighterPrefabList.Count;
            FighterPools.Add(Prefab.PrefabID, new List<Projectiles>());
            FighterPrefabList.Add(Prefab);
        }

        Projectiles Fighter;
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

        return Fighter;
    }

    public virtual void ReturnToPool()
    {
        gameObject.SetActive(false);
        FighterPools[PrefabID].Add(this);
    }

    protected virtual void OnEnable()
    {
        LifeSpawnTimer = 0;
        if (ProjectileRigidbody == null)
        {
            ProjectileRigidbody = GetComponent<Rigidbody2D>();
        }

        if (ProjectileCollider == null)
        {
            ProjectileCollider = GetComponent<Collider2D>();
        }

        ProjectileRigidbody.AddForce(LaunchingForce);
    }

    private void Update()
    {
        LifeSpawnTimer += Time.deltaTime;
        if (LifeSpawnTimer >= MaxLifeSpan)
        {
            DemolishProjectile();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        OnContact(collision);
    }

    public virtual void DemolishProjectile()
    {
        // This is when a projectile makes contact or lifespan ends - Contact only occurs on Collider Based Projectiles not Trigger
        ReturnToPool();
    }

    public virtual void OnContact(Collision2D collision)
    {
        DemolishProjectile();
    }
}
