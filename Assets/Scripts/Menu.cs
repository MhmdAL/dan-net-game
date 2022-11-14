using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine.SceneManagement;
using Unity.Networking.Transport;

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_InputField ipField;
    [SerializeField] private TMP_InputField portField;
    
    public void StartClientServer()
    {
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");
        var server = ClientServerBootstrap.CreateServerWorld("ServerWorld");

        SceneManager.LoadScene("GameScene");

        var port = ushort.Parse(portField.text);

        NetworkEndpoint ep = NetworkEndpoint.AnyIpv4.WithPort(port);
        {
            using var drvQuery = server.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(ep);
        }

        ep = NetworkEndpoint.LoopbackIpv4.WithPort(port);
        {
            using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, ep);
        }

    }

    public void ConnectToServer()
    {
        var client = ClientServerBootstrap.CreateClientWorld("ClientWorld");

        SceneManager.LoadScene("GameScene");

        var ip = ipField.text;
        var port = ushort.Parse(portField.text);

        var ep = NetworkEndpoint.Parse(ip, port);
        {
            using var drvQuery = client.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            drvQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Connect(client.EntityManager, ep);
        }
    }
}
