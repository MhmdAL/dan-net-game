using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(UnitDamageSystem))]
public partial class UnitDeathSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithDeferredPlaybackSystem<EndSimulationEntityCommandBufferSystem>()
            .ForEach((Entity ent, EntityCommandBuffer ecb, in HealthComponent healthComp) =>
        {
            if (healthComp.CurrentHealth <= 0)
            {
                ecb.DestroyEntity(ent);
            }
        }).Schedule();
    }
}
