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

            var fpLookup = GetComponentLookup<FollowPathComponent>();

            foreach (var (tag, trans, ent) in SystemAPI.Query<ZombieTag, Translation>().WithEntityAccess())
            {
                var newSync = new ForceSyncPositionComponent();
                newSync.Position = trans.Value.xy;
                
                if (fpLookup.TryGetComponent(ent, out var fp))
                {
                    newSync.CurrentWaypoint = fp.CurrentWaypoint;
                }

                // Debug.Log("Forcing sync on: " + ent);
                
                commandBuffer.SetComponent(ent, newSync);
                commandBuffer.SetComponentEnabled<ForceSyncPositionComponent>(ent, true);
            }
        
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
        syncSystemData.Timer = 1f;
        
        state.EntityManager.SetComponentData<SynSystemData>(state.World.GetExistingSystem(typeof(SynSystem)), syncSystemData);

        commandBuffer.Playback(state.EntityManager);
    }
}

