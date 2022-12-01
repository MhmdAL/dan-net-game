using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;
using Unity.Collections;
using Unity.Mathematics;

[GhostComponent(PrefabType=GhostPrefabType.All)]
public struct TowerPlacementInput : IInputComponentData
{
    public InputEvent PlaceTower;
    public int2 Cell;
}
[DisallowMultipleComponent]
public class TowerInputAuthoring : MonoBehaviour
{
    class TowerInputBaker : Unity.Entities.Baker<TowerInputAuthoring>
    {
        public override void Bake(TowerInputAuthoring authoring)
        {
            AddComponent<TowerPlacementInput>();
        }
    }
}

public struct PlaceTowerCommand : IRpcCommand
{
    public int2 Cell;
    public TowerType Type;
}

public enum TowerType
{
    Fire,
    Slow
}