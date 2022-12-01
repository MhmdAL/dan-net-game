using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct SlowTowerTag : IComponentData
{
}

public class SlowTowerTagAuthoring : MonoBehaviour
{
    public class SlowTowerTagBaker : Baker<SlowTowerTagAuthoring>
    {
        public override void Bake(SlowTowerTagAuthoring authoring)
        {
            AddComponent(new SlowTowerTag() {  });
        }
    }
}