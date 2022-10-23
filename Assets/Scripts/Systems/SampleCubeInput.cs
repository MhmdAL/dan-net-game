using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Unity.Collections;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class SampleCubeInput : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<NetworkIdComponent>();
    }

    protected override void OnUpdate()
    {
        var localInput = GetSingleton<CommandTargetComponent>().targetEntity;
        if (localInput == Entity.Null)
        {
            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            var localPlayerId = GetSingleton<NetworkIdComponent>().Value;
            var commandTargetEntity = GetSingletonEntity<CommandTargetComponent>();
            Entities
                .WithAll<MovableCubeComponent>()
                .WithNone<CubeInput>()
                .ForEach((Entity ent, in GhostOwnerComponent ghostOwner) =>
                {
                    if (ghostOwner.NetworkId == localPlayerId)
                    {
                        commandBuffer.AddBuffer<CubeInput>(ent);
                        commandBuffer.SetComponent(commandTargetEntity, new CommandTargetComponent { targetEntity = ent });
                    }
                }).Run();
            
            commandBuffer.Playback(EntityManager);
            return;
        }
        var input = default(CubeInput);
        input.Tick = GetSingleton<NetworkTime>().ServerTick;
        if (Input.GetKey("a"))
            input.horizontal -= 1;
        if (Input.GetKey("d"))
            input.horizontal += 1;
        if (Input.GetKey("s"))
            input.vertical -= 1;
        if (Input.GetKey("w"))
            input.vertical += 1;
        var inputBuffer = EntityManager.GetBuffer<CubeInput>(localInput);
        inputBuffer.AddCommandData(input);
    }
}