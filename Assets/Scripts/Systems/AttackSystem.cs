using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(TimerSystem))]
public partial class AttackSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var ecb = new EntityCommandBuffer(Allocator.Temp);

        var damageBufferLookup = GetBufferLookup<DamageBuffer>();
        var modBufferLookup = GetBufferLookup<ModifierBufferComponent>();

        var random = new Random((uint)UnityEngine.Random.Range(0, 1000000));

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
                var damage = random.NextInt(fireEffect.MinDamage, fireEffect.MaxDamage + 1);

                ecb.AddComponent(attackTarget.Value, new ApplyEffect
                {
                    Effect = new EffectBufferComponent
                    {
                        Duration = fireEffect.Duration,
                        Rate = fireEffect.Rate,
                        Value = damage,
                        ElapsedSinceLastApplication = fireEffect.Rate,
                        Type = EffectType.FireDoT,
                        SourceId = ent.Index,
                    }
                });
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

        foreach (var (perf, ent) in SystemAPI.Query<PerformAttack>().WithEntityAccess())
        {
            ecb.RemoveComponent<PerformAttack>(ent);
        }

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