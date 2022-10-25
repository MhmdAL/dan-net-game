using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class ZombieSpawnerAuthoring : MonoBehaviour
{
    public GameObject Prefab;
}

public struct ZombieSpawnerComponent : IComponentData
{
    public Entity Prefab;
    public float TimeTillNextSpawn;
}

public class ZombieSpawnerBaker : Baker<ZombieSpawnerAuthoring>
{
    public override void Bake(ZombieSpawnerAuthoring authoring)
    {
        AddComponent(new ZombieSpawnerComponent { Prefab = GetEntity(authoring.Prefab) });
    }
}
