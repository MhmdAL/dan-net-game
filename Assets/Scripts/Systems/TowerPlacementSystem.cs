using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Transforms;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class TowerPlacementSystem : SystemBase
{

    protected override void OnCreate()
    {
        RequireForUpdate<TowerSpawnerComponent>();
    }

    protected override void OnUpdate()
    {
        var towerSpawner = SystemAPI.GetSingleton<TowerSpawnerComponent>();

        var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.TempJob);

        new ReceivePlaceTowerCommandJob
        {
            ecb = ecb,
            fireTowerPrefab = towerSpawner.FireTowerPrefab,
            slowTowerPrefab = towerSpawner.SlowTowerPrefab
        }.Run();

        ecb.Playback(EntityManager);
        ecb.Dispose();
    }

    private partial struct ReceivePlaceTowerCommandJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public Entity fireTowerPrefab;
        public Entity slowTowerPrefab;

        public void Execute(Entity ent, ref PlaceTowerCommand command, ref ReceiveRpcCommandRequestComponent req)
        {
            ecb.DestroyEntity(ent);

            Debug.Log("Placing tower");

            var pos = GridUtils.GetCellWorldPosition(command.Cell);

            Entity tower;
            if (command.Type == TowerType.Fire)
                tower = ecb.Instantiate(fireTowerPrefab);
            else
                tower = ecb.Instantiate(slowTowerPrefab);
            
            ecb.SetComponent(tower, new Translation { Value = new float3(pos, 0) });
        }
    }

    [WithAll(typeof(Simulate))]
    partial struct PlaceTowerJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public Entity towerPrefab;

        public void Execute(TowerPlacementInput towerInput)
        {
            if (!towerInput.PlaceTower.IsSet)
                return;

            Debug.Log("Placing tower");

            var pos = GridUtils.GetCellWorldPosition(towerInput.Cell);

            var tower = ecb.Instantiate(towerPrefab);
            ecb.SetComponent(tower, new Translation { Value = new float3(pos, 0) });
        }
    }
}
