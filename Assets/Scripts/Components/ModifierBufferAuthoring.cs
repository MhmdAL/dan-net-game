using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public struct ModifierBufferComponent : IBufferElementData
{
    public float Duration;
    public float Value;
    public ModifierType Type;
    
    public float ElapsedDuration;

    public long SourceId;
}

public class ModifierBufferAuthoring : MonoBehaviour
{
    public class ModifierBufferBaker : Baker<ModifierBufferAuthoring>
    {
        public override void Bake(ModifierBufferAuthoring authoring)
        {
            AddBuffer<ModifierBufferComponent>();
        }
    }
}

public enum ModifierType
{
    Slow
}
