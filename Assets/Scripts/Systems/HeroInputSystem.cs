using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct HeroInputSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<NetworkIdComponent>();
    }

    public void OnDestroy(ref SystemState state)
    {
    }

    public void OnUpdate(ref SystemState state)
    {
        foreach (var playerInput in SystemAPI.Query<RefRW<HeroInput>>().WithAll<GhostOwnerIsLocal>())
        {
            playerInput.ValueRW = default;
            if (Input.GetKey("left"))
                playerInput.ValueRW.Horizontal -= 1;
            if (Input.GetKey("right"))
                playerInput.ValueRW.Horizontal += 1;
            if (Input.GetKey("down"))
                playerInput.ValueRW.Vertical -= 1;
            if (Input.GetKey("up"))
                playerInput.ValueRW.Vertical += 1;
        }
    }
}
