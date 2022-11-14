using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public partial class GridPositioningSystem : SystemBase
{
    public NativeMultiHashMap<int2, CellEntity> CellEntitiesHashMap;
    
    protected override void OnCreate()
    {
        CellEntitiesHashMap = new NativeMultiHashMap<int2, CellEntity>(0, Allocator.Persistent);
        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        CellEntitiesHashMap.Dispose();
        base.OnDestroy();
    }

    private int GetEntityCountInQuadrant(int2 pos)
    {
        CellEntity ent;
        NativeMultiHashMapIterator<int2> iter;
        int count = 0;
        if (CellEntitiesHashMap.TryGetFirstValue(pos, out ent, out iter))
        {
            do
            {
                count++;
            } while (CellEntitiesHashMap.TryGetNextValue(out ent, ref iter));
        }

        return count;
    }

    protected override void OnUpdate()
    {
        CellEntitiesHashMap.Clear();

        var query = GetEntityQuery(typeof(Translation), typeof(HealthComponent));

        var entityCount = query.CalculateEntityCount();

        if (entityCount > CellEntitiesHashMap.Capacity)
        {
            CellEntitiesHashMap.Capacity = entityCount + 10;
        }

        Entities
            .ForEach((Entity ent, ref GridPositionComponent gridPosition, in Translation trans, in UnitTag unitTag) =>
            {
                gridPosition.Value = Utils.GetQuadrant(trans.Value.xy);
                
                CellEntitiesHashMap.Add(gridPosition.Value, new CellEntity { Entity = ent, UnitType = unitTag.Type });
            }).WithoutBurst().Run(); }
}

public struct CellEntity
{
    public Entity Entity;
    public UnitType UnitType;
}

public partial class DebugGridSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .ForEach((Entity ent, in GridPositionComponent gridPosition) =>
            {
                Utils.DrawQuadrant(gridPosition.Value);
            }).Run();

        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var mouseGridPos = Utils.GetQuadrant(mousePos);
        Utils.DrawQuadrant(mouseGridPos, Color.green);
    }
}