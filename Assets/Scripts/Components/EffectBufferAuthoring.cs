using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct EffectBufferComponent : IBufferElementData
{
    public float Duration;
    public float Value;
    public EffectType Type;
    public float Rate;

    public float ElapsedDuration;
    public float ElapsedSinceLastApplication;

    public long SourceId;
}

public enum EffectType
{
    FireDoT
}

public class EffectBufferAuthoring : MonoBehaviour
{
    public class EffectBufferBaker : Baker<EffectBufferAuthoring>
    {
        public override void Bake(EffectBufferAuthoring authoring)
        {
            AddBuffer<EffectBufferComponent>();
        }
    }
}
