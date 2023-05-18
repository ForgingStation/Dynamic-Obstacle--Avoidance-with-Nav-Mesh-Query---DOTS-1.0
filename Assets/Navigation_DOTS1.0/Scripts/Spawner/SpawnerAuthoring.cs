using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public struct Spawner : IComponentData
{
    public Entity prefabEntity;
    public float3 gridSize;
    public float3 gridOffset;
    public float3 padding;
    public bool spawned;
    public int cycleCount;
    public bool relativeToSpawner;
    public int maxCycles;
    public float spawnEvery;
    public float elapsed;
}

public struct Transform : IComponentData
{
    public LocalTransform localTransform;
}

public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject prefab;
    public float3 gridSize;
    public float3 gridOffset;
    public float3 padding;
    public bool relativeToSpawner;
    public int maxCycles;
    public float spawnEvery;
}

public class SpawnerBaker : Baker<SpawnerAuthoring>
{
    public override void Bake(SpawnerAuthoring authoring)
    {
        Entity entity = GetEntity(TransformUsageFlags.None);
        LocalTransform lt = new LocalTransform();
        lt.Position = authoring.transform.position;
        lt.Rotation = authoring.transform.rotation;
        lt.Scale = 1;
        AddComponent(entity, new Spawner
        {
            prefabEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
            gridSize = authoring.gridSize,
            padding = authoring.padding,
            gridOffset = authoring.gridOffset,
            relativeToSpawner = authoring.relativeToSpawner,
            maxCycles= authoring.maxCycles,
            spawnEvery = authoring.spawnEvery,
            cycleCount = 0,
            elapsed = 0
        });

        AddComponent(entity, new Transform
        {
            localTransform = lt
        });
    }
}