using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public struct AttackData : IComponentData
{
    [GhostField] public int Damage;
    [GhostField] public float MaxCooldown;
    [GhostField] public float CurrentCooldown;
    [GhostField] public float HitChance;
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