using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[BurstCompile]
public partial struct MoveHeroSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<Simulate>()
            .WithAll<HeroInput>()
            .WithAllRW<Translation>();
        var query = state.GetEntityQuery(builder);
        state.RequireForUpdate(query);
    }
    
    [BurstCompile]
    public void OnDestroy(ref SystemState state)
    {
    }
    
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var moveJob = new MoveHeroJob
        {
            deltaTime = SystemAPI.Time.DeltaTime
        };
        state.Dependency = moveJob.ScheduleParallel(state.Dependency);
    }

    [BurstCompile]
    [WithAll(typeof(Simulate))]
    partial struct MoveHeroJob : IJobEntity
    {
        public float deltaTime;
        public void Execute(HeroInput playerInput, ref Translation trans, in SpeedComponent speed)
        {
            var moveInput = new float2(playerInput.Horizontal, playerInput.Vertical);
            moveInput = math.normalizesafe(moveInput) * speed.Value * deltaTime;
            trans.Value += new float3(moveInput.x, moveInput.y, 0);
        }
    }
}
