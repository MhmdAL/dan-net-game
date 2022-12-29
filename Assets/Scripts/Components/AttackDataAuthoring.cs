using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct AttackData : IComponentData
{
    public int Damage;
    public float MaxCooldown;
    public float CurrentCooldown;
    public float HitChance;
}

public class AttackDataAuthoring : MonoBehaviour
{
    public class AttackDataBaker : Baker<AttackDataAuthoring>
    {
        public override void Bake(AttackDataAuthoring authoring)
        {
            AddComponent(new AttackData
            {
                MaxCooldown = authoring.AttackCooldown,
                Damage = authoring.AttackDamage,
                HitChance = authoring.HitChance
            });
        }
    }

    public float AttackCooldown;
    public int AttackDamage;
    public float HitChance;
}