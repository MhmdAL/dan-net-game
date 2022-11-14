using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class MoveZombiesSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        var followPathLookup = GetComponentLookup<FollowPathComponent>();
        var unitTagLookup = GetComponentLookup<UnitTag>();

        Entities
            .WithReadOnly(followPathLookup)
            .WithReadOnly(unitTagLookup)
            .WithAll<WaypointBufferComponent>()
            .ForEach((Entity ent, in AttackTargetData attackTarget) =>
            {
                if (unitTagLookup.HasComponent(attackTarget.Value))
                {
                    followPathLookup.SetComponentEnabled(ent, false);
                }
                else
                {
                    followPathLookup.SetComponentEnabled(ent, true);
                }
            }).Run();

        var deltaTime = SystemAPI.Time.DeltaTime;

        Entities
            .ForEach((ref DynamicBuffer<WaypointBufferComponent> waypoints, ref FollowPathComponent followPathComponent,
                ref Translation trans,
                in SpeedComponent speed) =>
            {
                var nextWaypoint = waypoints[followPathComponent.CurrentWaypoint + 1];

                var dir = nextWaypoint.Position - trans.Value.xy;
                var moveDir = math.normalize(dir) * speed.Value * deltaTime;

                trans.Value += new float3(moveDir, 0);

                if (math.lengthsq(dir) <= 0.01f)
                {
                    followPathComponent.CurrentWaypoint += 1;
                }

                if (followPathComponent.CurrentWaypoint == waypoints.Length - 1)
                {
                    followPathComponent.CurrentWaypoint = 0;
                    trans.Value = new float3(waypoints[0].Position, 0);
                }
            }).Run();
    }
}