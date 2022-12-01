using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct HeroTag : IComponentData
{
}

public class HeroTagAuthoring : MonoBehaviour
{
    public class HeroTagBaker : Baker<HeroTagAuthoring>
    {
        public override void Bake(HeroTagAuthoring authoring)
        {
            AddComponent(new HeroTag() {  });
        }
    }
}