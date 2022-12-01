using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct ApplySlowOnAttack : IComponentData
{
    public float Duration;
    public float MinSlow;
    public float MaxSlow;
}

public class ApplySlowOnAttackAuthoring : MonoBehaviour
{
    public class ApplySlowOnAttackBaker : Baker<ApplySlowOnAttackAuthoring>
    {
        public override void Bake(ApplySlowOnAttackAuthoring authoring)
        {
            AddComponent(new ApplySlowOnAttack
            {
                Duration = authoring.Duration,
                MinSlow = authoring.MinSlow,
                MaxSlow = authoring.MaxSlow,
            });
        }
    }

    public float Duration;
    public float MinSlow;
    public float MaxSlow;

}