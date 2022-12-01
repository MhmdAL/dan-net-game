using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct ApplyFireEffectOnAttack : IComponentData
{
    public float Duration;
    public int MinDamage;
    public int MaxDamage;
    public float Rate;
}

public class ApplyFireEffectOnAttackAuthoring : MonoBehaviour
{
    public class ApplyFireEffectOnAttackBaker : Baker<ApplyFireEffectOnAttackAuthoring>
    {
        public override void Bake(ApplyFireEffectOnAttackAuthoring authoring)
        {
            AddComponent(new ApplyFireEffectOnAttack
            {
                Duration = authoring.Duration,
                MinDamage = authoring.MinDamage,
                MaxDamage = authoring.MaxDamage,
                Rate = authoring.Rate
            });
        }
    }

    public float Duration;
    public int MinDamage;
    public int MaxDamage;
    public float Rate;
}