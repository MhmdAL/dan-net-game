using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateBefore(typeof(UnitDamageSystem))]
[UpdateAfter(typeof(FindTargetSystem))]
public partial class AttackSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var damageBufferLookup = GetBufferLookup<DamageBuffer>();
        var effectBufferLookup = GetBufferLookup<EffectBufferComponent>();
        var modBufferLookup = GetBufferLookup<ModifierBufferComponent>();

        // var random = new Random((uint)UnityEngine.Random.Range(1, 1000000));
        var random = new Random(1000);

        Entities
            .WithAll<PerformAttack>()
            .ForEach((Entity ent, in AttackData attackData, in AttackTargetData attackTarget) =>
            {
                if (damageBufferLookup.TryGetBuffer(attackTarget.Value, out var damageBuffer))
                {
                    damageBuffer.Add(new DamageBuffer { Value = attackData.Damage });
                }
            }).Run();
        
        Entities
            .WithAll<PerformAttack>()
            .ForEach((Entity ent, in AttackTargetData attackTarget, in ApplyFireEffectOnAttack fireEffect) =>
            {
                if (effectBufferLookup.TryGetBuffer(attackTarget.Value, out var effectBuffer))
                {
                    var damage = random.NextInt(fireEffect.MinDamage, fireEffect.MaxDamage + 1);
        
                    effectBuffer.Add(new EffectBufferComponent()
                    {
                        Duration = fireEffect.Duration,
                        Rate = fireEffect.Rate,
                        Value = damage,
                        ElapsedSinceLastApplication = fireEffect.Rate,
                        Type = EffectType.FireDoT,
                        SourceId = ent.Index,
                    });
                }
            }).Run();
        
        Entities
            .WithAll<PerformAttack>()
            .ForEach((Entity ent, in AttackTargetData attackTarget, in ApplySlowOnAttack slowMod) =>
            {
                if (modBufferLookup.TryGetBuffer(attackTarget.Value, out var modBuffer))
                {
                    var slow = random.NextFloat(slowMod.MinSlow, slowMod.MaxSlow);
        
                    modBuffer.Add(new ModifierBufferComponent
                    {
                        Duration = slowMod.Duration,
                        Value = slow,
                        Type = ModifierType.Slow,
                        SourceId = ent.Index,
                    });
                }
            }).Run();
        
        Entities
            .WithAll<PerformAttack>()
            .ForEach((Entity ent) =>
            {
                ecb.RemoveComponent<PerformAttack>(ent);
            }).Run();
        
        ecb.Playback(EntityManager);
    }
}

public struct PerformAttack : IComponentData
{
    public Entity Target;
}

public struct ApplyEffect : IComponentData
{
    public EffectBufferComponent Effect;
}