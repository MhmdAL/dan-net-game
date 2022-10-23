using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class MovableCubeAuthoring : MonoBehaviour
{
    public int Value;
}

public struct MovableCubeComponent : IComponentData
{
    [GhostField]
    public int Value;
}

public class MovableCubeBaker : Baker<MovableCubeAuthoring>
{
    public override void Bake(MovableCubeAuthoring authoring)
    {
        AddComponent(new MovableCubeComponent { Value = authoring.Value });
    }
}