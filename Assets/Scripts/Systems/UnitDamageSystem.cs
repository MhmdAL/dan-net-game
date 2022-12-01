using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(AttackSystem))]
[UpdateAfter(typeof(EffectSystem))]
public partial class UnitDamageSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((ref HealthComponent healthComp, ref DynamicBuffer<DamageBuffer> damageBuffer) =>
        {
            foreach (var dmg in damageBuffer)
            {
                healthComp.CurrentHealth -= dmg.Value;
            }
            
            damageBuffer.Clear();
        }).Run();
    }
}