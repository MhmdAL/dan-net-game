using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class FindTargetSystem : SystemBase
{
    private GridPositioningSystem _gridPositioningSystem;

    private NativeArray<int2> _detectionCells;

    protected override void OnCreate()
    {
        _gridPositioningSystem = World.GetExistingSystemManaged<GridPositioningSystem>();

        _detectionCells = new NativeArray<int2>(9, Allocator.Persistent);
        _detectionCells[0] = new int2(0, 0);
        _detectionCells[1] = new int2(0, 1);
        _detectionCells[2] = new int2(1, 1);
        _detectionCells[3] = new int2(1, 0);
        _detectionCells[4] = new int2(1, -1);
        _detectionCells[5] = new int2(0, -1);
        _detectionCells[6] = new int2(-1, -1);
        _detectionCells[7] = new int2(-1, 0);
        _detectionCells[8] = new int2(-1, 1);

        base.OnCreate();
    }

    protected override void OnDestroy()
    {
        _detectionCells.Dispose();
        
        base.OnDestroy();
    }

    private static Entity FindNearestTarget(int2 gridPos, UnitType unitType,
        NativeMultiHashMap<int2, CellEntity> cellEntitiesMap, NativeArray<int2> detectionCells, Unity.Mathematics.Random random)
    {
        NativeList<Entity> possibleTargets = new NativeList<Entity>(Allocator.Temp);

        for (int i = 0; i < 9; i++)
        {
            CellEntity cellEnt;
            NativeMultiHashMapIterator<int2> iter;

            if (cellEntitiesMap.TryGetFirstValue(gridPos + detectionCells[i], out cellEnt, out iter))
            {
                do
                {
                    if (cellEnt.UnitType != unitType)
                    {
                        possibleTargets.Add(cellEnt.Entity);
                    }
                } while (cellEntitiesMap.TryGetNextValue(out cellEnt, ref iter));
            }
        }

        if (possibleTargets.Length == 0)
        {
            possibleTargets.Dispose();
            return Entity.Null;
        }
        else
        {
            var res = possibleTargets[random.NextInt(0, possibleTargets.Length)];
            possibleTargets.Dispose();
            return res;
        }
    }

    protected override void OnUpdate()
    {
        var cellEntitiesMap = _gridPositioningSystem.CellEntitiesHashMap;
        var detectionCells = _detectionCells;

        var random = new Unity.Mathematics.Random((uint)Random.Range(1, 1000000));

        var gridPosLookup = GetComponentLookup<GridPositionComponent>(true);

        Entities
            .WithReadOnly(cellEntitiesMap)
            .WithReadOnly(detectionCells)
            .WithReadOnly(gridPosLookup)
            .ForEach((ref AttackTargetData attackTarget, in GridPositionComponent gridPos, in UnitTag unitTag) =>
            {
                // Check if the current target still exists & is within range
                GridPositionComponent targetPos;
                if (gridPosLookup.TryGetComponent(attackTarget.Value, out targetPos))
                {
                    for (int i = 0; i < 9; i++)
                    {
                        if (targetPos.Value.Equals(gridPos.Value + detectionCells[i]))
                            return;
                    }
                }
                
                // Otherwise find a new target
                attackTarget.Value = FindNearestTarget(gridPos.Value, unitTag.Type, cellEntitiesMap, detectionCells, random);
            }).Schedule();
    }
}