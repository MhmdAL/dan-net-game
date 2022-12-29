using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.NetCode;
using Unity.VisualScripting;
using UnityEngine;

public struct FollowPathComponent : IComponentData, IEnableableComponent
{
    public byte CurrentWaypoint { get; set; }
}

public class FollowPathAuthoring : MonoBehaviour
{
    public class FollowPathComponentBaker : Baker<FollowPathAuthoring>
    {
        public override void Bake(FollowPathAuthoring authoring)
        {
            AddComponent(new FollowPathComponent());
        }
    }
}