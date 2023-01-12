using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Collections;

public struct ModifiersData : IComponentData
{
    public NativeHashMap<int, int> MaxStacksMap;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateAfter(typeof(TimerSystem))]
public partial class ModifierSystem : SystemBase
{
    protected override void OnCreate()
    {
        var maxStacksMap = new NativeHashMap<int, int>(1, Allocator.Persistent);
        maxStacksMap.Add((int)ModifierType.Slow, 3);
        
        EntityManager.AddComponent<ModifiersData>(SystemHandle);
        EntityManager.SetComponentData(SystemHandle,
            new ModifiersData
                { MaxStacksMap = maxStacksMap });

        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        var state = EntityManager.GetComponentData<ModifiersData>(SystemHandle);
        
        var dt = SystemAPI.Time.DeltaTime;
        
        Dependency = new CalculateAndUpdateSpeedJob
        {
            maxSlowStacks = state.MaxStacksMap[(int)ModifierType.Slow]
        }.ScheduleParallel(Dependency);
    }

    public partial struct CalculateAndUpdateSpeedJob : IJobEntity
    {
        public int maxSlowStacks;
        
        private void Execute(ref DynamicBuffer<ModifierBufferComponent> modBuffer, ref SpeedData speed)
        {
            var speedMods = new NativeList<ModifierBufferComponent>(0, Allocator.TempJob);

            for (int i = 0; i < modBuffer.Length; i++)
            {
                var effect = modBuffer[i];

                if (effect.Type == ModifierType.Slow)
                {
                    speedMods.Add(effect);
                }
            }

            speedMods.Sort(new SlowSort());

            var speedMultiplier = 1f;

            for (int i = 0; i < speedMods.Length && i < maxSlowStacks; i++)
            {
                var effect = speedMods[i];

                speedMultiplier *= (1 - effect.Value);
            }

            speed.CurrentValue = speed.OriginalValue * speedMultiplier;

            speedMods.Dispose();
        }
    }
    
    // public partial struct SplitModifiersJob : IJobEntity
    // {
    //     public EntityCommandBuffer ecb;
    //     public BufferLookup<SlowModifierBufferComponent> slowBufferLookup;
    //
    //     private void Execute(Entity ent, ref DynamicBuffer<ModifierBufferComponent> modBuffer)
    //     {
    //         var slowMods = new NativeList<ModifierBufferComponent>(Allocator.TempJob);
    //
    //         for (int i = 0; i < modBuffer.Length; i++)
    //         {
    //             var effect = modBuffer[i];
    //
    //             if (effect.Type == ModifierType.Slow)
    //             {
    //                 slowMods.Add(effect);
    //             }
    //         }
    //
    //         if (slowBufferLookup.TryGetBuffer(ent, out var slowBuffer))
    //         {
    //             slowBuffer.Clear();
    //             foreach (var slow in slowMods)
    //             {
    //                 slowBuffer.Add(new SlowModifierBufferComponent { Value = slow.Value });
    //             }
    //         }
    //         else
    //         {
    //             slowBuffer = ecb.AddBuffer<SlowModifierBufferComponent>(ent);
    //             foreach (var slow in slowMods)
    //             {
    //                 slowBuffer.Add(new SlowModifierBufferComponent { Value = slow.Value });
    //             }
    //         }
    //     }
    // }
    //
    // public partial struct UpdateSpeedJob : IJobEntity
    // {
    //     public int maxSlowStacks;
    //
    //     private void Execute(ref DynamicBuffer<SlowModifierBufferComponent> modBuffer, ref SpeedData speed)
    //     {
    //         var speedMods = modBuffer.ToNativeArray(Allocator.TempJob);
    //         // speedMods.Sort(new SlowSort());
    //
    //         var speedMultiplier = 1f;
    //
    //         for (int i = 0; i < speedMods.Length && i < maxSlowStacks; i++)
    //         {
    //             var effect = speedMods[i];
    //
    //             speedMultiplier *= (1 - effect.Value);
    //         }
    //
    //         speed.CurrentValue = speed.OriginalValue * speedMultiplier;
    //     }
    // }
}

public struct SlowSort : IComparer<ModifierBufferComponent>
{
    public int Compare(ModifierBufferComponent a, ModifierBufferComponent b)
    {
        if (a.Value - b.Value < 0f)
        {
            return 1;
        }

        return -1;
    }
}

public struct SlowModifierBufferComponent : IBufferElementData
{
    public float Value;
}