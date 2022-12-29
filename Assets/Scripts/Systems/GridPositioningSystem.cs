using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.Burst;

public struct GridPositioningGlobalData : IComponentData
{
    public NativeMultiHashMap<int2, CellEntity> CellEntitiesHashMap;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(MoveZombiesSystem))]
public partial class GridPositioningSystem : SystemBase
{
    protected override void OnCreate()
    {
        EntityManager.AddComponent<GridPositioningGlobalData>(SystemHandle);
        EntityManager.SetComponentData(SystemHandle,
            new GridPositioningGlobalData
                { CellEntitiesHashMap = new NativeMultiHashMap<int2, CellEntity>(0, Allocator.Persistent) });

        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        var state = EntityManager.GetComponentData<GridPositioningGlobalData>(SystemHandle);

        state.CellEntitiesHashMap.Dispose();

        base.OnDestroy();
    }

    private int GetEntityCountInQuadrant(NativeMultiHashMap<int2, CellEntity> cellHashMap, int2 pos)
    {
        CellEntity ent;
        NativeMultiHashMapIterator<int2> iter;
        int count = 0;
        if (cellHashMap.TryGetFirstValue(pos, out ent, out iter))
        {
            do
            {
                count++;
            } while (cellHashMap.TryGetNextValue(out ent, ref iter));
        }

        return count;
    }

    protected override void OnUpdate()
    {
        var state = EntityManager.GetComponentData<GridPositioningGlobalData>(SystemHandle);

        state.CellEntitiesHashMap.Clear();

        var query = GetEntityQuery(typeof(Translation), typeof(GridPositionComponent));

        var entityCount = query.CalculateEntityCount();

        if (entityCount > state.CellEntitiesHashMap.Capacity)
        {
            state.CellEntitiesHashMap.Capacity = entityCount + 10;
        }

        new SetGridPositionsJob { CellEntitiesHashMap = state.CellEntitiesHashMap }.Schedule();
    }
}

[BurstCompile]
public partial struct SetGridPositionsJob : IJobEntity
{
    public NativeMultiHashMap<int2, CellEntity> CellEntitiesHashMap;

    private void Execute(Entity ent, ref GridPositionComponent gridPos, in Translation trans)
    {
        gridPos.Value = GridUtils.GetCell(trans.Value.xy);

        CellEntitiesHashMap.Add(gridPos.Value, new CellEntity { Entity = ent, EntityType = gridPos.EntityType, Position = trans.Value});
    }
}

public struct CellEntity
{
    public Entity Entity;
    public CellEntityType EntityType;
    public float3 Position;
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class DebugGridSystem : SystemBase
{
    protected override void OnUpdate()
    {
        foreach (var (gridPosition, ent) in SystemAPI.Query<GridPositionComponent>().WithEntityAccess())
        {
            GridUtils.DrawCell(gridPosition.Value);
        }

        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        var mouseGridPos = GridUtils.GetCell(mousePos);
        GridUtils.DrawCell(mouseGridPos, Color.green);

        Entities
            .WithAll<TowerTag>()
            .ForEach((ref AttackTargetData attackTarget, in GridPositionComponent gridPos, in AttackRangeData attackRange, in Translation trans) =>
            {
                GridUtils.DrawCircle(trans.Value, attackRange.Value, 100, Color.cyan);

                if (Exists(attackTarget.Value))
                    return;
                
                // var coveredCells = GridUtils.GetBoundingBox(gridPos.Value, attackRange.Value);
                //
                // GridUtils.DrawCells(coveredCells);
                //
                // coveredCells.Dispose();
            }).Run();
    }
}