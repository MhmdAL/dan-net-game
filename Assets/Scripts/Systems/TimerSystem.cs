using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class TimerSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var dt = SystemAPI.Time.DeltaTime;
        // var random = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(0, 100000));
        var random = new Unity.Mathematics.Random(1000);
        
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        var ecbParallel = ecb.AsParallelWriter();
        
        Dependency = new UpdateAttackTimersJob
        {
            dt = dt,
            ecb = ecbParallel,
            random = random,
            exists = GetComponentLookup<Translation>(true)
        }.ScheduleParallel(Dependency);
        
        Dependency = new UpdateEffectsTimersJob
        {
            dt = dt
        }.ScheduleParallel(Dependency);
        
        Dependency = new UpdateModifiersTimersJob
        {
            dt = dt
        }.ScheduleParallel(Dependency);
        
        Dependency.Complete();
        
        ecb.Playback(EntityManager);
    }

    [BurstCompile]
    public partial struct UpdateAttackTimersJob : IJobEntity
    {
        public float dt;
        [ReadOnly] public ComponentLookup<Translation> exists;
        public Unity.Mathematics.Random random;
        public EntityCommandBuffer.ParallelWriter ecb;

        private void Execute(Entity ent, [EntityInQueryIndex] int entityInQueryIndex, ref AttackData attackData,
            in AttackTargetData attackTarget)
        {
            attackData.CurrentCooldown -= dt;

            if (attackData.CurrentCooldown <= 0)
            {
                if (exists.HasComponent(attackTarget.Value))
                {
                    var rand = random.NextFloat(0, 1f);

                    if (rand <= attackData.HitChance)
                    {
                        ecb.AddComponent(entityInQueryIndex, ent, new PerformAttack { Target = attackTarget.Value });
                    }
                }

                attackData.CurrentCooldown += attackData.MaxCooldown;
            }
        }
    }

    [BurstCompile]
    public partial struct UpdateEffectsTimersJob : IJobEntity
    {
        public float dt;

        private void Execute(ref DynamicBuffer<EffectBufferComponent> effectBuffer)
        {
            var toBeRemoved = new NativeList<int>(Allocator.TempJob);

            for (int i = 0; i < effectBuffer.Length; i++)
            {
                var item = effectBuffer[i];
                item.ElapsedDuration += dt;
                item.ElapsedSinceLastApplication += dt;
                effectBuffer[i] = item;

                if (item.ElapsedDuration > item.Duration)
                {
                    toBeRemoved.Add(i);
                }
            }

            for (int i = toBeRemoved.Length - 1; i >= 0; i--)
            {
                effectBuffer.RemoveAtSwapBack(toBeRemoved[i]);
            }

            toBeRemoved.Dispose();
        }
    }

    [BurstCompile]
    public partial struct UpdateModifiersTimersJob : IJobEntity
    {
        public float dt;

        private void Execute(ref DynamicBuffer<ModifierBufferComponent> modBuffer)
        {
            var toBeRemoved = new NativeList<int>(Allocator.TempJob);

            for (int i = 0; i < modBuffer.Length; i++)
            {
                var item = modBuffer[i];
                item.ElapsedDuration += dt;
                modBuffer[i] = item;

                if (item.ElapsedDuration > item.Duration)
                {
                    toBeRemoved.Add(i);
                }
            }

            for (int i = toBeRemoved.Length - 1; i >= 0; i--)
            {
                modBuffer.RemoveAtSwapBack(toBeRemoved[i]);
            }

            toBeRemoved.Dispose();
        }
    }
}