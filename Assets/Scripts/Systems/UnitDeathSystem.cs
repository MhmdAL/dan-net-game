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
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        
        Entities.ForEach((Entity ent, in HealthComponent healthComp) =>
        {
            if (healthComp.CurrentHealth <= 0)
            {
                commandBuffer.DestroyEntity(ent);
            }
        }).Run();
        
        commandBuffer.Playback(EntityManager);
    }
}
