using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct TowerSpawnerComponent : IComponentData
{
    public Entity FireTowerPrefab;
    public Entity SlowTowerPrefab;
}

public class TowerSpawnerAuthoring : MonoBehaviour
{
    public class TowerSpawnerBaker : Baker<TowerSpawnerAuthoring>
    {
        public override void Bake(TowerSpawnerAuthoring authoring)
        {
            AddComponent(new TowerSpawnerComponent { FireTowerPrefab = GetEntity(authoring.FireTowerPrefab),
                SlowTowerPrefab = GetEntity(authoring.SlowTowerPrefab)});
        }
    }

    public GameObject FireTowerPrefab;
    public GameObject SlowTowerPrefab;
}
