using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

// When client has a connection with network id, go in game and tell server to also go in game
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class GoInGameClientSystem : SystemBase
{
    protected override void OnCreate()
    {
        // Make sure we wait with the sub scene containing the prefabs to load before going in-game
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<NetworkIdComponent>(), ComponentType.Exclude<NetworkStreamInGame>()));
    }
    
    protected override void OnUpdate()
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        Entities.WithNone<NetworkStreamInGame>().ForEach((Entity ent, in NetworkIdComponent id) =>
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(ent);
            
            var req = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<GoInGameRequest>(req);
            commandBuffer.AddComponent(req, new SendRpcCommandRequestComponent { TargetConnection = ent });
        }).Run();
        commandBuffer.Playback(EntityManager);
    }
}