using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.NetCode;

public struct ForceSyncPositionComponent : IComponentData, IEnableableComponent
{
    [GhostField(Quantization = 1000)] public float2 Position;
    [GhostField(Smoothing = SmoothingAction.Clamp)] public byte CurrentWaypoint;
}


public class ForceSyncPositionComponentAuthoring : MonoBehaviour
{
    public class ForceSyncPositionComponentBaker : Baker<ForceSyncPositionComponentAuthoring>
    {
        public override void Bake(ForceSyncPositionComponentAuthoring authoring)
        {
            AddComponent(new ForceSyncPositionComponent()
            {
                Position = new float2(-8, 4)
            });
        }
    }
}