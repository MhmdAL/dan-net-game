using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct AttackTargetData : IComponentData
{
    public Entity Value;
}

public class AttackTargetDataAuthoring : MonoBehaviour
{
    public class AttackTargetDataBaker : Baker<AttackTargetDataAuthoring>
    {
        public override void Bake(AttackTargetDataAuthoring authoring)
        {
            AddComponent(new AttackTargetData {});
        }
    }
}
