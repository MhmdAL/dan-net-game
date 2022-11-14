using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[GhostComponent(PrefabType=GhostPrefabType.AllPredicted)]
public struct HeroInput : IInputComponentData
{
    public int Horizontal;
    public int Vertical;
}

[DisallowMultipleComponent]
public class HeroInputAuthoring : MonoBehaviour
{
    class HeroInputBaking : Unity.Entities.Baker<HeroInputAuthoring>
    {
        public override void Bake(HeroInputAuthoring authoring)
        {
            AddComponent<HeroInput>();
        }
    }
}