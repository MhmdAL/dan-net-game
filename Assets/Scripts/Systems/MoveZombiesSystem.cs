using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(MoveZombiesSystem))]
public partial class StopZombieMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var deltaTime = SystemAPI.Time.DeltaTime;

        var resetToWPLookup = GetComponentLookup<ForceSyncPositionComponent>();

        Entities
            .ForEach((Entity ent, ref DynamicBuffer<WaypointBufferComponent> waypoints,
                ref FollowPathComponent followPathComponent,
                ref Translation trans,
                in SpeedData speed) =>
            {
                // var nextWaypoint = waypoints[followPathComponent.CurrentWaypoint + 1];
                //
                // var dir = nextWaypoint.Position - trans.Value.xy;
                //
                // if (math.lengthsq(dir) <= 0.01f)
                // {
                //     followPathComponent.CurrentWaypoint += 1;
                //     // Debug.Log("Arrived at waypoint: " + followPathComponent.CurrentWaypoint);
                //
                //     if (followPathComponent.CurrentWaypoint >= waypoints.Length - 1)
                //     {
                //         followPathComponent.CurrentWaypoint = 0;
                //         trans.Value = new float3(waypoints[followPathComponent.CurrentWaypoint].Position, 0);
                //     }
                //
                //     if (resetToWPLookup.TryGetComponent(ent, out var sync))
                //     {
                //         sync.CurrentWaypoint = followPathComponent.CurrentWaypoint;
                //     
                //         ecb.SetComponentEnabled<ForceSyncPositionComponent>(ent, true);
                //         ecb.SetComponent(ent, sync);
                //     }
                // }

                // if (followPathComponent.CurrentWaypoint == waypoints.Length - 1)
                // {
                //     followPathComponent.CurrentWaypoint = 0;
                //     trans.Value = new float3(waypoints[followPathComponent.CurrentWaypoint].Position, 0);
                //
                //     if (resetToWPLookup.TryGetComponent(ent, out var sync))
                //     {
                //         sync.CurrentWaypoint = followPathComponent.CurrentWaypoint;
                //
                //         ecb.SetComponentEnabled<ForceSyncPositionComponent>(ent, true);
                //         ecb.SetComponent(ent, sync);
                //     }
                // }
            }).Run();
        
        ecb.Playback(EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateBefore(typeof(FindTargetSystem))]
public partial class MoveZombiesSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var followPathLookup = GetComponentLookup<FollowPathComponent>();
        var unitTagLookup = GetComponentLookup<UnitTag>();

        Entities
            .WithReadOnly(followPathLookup)
            .WithReadOnly(unitTagLookup)
            .WithChangeFilter<AttackTargetData>()
            .WithAll<WaypointBufferComponent>()
            .ForEach((Entity ent, in AttackTargetData attackTarget) =>
            {
                if (unitTagLookup.HasComponent(attackTarget.Value))
                {
                    if (followPathLookup.TryGetComponent(ent, out var fp) && followPathLookup.IsComponentEnabled(ent))
                    {
                        ecb.SetComponentEnabled<FollowPathComponent>(ent, false);
                    }
                }
                else
                {
                    if (followPathLookup.TryGetComponent(ent, out var fp) && !followPathLookup.IsComponentEnabled(ent))
                    {
                        ecb.SetComponentEnabled<FollowPathComponent>(ent, true);
                    }
                }
            }).Run();

        ecb.Playback(EntityManager);

        ecb = new EntityCommandBuffer(Allocator.Temp);

        var deltaTime = SystemAPI.Time.DeltaTime;

        var resetToWPLookup = GetComponentLookup<ForceSyncPositionComponent>();

        Entities
            .ForEach((Entity ent, ref DynamicBuffer<WaypointBufferComponent> waypoints,
                ref FollowPathComponent followPathComponent,
                ref Translation trans,
                in SpeedData speed) =>
            {
                var nextWaypoint = waypoints[followPathComponent.CurrentWaypoint + 1];

                var dir = nextWaypoint.Position - trans.Value.xy;
                var moveDir = math.normalize(dir) * speed.CurrentValue * deltaTime;
                
                // Debug.Log(ent + "//" + moveDir);

                trans.Value += new float3(moveDir, 0);
                
                if (math.lengthsq(dir) <= 0.01f)
                {
                    followPathComponent.CurrentWaypoint += 1;
                    // Debug.Log("Arrived at waypoint: " + followPathComponent.CurrentWaypoint);

                    if (followPathComponent.CurrentWaypoint >= waypoints.Length - 1)
                    {
                        followPathComponent.CurrentWaypoint = 0;
                        trans.Value = new float3(waypoints[followPathComponent.CurrentWaypoint].Position, 0);
                    }

                    if (resetToWPLookup.TryGetComponent(ent, out var sync))
                    {
                        sync.CurrentWaypoint = followPathComponent.CurrentWaypoint;
                        sync.Position = trans.Value.xy;
                    
                        ecb.SetComponentEnabled<ForceSyncPositionComponent>(ent, true);
                        ecb.SetComponent(ent, sync);
                    }
                }
            }).Run();

        ecb.Playback(EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateBefore(typeof(MoveZombiesSystem))]
public partial class ForceMoveZombiesSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        Entities
            .WithAll<ForceSyncPositionComponent>()
            .ForEach((Entity ent, ref DynamicBuffer<WaypointBufferComponent> waypoints,
                ref FollowPathComponent followPathComponent,
                ref Translation trans, in ForceSyncPositionComponent sync) =>
            {
                followPathComponent.CurrentWaypoint = sync.CurrentWaypoint;
                // trans.Value = new float3(waypoints[sync.CurrentWaypoint].Position, 0);
                trans.Value = new float3(sync.Position, 0);
                // Debug.Log("Syncing: " + sync.Position + "// " + ent);

                ecb.SetComponentEnabled<ForceSyncPositionComponent>(ent, false);
            }).Run();

        ecb.Playback(EntityManager);
    }
}