using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.EventSystems;

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class TowerInputSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var isPDown = Input.GetKeyDown(KeyCode.P);
        var isODown = Input.GetKeyDown(KeyCode.O);

        var mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        var cellPos = GridUtils.GetCell(mouseWorldPos);

        if ((isPDown || isODown) && !EventSystem.current.IsPointerOverGameObject())
        {
            var towerType = isPDown ? TowerType.Fire : TowerType.Slow;
            var req = EntityManager.CreateEntity(typeof(SendRpcCommandRequestComponent), typeof(PlaceTowerCommand));
            EntityManager.SetComponentData(req, new PlaceTowerCommand { Cell = cellPos, Type = towerType});
        }

        // foreach (var towerInput in SystemAPI.Query<RefRW<TowerPlacementInput>>())
        // {
        //     towerInput.ValueRW = default;

        //     if (isMouseDown)
        //     {
        //         Debug.Log("Tower input");

        //         towerInput.ValueRW.PlaceTower.Set();
        //         towerInput.ValueRW.Cell = cellPos;
        //     }
        // }
    }
}