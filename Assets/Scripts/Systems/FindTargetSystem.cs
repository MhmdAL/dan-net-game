using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateBefore(typeof(AttackSystem))]
public partial class FindTargetSystem : SystemBase
{
    private static NativeList<CellEntity> GetCellEntities(NativeMultiHashMap<int2, CellEntity> cellEntitiesMap,
        NativeArray<int2> coveredCells)
    {
        var cellEntities = new NativeList<CellEntity>(Allocator.Temp);

        for (int i = 0; i < coveredCells.Length; i++)
        {
            CellEntity cellEnt;
            NativeMultiHashMapIterator<int2> iter;

            if (cellEntitiesMap.TryGetFirstValue(coveredCells[i], out cellEnt, out iter))
            {
                do
                {
                    cellEntities.Add(cellEnt);
                } while (cellEntitiesMap.TryGetNextValue(out cellEnt, ref iter));
            }
        }

        return cellEntities;
    }

    private static NativeList<Entity> GetPossibleTargets(float3 position, NativeList<CellEntity> cellEntities,
        float? range, CellEntityType targetType)
    {
        NativeList<Entity> possibleTargets = new NativeList<Entity>(Allocator.Temp);

        for (int i = 0; i < cellEntities.Length; i++)
        {
            var cellEnt = cellEntities[i];
            if (cellEnt.EntityType == targetType)
            {
                if (range != null && math.distancesq(position, cellEnt.Position) < range * range)
                {
                    possibleTargets.Add(cellEnt.Entity);
                }
                else if (range == null)
                {
                    possibleTargets.Add(cellEnt.Entity);
                }
            }
        }

        return possibleTargets;
    }

    private static Entity FindRandomTarget(float3 position, CellEntityType unitType, float? range,
        NativeMultiHashMap<int2, CellEntity> cellEntitiesMap, NativeArray<int2> detectionCells,
        Unity.Mathematics.Random random)
    {
        var cellEntities = GetCellEntities(cellEntitiesMap, detectionCells);

        var possibleTargets = GetPossibleTargets(position, cellEntities, range, unitType);

        cellEntities.Dispose();
        detectionCells.Dispose();

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
        var cellEntitiesMap = EntityManager
            .GetComponentData<GridPositioningGlobalData>(World.GetExistingSystem<GridPositioningSystem>())
            .CellEntitiesHashMap;

        var random = new Unity.Mathematics.Random((uint)Random.Range(1, 1000000));

        var gridPosLookup = GetComponentLookup<GridPositionComponent>(true);
        var translationLookup = GetComponentLookup<Translation>(true);

        new UnitFindTargetJob
        {
            GridPosLookup = gridPosLookup,
            CellEntitiesMap = cellEntitiesMap,
            Random = random
        }.Schedule();

        new TowerFindTargetJob
        {
            TranslationLookup = translationLookup,
            CellEntitiesMap = cellEntitiesMap,
            Random = random
        }.Schedule();
    }
    
    [BurstCompile]
    [WithAny(typeof(HeroTag), typeof(ZombieTag))]
    public partial struct UnitFindTargetJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<GridPositionComponent> GridPosLookup;
        [ReadOnly] public NativeMultiHashMap<int2, CellEntity> CellEntitiesMap;
        
        public Unity.Mathematics.Random Random;
        
        private void Execute(ref AttackTargetData attackTarget, in GridPositionComponent gridPos, in UnitTag unitTag,
            in Translation trans)
        {
            var coveredCells = GridUtils.GetBoundingBox(gridPos.Value, GridUtils.CellSize);

            // Check if the current target still exists & is within range
            GridPositionComponent targetPos;
            if (GridPosLookup.TryGetComponent(attackTarget.Value, out targetPos))
            {
                for (int i = 0; i < coveredCells.Length; i++)
                {
                    if (targetPos.Value.Equals(gridPos.Value + coveredCells[i]))
                        return;
                }
            }
            
            // Otherwise find a new target
            var targetType = unitTag.Type == CellEntityType.Hero ? CellEntityType.Zombie : CellEntityType.Hero;
            attackTarget.Value =
                FindRandomTarget(trans.Value, targetType, null, CellEntitiesMap, coveredCells.ToArray(Allocator.Temp), Random);

            coveredCells.Dispose();
        }
    }
    
    [BurstCompile]
    [WithAll(typeof(TowerTag))]
    public partial struct TowerFindTargetJob : IJobEntity
    {
        [ReadOnly] public ComponentLookup<Translation> TranslationLookup;
        [ReadOnly] public NativeMultiHashMap<int2, CellEntity> CellEntitiesMap;
        
        public Unity.Mathematics.Random Random;
        
        private void Execute(ref AttackTargetData attackTarget, in GridPositionComponent gridPos, in Translation trans,
            in AttackRangeData attackRange)
        {
            // Check if the current target still exists & is within range
            Translation targetPos;
            if (TranslationLookup.TryGetComponent(attackTarget.Value, out targetPos))
            {
                if (math.distancesq(targetPos.Value, trans.Value) < attackRange.Value * attackRange.Value)
                    return;
            }
        
            var coveredCells = GridUtils.GetBoundingBox(gridPos.Value, attackRange.Value);
        
            // Otherwise find a new target
            attackTarget.Value = FindRandomTarget(trans.Value, CellEntityType.Zombie, attackRange.Value,
                CellEntitiesMap,
                coveredCells.ToArray(Allocator.Temp), Random);
        
            coveredCells.Dispose();
        }
    }
}