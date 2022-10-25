using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class MoveZombiesSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = SystemAPI.Time.DeltaTime;
        
        Entities
            .ForEach((Entity ent, ref DynamicBuffer<WaypointBufferComponent> waypoints, ref CurrentWaypointComponent waypoint, ref Translation trans, 
                in SpeedComponent speed) =>
            {
                var nextWaypoint = waypoints[waypoint.Value + 1];

                var dir = nextWaypoint.Position - trans.Value.xy;
                var moveDir = math.normalize(dir) * speed.Value * deltaTime;

                trans.Value += new float3(moveDir, 0);
                
                if (math.length(dir) <= 0.1f)
                {
                    Debug.Log("waypoint reached");
                    waypoint.Value += 1;
                }

                if (waypoint.Value == waypoints.Length - 1)
                {
                    waypoint.Value = 0;
                    trans.Value = new float3(waypoints[0].Position, 0);
                }
            }).Run();

    }
}