using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct ZombieTag : IComponentData
{
}

public class ZombieTagAuthoring : MonoBehaviour
{
    public class ZombieTagBaker : Baker<ZombieTagAuthoring>
    {
        public override void Bake(ZombieTagAuthoring authoring)
        {
            AddComponent(new ZombieTag() {  });
        }
    }
}