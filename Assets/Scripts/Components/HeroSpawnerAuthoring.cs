using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct HeroSpawner : IComponentData
{
    public Entity HeroPrefab;
}

public class HeroSpawnerAuthoring : MonoBehaviour
{
    public class HeroSpawnerBaker : Baker<HeroSpawnerAuthoring>
    {
        public override void Bake(HeroSpawnerAuthoring authoring)
        {
            AddComponent(new HeroSpawner {HeroPrefab = GetEntity(authoring.HeroPrefab)});
        }
    }

    public GameObject HeroPrefab;
}