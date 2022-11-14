using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct SpeedComponent : IComponentData
{
    public float Value { get; set; }
}

public class SpeedComponentAuthoring : MonoBehaviour
{
    public class SpeedComponentBaker : Baker<SpeedComponentAuthoring>
    {
        public override void Bake(SpeedComponentAuthoring authoring)
        {
            AddComponent(new SpeedComponent() {Value = authoring.Value});
        }
    }

    public float Value;
}