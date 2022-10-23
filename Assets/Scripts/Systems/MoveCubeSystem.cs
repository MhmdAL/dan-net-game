using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Unity.Collections;
using UnityEngine;

[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
public partial class MoveCubeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var tick = GetSingleton<NetworkTime>().ServerTick;
        var deltaTime = SystemAPI.Time.DeltaTime;
        Entities.WithAll<Simulate>().ForEach((ref DynamicBuffer<CubeInput> inputBuffer, ref Translation trans) =>
        {
            inputBuffer.GetDataAtTick(tick, out var input);
            
            if (input.horizontal > 0)
                trans.Value.x += deltaTime;
            if (input.horizontal < 0)
                trans.Value.x -= deltaTime;
            if (input.vertical > 0)
                trans.Value.y += deltaTime;
            if (input.vertical < 0)
                trans.Value.y -= deltaTime;
            
        }).ScheduleParallel();
    }
}