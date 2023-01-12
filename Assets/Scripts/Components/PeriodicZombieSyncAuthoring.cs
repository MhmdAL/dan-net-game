using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct PeriodicZombieSync : IComponentData
{
    public float MinInterval;
    public float MaxInterval;
    public float CurrentTimer;
}

public class PeriodicZombieSyncAuthoring : MonoBehaviour
{
    public class PeriodicZombieSyncBaker : Baker<PeriodicZombieSyncAuthoring>
    {
        public override void Bake(PeriodicZombieSyncAuthoring authoring)
        {
            AddComponent(new PeriodicZombieSync()
            {
                MinInterval = authoring.MinInterval,
                MaxInterval = authoring.MaxInterval,
                CurrentTimer = authoring.CurrentTimer
            });
        }
    }
    
    public float MinInterval;
    public float MaxInterval;
    public float CurrentTimer;
}