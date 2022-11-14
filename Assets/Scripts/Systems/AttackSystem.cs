using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class AttackSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var dt = SystemAPI.Time.DeltaTime;
        
        Entities.ForEach((ref AttackData attackTargetComponent) =>
        {
            attackTargetComponent.CurrentCooldown -= dt;
        }).Run();

        var damageBufferLookup = GetBufferLookup<DamageBuffer>();

        Entities.ForEach((ref AttackData attackData, in AttackTargetData attackTarget) =>
        {
            DynamicBuffer<DamageBuffer> damageBuffer;
            if (attackData.CurrentCooldown <= 0 && damageBufferLookup.TryGetBuffer(attackTarget.Value, out damageBuffer))
            {
                damageBuffer.Add(new DamageBuffer() { Value = attackData.Damage });

                attackData.CurrentCooldown = attackData.MaxCooldown;
            }
        }).Schedule();
    }
}