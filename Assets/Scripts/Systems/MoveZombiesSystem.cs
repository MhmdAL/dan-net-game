using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(MoveZombiesSystem))]
public partial class StopZombieMovementSystem : SystemBase
{
    protected override void OnUpdate()
    {
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
// [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(FindTargetSystem))]
public partial class MoveZombiesSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        // var ecb = new EntityCommandBuffer(Allocator.TempJob);
        // var ecbP = ecb.AsParallelWriter();

        var followPathLookup = GetComponentLookup<FollowPathComponent>();
        var unitTagLookup = GetComponentLookup<UnitTag>();

        Dependency = Entities
            .WithReadOnly(followPathLookup)
            .WithReadOnly(unitTagLookup)
            .WithChangeFilter<AttackTargetData>()
            .WithAll<WaypointBufferComponent>()
            .WithDeferredPlaybackSystem<EndSimulationEntityCommandBufferSystem>()
            .ForEach((Entity ent, EntityCommandBuffer ecb, int entityInQueryIndex, AttackTargetData attackTarget) =>
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
            }).ScheduleParallel(Dependency);

        Dependency.Complete();
        //
        // ecb.Playback(EntityManager);

        // ecb = new EntityCommandBuffer(Allocator.Temp);

        var deltaTime = SystemAPI.Time.DeltaTime;

        var resetToWPLookup = GetComponentLookup<ForceSyncPositionComponent>();

        Dependency = Entities
            .ForEach((Entity ent, ref DynamicBuffer<WaypointBufferComponent> waypoints,
                ref FollowPathComponent followPathComponent,
                ref Translation trans,
                in SpeedData speed) =>
            {
                var nextWaypoint = waypoints[followPathComponent.CurrentWaypoint + 1];

                var dir = nextWaypoint.Position - trans.Value.xy;
                var dir2 = new float2(math.sign(dir.x), math.sign(dir.y));
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

                    // if (resetToWPLookup.TryGetComponent(ent, out var sync))
                    // {
                    //     sync.CurrentWaypoint = followPathComponent.CurrentWaypoint;
                    //     sync.Position = trans.Value.xy;
                    //
                    //     ecb.SetComponentEnabled<ForceSyncPositionComponent>(ent, true);
                    //     ecb.SetComponent(ent, sync);
                    // }
                }
            }).ScheduleParallel(Dependency);

        // Dependency.Complete();

        // Dependency.Complete();
        //
        // ecb.Playback(EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
[UpdateBefore(typeof(SynSystem))]
public partial class ForceMoveZombiesSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var followPathLookup = GetComponentLookup<FollowPathComponent>();

        Entities
            .WithAll<ForceSyncPositionComponent>()
            .ForEach((Entity ent,
                ref Translation trans, in ForceSyncPositionComponent sync) =>
            {
                if (followPathLookup.TryGetComponent(ent, out var followPathComponent))
                {
                    followPathComponent.CurrentWaypoint = sync.CurrentWaypoint;

                    followPathLookup[ent] = followPathComponent;
                }

                // trans.Value = new float3(waypoints[sync.CurrentWaypoint].Position, 0);
                trans.Value = new float3(sync.Position, 0);
                Debug.Log("Syncing: " + sync.Position + "// " + ent);

                ecb.SetComponentEnabled<ForceSyncPositionComponent>(ent, false);
            }).WithoutBurst().Run();

        ecb.Playback(EntityManager);
    }
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(ForceMoveZombiesSystem))]
public partial class PeriodicSyncZombiesSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var forceSyncLookup = GetComponentLookup<ForceSyncPositionComponent>();
        var followPathLookup = GetComponentLookup<FollowPathComponent>();

        var dt = SystemAPI.Time.DeltaTime;

        var random = new Random(1000);

        Entities
            .WithAll<PeriodicZombieSync>()
            .ForEach((Entity ent,
                ref PeriodicZombieSync sync,
                in Translation trans) =>
            {
                sync.CurrentTimer -= dt;

                if (sync.CurrentTimer <= 0 && forceSyncLookup.TryGetComponent(ent, out var forceSync))
                {
                    var interval = random.NextFloat(sync.MinInterval, sync.MaxInterval);

                    sync.CurrentTimer += interval;

                    forceSync.Position = trans.Value.xy;

                    if (followPathLookup.TryGetComponent(ent, out var followPathComponent))
                    {
                        forceSync.CurrentWaypoint = followPathComponent.CurrentWaypoint;
                    }

                    ecb.SetComponentEnabled<ForceSyncPositionComponent>(ent, true);
                    ecb.SetComponent(ent, forceSync);
                }
            }).Run();

        ecb.Playback(EntityManager);
    }
}