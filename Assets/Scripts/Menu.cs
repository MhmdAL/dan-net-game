using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.SceneManagement;
using Unity.Networking.Transport;

public class Menu : MonoBehaviour
{
    public string IP = "127.0.0.1";
    public ushort Port = 7979;
    
    public void StartClientServer()
    {
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");

        SceneManager.LoadScene("GameScene");

        NetworkEndpoint ep = NetworkEndpoint.AnyIpv4.WithPort(Port);
        {
            using var drvQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(ep);
        }

        ep = NetworkEndpoint.LoopbackIpv4.WithPort(Port);
        {
            using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, ep);
        }

    }

    public void ConnectToServer()
    {
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        SceneManager.LoadScene("GameScene");

        var ep = NetworkEndpoint.Parse(IP, Port);
        {
            using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, ep);
        }
    }
}
