using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct UnitTag : IComponentData
{
    public UnitType Type;
}

public enum UnitType
{
    Hero,
    Zombie
}

public class UnitTagAuthoring : MonoBehaviour
{
    public class UnitTagBaker : Baker<UnitTagAuthoring>
    {
        public override void Bake(UnitTagAuthoring authoring)
        {
            AddComponent(new UnitTag() { Type = authoring.Type });
        }
    }

    public UnitType Type;
}