using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.SceneManagement;
using Unity.Networking.Transport;

public class Menu : MonoBehaviour
{
    public void StartClientServer()
    {
        var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        SceneManager.LoadScene("SampleScene");

        NetworkEndpoint ep = NetworkEndpoint.AnyIpv4.WithPort(7979);
        {
            using var drvQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(ep);
        }

        ep = NetworkEndpoint.LoopbackIpv4.WithPort(7979);
        {
            using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, ep);
        }

    }

    public void ConnectToServer()
    {
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        SceneManager.LoadScene("SampleScene");

        var ep = NetworkEndpoint.Parse("127.0.0.1", 7979);
        {
            using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, ep);
        }
    }
}
