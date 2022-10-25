using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class SpawnZombiesSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);
        var deltaTime = SystemAPI.Time.DeltaTime;

        Entities
            .ForEach((ref ZombieSpawnerComponent spawner, in Translation trans) =>
            {
                spawner.TimeTillNextSpawn -= deltaTime;

                if (spawner.TimeTillNextSpawn <= 0)
                {
                    var zombie = ecb.Instantiate(spawner.Prefab);
                    ecb.AddComponent(zombie, new SpeedComponent { Value = Random.Range(1f, 4f) });
                    ecb.AddComponent(zombie, new CurrentWaypointComponent { Value = 0 });

                    var waypoints = ecb.AddBuffer<WaypointBufferComponent>(zombie);

                    waypoints.Length = 6;
                    waypoints[0] = new WaypointBufferComponent { Position = new float2(-4, 4) };
                    waypoints[1] = new WaypointBufferComponent { Position = new float2(-4, 0) };
                    waypoints[2] = new WaypointBufferComponent { Position = new float2(0, -4) };
                    waypoints[3] = new WaypointBufferComponent { Position = new float2(0, 4) };
                    waypoints[4] = new WaypointBufferComponent { Position = new float2(4, 4) };
                    waypoints[5] = new WaypointBufferComponent { Position = new float2(4, -4) };

                    ecb.SetComponent(zombie, new Translation { Value = new float3(waypoints[0].Position, 0) });

                    spawner.TimeTillNextSpawn = 5;
                }
            }).Run();

        ecb.Playback(EntityManager);
    }
}