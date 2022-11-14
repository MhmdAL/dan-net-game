using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneMenu : MonoBehaviour
{
    public void LoadMainMenu()
    {
        var clientServerWorlds = new List<World>();
        foreach (var world in World.All)
        {
            if (world.IsClient() || world.IsServer())
                clientServerWorlds.Add(world);
        }

        foreach (var world in clientServerWorlds)
            world.Dispose();

        SceneManager.LoadScene("Menu");
    }
}