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