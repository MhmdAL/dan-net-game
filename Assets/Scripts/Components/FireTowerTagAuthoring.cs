using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct FireTowerTag : IComponentData
{
}

public class FireTowerTagAuthoring : MonoBehaviour
{
    public class FireTowerTagBaker : Baker<FireTowerTagAuthoring>
    {
        public override void Bake(FireTowerTagAuthoring authoring)
        {
            AddComponent(new FireTowerTag() {  });
        }
    }
}