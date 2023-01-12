using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
public partial class SpawnZombiesSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // var ecb = new EntityCommandBuffer(Allocator.Temp);
        var deltaTime = SystemAPI.Time.fixedDeltaTime;

        var waypointBufferLookup = GetBufferLookup<WaypointBufferComponent>();
        
        Entities
            .WithDeferredPlaybackSystem<EndSimulationEntityCommandBufferSystem>()
            .ForEach((EntityCommandBuffer ecb, ref ZombieSpawnerComponent spawner, in Translation trans) =>
            {
                spawner.TimeTillNextSpawn -= deltaTime;

                if (spawner.TimeTillNextSpawn <= 0)
                {
                    var zombie = ecb.Instantiate(spawner.Prefab);
                    
                    var speed = 5;

                    ecb.AddComponent(zombie, new SpeedData { OriginalValue = speed, CurrentValue = speed });

                    if (waypointBufferLookup.TryGetBuffer(spawner.Prefab, out var wpBuffer))
                    {
                        ecb.SetComponent(zombie, new Translation { Value = new float3(wpBuffer[0].Position, 0) });
                    }

                    spawner.TimeTillNextSpawn = spawner.SpawnCooldown;
                }
            }).Schedule();

        // ecb.Playback(EntityManager);
    }
}