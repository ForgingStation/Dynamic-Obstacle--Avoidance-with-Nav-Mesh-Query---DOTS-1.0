using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct NavigationGlobalProperties : IComponentData
{
    public int maxIteration;
    public int maxPathSize;
    public int maxPathNodePoolSize;
    public float3 extents;
    public bool dynamicPathFinding;
    public float minimumDistanceToWaypoint;
    public bool agentMovementEnabled;
    public float3 units;
    public bool setGlobalRelativeLocation;
    public float dynamicPathRecalculatingFrequency;
    public float unitsInForwardDirection;
    public bool retracePath;
    public float agentSpeed;
    public float rotationSpeed;
}

public class NavigationGlobalProperties_Authoring : MonoBehaviour
{
    [Header("Navigation Global Properties")]
    public int maxIteration;
    public int maxPathSize;
    public int maxPathNodePoolSize;
    public float3 extents;
    public bool dynamicPathFinding;
    public float dynamicPathRecalculatingFrequency;
    public float unitsInForwardDirection;

    [Header("Agent")]
    public bool setGlobalRelativeLocation;
    public float3 units;

    [Header("Agent Movement")]
    public bool agentMovementEnabled;
    public float minimumDistanceToWaypoint;
    public float agentSpeed;
    public float rotationSpeed;
    public bool retracePath;
}

public class NavigationGlobalProperties_Baker : Baker<NavigationGlobalProperties_Authoring>
{
    public override void Bake(NavigationGlobalProperties_Authoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);
        AddComponent(entity, new NavigationGlobalProperties
        {
            maxIteration = authoring.maxIteration,
            maxPathSize = authoring.maxPathSize,
            maxPathNodePoolSize = authoring.maxPathNodePoolSize,
            extents = authoring.extents,
            dynamicPathFinding = authoring.dynamicPathFinding,
            minimumDistanceToWaypoint = authoring.minimumDistanceToWaypoint,
            agentMovementEnabled = authoring.agentMovementEnabled,
            units = authoring.units,
            setGlobalRelativeLocation= authoring.setGlobalRelativeLocation,
            unitsInForwardDirection= authoring.unitsInForwardDirection,
            dynamicPathRecalculatingFrequency = authoring.dynamicPathRecalculatingFrequency,
            retracePath= authoring.retracePath,
            agentSpeed = authoring.agentSpeed,
            rotationSpeed = authoring.rotationSpeed
        });
    }
}
