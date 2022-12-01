using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[InternalBufferCapacity(16)]
public struct WaypointBufferComponent : IBufferElementData
{
    public float2 Position;
}

public class WaypointBufferAuthoring : MonoBehaviour
{
    public class WaypointBufferBaker : Baker<WaypointBufferAuthoring>
    {
        public override void Bake(WaypointBufferAuthoring authoring)
        {
            var buf = AddBuffer<WaypointBufferComponent>();

            foreach (var wp in authoring.Waypoints)
            {
                buf.Add(new WaypointBufferComponent { Position = wp });
            }
        }
    }

    public List<float2> Waypoints;
}