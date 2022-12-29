using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.NetCode;

public struct SpeedData : IComponentData
{
    public float OriginalValue { get; set; }
    public float CurrentValue { get; set; }
}


public class SpeedComponentAuthoring : MonoBehaviour
{
    public class SpeedDataBaker : Baker<SpeedComponentAuthoring>
    {
        public override void Bake(SpeedComponentAuthoring authoring)
        {
            AddComponent(new SpeedData() { CurrentValue = authoring.Value, OriginalValue = authoring.Value });
        }
    }

    public float Value;
}