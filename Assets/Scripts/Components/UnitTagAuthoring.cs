using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct UnitTag : IComponentData
{
    public CellEntityType Type;
}

public enum CellEntityType
{
    Hero,
    Zombie,
    Tower
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

    public CellEntityType Type;
}