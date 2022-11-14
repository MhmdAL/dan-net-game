using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct HealthComponent : IComponentData
{
    public int MaxHealth;
    public int CurrentHealth;
}

public class HealthComponentAuthoring : MonoBehaviour
{
    public class HealthComponentBaker : Baker<HealthComponentAuthoring>
    {
        public override void Bake(HealthComponentAuthoring authoring)
        {
            var health = default(HealthComponent);

            health.MaxHealth = authoring.MaxHealth;
            health.CurrentHealth = health.MaxHealth;
            
            AddComponent(health);
        }
    }

    public int MaxHealth;
}
