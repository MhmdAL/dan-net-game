using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct DamageBuffer : IBufferElementData
{
    public int Value;
}

public class DamageBufferAuthoring : MonoBehaviour
{
    public class DamageBufferBaker : Baker<DamageBufferAuthoring>
    {
        public override void Bake(DamageBufferAuthoring authoring)
        {
            AddBuffer<DamageBuffer>();
        }
    }
}
