using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct GridPositionComponent : IComponentData
{
    public CellEntityType EntityType;
    public int2 Value;
}

public class GridPositionComponentAuthoring : MonoBehaviour
{
    public class GridPositionBaker : Baker<GridPositionComponentAuthoring>
    {
        public override void Bake(GridPositionComponentAuthoring authoring)
        {
            AddComponent(new GridPositionComponent { EntityType = authoring.EntityType });
        }
    }

    public CellEntityType EntityType;
}