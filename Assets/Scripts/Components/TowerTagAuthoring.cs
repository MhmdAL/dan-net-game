using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct TowerTag : IComponentData
{
}

public class TowerTagAuthoring : MonoBehaviour
{
    public class TowerTagBaker : Baker<TowerTagAuthoring>
    {
        public override void Bake(TowerTagAuthoring authoring)
        {
            AddComponent(new TowerTag() {  });
        }
    }
}