using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.NetCode;

public struct AttackRangeData : IComponentData
{
    [GhostField] public float Value { get; set; }
}


public class AttackRangeDataAuthoring : MonoBehaviour
{
    public class AttackRangeDataBaker : Baker<AttackRangeDataAuthoring>
    {
        public override void Bake(AttackRangeDataAuthoring authoring)
        {
            AddComponent(new AttackRangeData() { Value = authoring.Value });
        }
    }

    public float Value;
}