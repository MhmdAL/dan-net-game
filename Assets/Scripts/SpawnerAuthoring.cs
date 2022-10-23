using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject SquarePrefab;
}

public struct SpawnerComponent : IComponentData
{
    public Entity SquarePrefab;
}

public class SpawnerBaker : Baker<SpawnerAuthoring>
{
    public override void Bake(SpawnerAuthoring authoring)
    {
        AddComponent(new SpawnerComponent { SquarePrefab = GetEntity(authoring.SquarePrefab) });
    }
}
