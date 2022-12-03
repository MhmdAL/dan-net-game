
using System;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(InitializationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class SpriteBugFixSystem : SystemBase
{
    private EntityQuery _players, _sprites;
    
    protected override void OnCreate()
    {
        _players = GetEntityQuery(typeof(CompanionLink));
        _sprites = GetEntityQuery(ComponentType.ReadOnly<SpriteRenderer>());
        
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        EntityManager.RemoveComponent(_players, typeof(CompanionLink));
        EntityManager.RemoveComponent<SpriteRenderer>(_sprites);
    }
}
