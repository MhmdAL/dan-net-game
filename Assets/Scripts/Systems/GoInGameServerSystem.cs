using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;

// When server receives go in game request, go in game and delete request
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class GoInGameServerSystem : SystemBase
{
    protected override void OnCreate()
    {
        RequireForUpdate<SpawnerComponent>();
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<GoInGameRequest>(), ComponentType.ReadOnly<ReceiveRpcCommandRequestComponent>()));
    }

    protected override void OnUpdate()
    {
        var prefab = GetSingleton<SpawnerComponent>().SquarePrefab;
        var networkIdLookup = GetComponentLookup<NetworkIdComponent>(true);
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        Entities.WithNone<SendRpcCommandRequestComponent>().ForEach((Entity reqEnt, in GoInGameRequest req, in ReceiveRpcCommandRequestComponent reqSrc) =>
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(reqSrc.SourceConnection);
            UnityEngine.Debug.Log(string.Format("Server setting connection {0} to in game", networkIdLookup[reqSrc.SourceConnection].Value));

            var player = commandBuffer.Instantiate(prefab);
            commandBuffer.SetComponent(player, new GhostOwnerComponent { NetworkId = networkIdLookup[reqSrc.SourceConnection].Value});
            commandBuffer.AddBuffer<CubeInput>(player);

            // commandBuffer.SetComponent(reqSrc.SourceConnection, new CommandTargetComponent {targetEntity = player});

            commandBuffer.DestroyEntity(reqEnt);
        }).Run();
        commandBuffer.Playback(EntityManager);
    }
}