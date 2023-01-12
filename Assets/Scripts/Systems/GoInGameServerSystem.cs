using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.VisualScripting;
using UnityEngine;

public struct SynSystemData : IComponentData
{
    public bool Active;
    public float Timer;
}

public struct ZombieSyncData
{
    public Entity Entity;
    public float2 Position;
    public byte CurrentWaypoint;
}

// public struct SyncZombiesCommand : IRpcCommand
// {
//     public ZombieSyncData[] SyncData;
// }

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateBefore(typeof(GoInGameServerSystem))]
public partial class SynSystem : SystemBase
{
    protected override void OnCreate()
    {
        EntityManager.AddComponent<SynSystemData>(SystemHandle);
        EntityManager.SetComponentData(SystemHandle,
            new SynSystemData
                { Active = false, Timer = 0});
        
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        var state = EntityManager.GetComponentData<SynSystemData>(SystemHandle);
        
        // Debug.Log(state.Active + "/" + state.Timer);

        if (state.Active)
        {
            state.Timer -= SystemAPI.Time.DeltaTime;
        }

        if (state.Active && state.Timer <= 0)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);

            var zombieSyncDatas = new NativeList<ZombieSyncData>(0, Allocator.Temp);

            var fpLookup = GetComponentLookup<FollowPathComponent>();

            foreach (var (tag, trans, ent) in SystemAPI.Query<ZombieTag, Translation>().WithEntityAccess())
            {
                var newSync = new ForceSyncPositionComponent();
                newSync.Position = trans.Value.xy;
                
                if (fpLookup.TryGetComponent(ent, out var fp))
                {
                    newSync.CurrentWaypoint = fp.CurrentWaypoint;
                }
                
                // zombieSyncDatas.Add(new ZombieSyncData()
                // {
                //     Entity = ent,
                //     Position = newSync.Position,
                //     CurrentWaypoint = newSync.CurrentWaypoint
                // });

                // Debug.Log("Forcing sync on: " + ent);
                
                commandBuffer.SetComponentEnabled<ForceSyncPositionComponent>(ent, true);
                commandBuffer.SetComponent(ent, newSync);
            }

            // var test = new ZombieSyncData[1];
            // test[0] = zombieSyncDatas[0];
            //
            // var req = EntityManager.CreateEntity(typeof(SendRpcCommandRequestComponent), typeof(SyncZombiesCommand));
            // EntityManager.SetComponentData(req, new SyncZombiesCommand { SyncData = test});
            //
            // Debug.Log($"Trying to sync {zombieSyncDatas.Length} zombies");
            
            commandBuffer.Playback(EntityManager);

            state.Active = false;
        }
        
        EntityManager.SetComponentData(SystemHandle, state);
    }
}

// When server receives go in game request, go in game and delete request
[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial struct GoInGameServerSystem : ISystem
{
    private ComponentLookup<NetworkIdComponent> networkIdFromEntity;

    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<HeroSpawner>();
        
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRequest>()
            .WithAll<ReceiveRpcCommandRequestComponent>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
        networkIdFromEntity = state.GetComponentLookup<NetworkIdComponent>(true);
    }

    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }

    // [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var prefab = SystemAPI.GetSingleton<HeroSpawner>().HeroPrefab;
        
        state.EntityManager.GetName(prefab, out var prefabName);
        var worldName = state.WorldUnmanaged.Name;

        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        networkIdFromEntity.Update(ref state);

        foreach (var (reqSrc, reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequestComponent>>().WithAll<GoInGameRequest>().WithEntityAccess())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
            var networkIdComponent = networkIdFromEntity[reqSrc.ValueRO.SourceConnection];

            UnityEngine.Debug.Log($"'{worldName}' setting connection '{networkIdComponent.Value}' to in game, spawning a Ghost '{prefabName}' for them!");

            var player = commandBuffer.Instantiate(prefab);
            commandBuffer.SetComponent(player, new GhostOwnerComponent { NetworkId = networkIdComponent.Value});

            commandBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup{Value = player});
            
            commandBuffer.DestroyEntity(reqEntity);
        }
        
        var syncSystemData = state.EntityManager.GetComponentData<SynSystemData>(state.World.GetExistingSystem(typeof(SynSystem)));

        syncSystemData.Active = true;
        syncSystemData.Timer = 2f;
        
        state.EntityManager.SetComponentData<SynSystemData>(state.World.GetExistingSystem(typeof(SynSystem)), syncSystemData);

        commandBuffer.Playback(state.EntityManager);
    }
}

// [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
// public partial class SyncZombiesSystem : SystemBase
// {
//     protected override void OnUpdate()
//     {
//         var ecb = new EntityCommandBuffer(Allocator.TempJob);
//         
//         new ReceiveSyncZombiesCommandJob
//         {
//             ecb = ecb
//         }.Run();
//         
//         ecb.Playback(EntityManager);
//     }
// }
//
// public partial struct ReceiveSyncZombiesCommandJob : IJobEntity
// {
//     public EntityCommandBuffer ecb;
//
//     public void Execute(Entity ent, ref SyncZombiesCommand command, ref ReceiveRpcCommandRequestComponent req)
//     {
//         ecb.DestroyEntity(ent);
//
//         for (int i = 0; i < command.SyncData.Length; i++)
//         {
//             ecb.SetComponent(command.SyncData[i].Entity, new Translation {Value = new float3(command.SyncData[i].Position, 0) });
//             ecb.SetComponent(command.SyncData[i].Entity, new FollowPathComponent { CurrentWaypoint = command.SyncData[i].CurrentWaypoint });
//         }
//         
//         Debug.Log($"Synced {command.SyncData.Length} zombies");
//     }
// }