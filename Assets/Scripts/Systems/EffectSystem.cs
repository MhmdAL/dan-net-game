using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using UnityEngine;

public struct EffectsData : IComponentData
{
    public NativeHashMap<int, int> MaxStacksMap;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(TimerSystem))]
public partial class EffectSystem : SystemBase
{
    protected override void OnCreate()
    {
        var maxStacksMap = new NativeHashMap<int, int>(1, Allocator.Persistent);
        maxStacksMap.Add((int)EffectType.FireDoT, 3);
        
        EntityManager.AddComponent<EffectsData>(SystemHandle);
        EntityManager.SetComponentData(SystemHandle,
            new EffectsData
                { MaxStacksMap = maxStacksMap });

        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        var state = EntityManager.GetComponentData<EffectsData>(SystemHandle);

        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        
        new CalculateEffectsJob
        {
            ecb = ecb,
            maxStacksMap = state.MaxStacksMap
        }.Run();

        ecb.Playback(EntityManager);

        ecb = new EntityCommandBuffer(Allocator.TempJob);

        new ApplyFireEffectJob
        {
            entityCommandBuffer = ecb
        }.Run();

        ecb.Playback(EntityManager);
    }
    
    /// <summary>
    /// Given a buffer of effects on an entity, calculate the net effect based on the type of effect and priorities
    /// </summary>
    [BurstCompile]
    public partial struct CalculateEffectsJob : IJobEntity
    {
        public EntityCommandBuffer ecb;
        public NativeHashMap<int, int> maxStacksMap;
        
        private static NativeHashMap<int, NativeList<(int, EffectBufferComponent)>> SplitEffects(DynamicBuffer<EffectBufferComponent> effectBuffer)
        {
            var map = new NativeHashMap<int, NativeList<(int, EffectBufferComponent)>>(0, Allocator.TempJob);
            
            for (int i = 0; i < effectBuffer.Length; i++)
            {
                var effect = effectBuffer[i];
                
                if (map.TryGetValue((int)effect.Type, out var effects))
                {
                    effects.Add((i, effect));
                }
                else
                {
                    var newList = new NativeList<(int, EffectBufferComponent)>(Allocator.TempJob);
                    newList.Add((i, effect));
                    
                    map.Add((int)effect.Type, newList);
                }
            }

            return map;
        }
        
        private static float CalculateTotalFireDamage(NativeList<(int, EffectBufferComponent)> fireEffects,
            DynamicBuffer<EffectBufferComponent> effectBuffer,
            int maxStacks)
        {
            fireEffects.Sort(new EffectSort());

            var totalFireDamage = 0f;
            for (int i = 0; i < fireEffects.Length && i < maxStacks; i++)
            {
                var effect = fireEffects[i].Item2;

                if (effect.ElapsedSinceLastApplication >= effect.Rate)
                {
                    totalFireDamage += effect.Value;

                    effect.ElapsedSinceLastApplication -= effect.Rate;
                    
                    effectBuffer[fireEffects[i].Item1] = effect;
                }
            }

            return totalFireDamage;
        }

        private void Execute(Entity ent, ref DynamicBuffer<EffectBufferComponent> effectBuffer)
        {
            var effectsMap = SplitEffects(effectBuffer);
            
            if (effectsMap.TryGetValue((int)EffectType.FireDoT, out var fireEffects))
            {
                var totalFireDamage = CalculateTotalFireDamage(fireEffects, effectBuffer, maxStacksMap[(int)EffectType.FireDoT]);

                ecb.AddComponent(ent, new ApplyFireDoT { Damage = totalFireDamage });

                fireEffects.Dispose();
            }

            effectsMap.Dispose();
        }
    }
    
    [BurstCompile]
    public partial struct ApplyFireEffectJob : IJobEntity
    {
        public EntityCommandBuffer entityCommandBuffer;

        private void Execute(Entity ent, ref DynamicBuffer<DamageBuffer> damageBuffer, in ApplyFireDoT effect)
        {
            damageBuffer.Add(new DamageBuffer { Value = (int)effect.Damage });

            entityCommandBuffer.RemoveComponent<ApplyFireDoT>(ent);
        }
    }

    public struct ApplyFireDoT : IComponentData, IEnableableComponent
    {
        public float Damage;
    }

    public struct EffectSort : IComparer<(int, EffectBufferComponent)>
    {
        public int Compare((int, EffectBufferComponent) a, (int, EffectBufferComponent) b)
        {
            var aDPS = a.Item2.Value / a.Item2.Rate;
            var bDPS = b.Item2.Value / b.Item2.Rate;

            var diff = aDPS - bDPS;
            
            if (diff < 0f || diff == 0f && a.Item2.SourceId > b.Item2.SourceId)
            {
                return 1;
            }
            
            return -1;
        }
    }
}