using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct SpawnerAspect : IAspect
{
    private readonly RefRW<Spawner> spawner;
    private readonly RefRO<Transform> trans;

    public void spawn(EntityCommandBuffer.ParallelWriter ecb, int chunkIndex, float deltaTime)
    {
        if (spawner.ValueRO.cycleCount >= spawner.ValueRO.maxCycles)
        {
            spawner.ValueRW.spawned = true;
        }
        else if (!spawner.ValueRO.spawned)
        {
            spawner.ValueRW.elapsed += deltaTime;
            if (spawner.ValueRO.elapsed > spawner.ValueRO.spawnEvery)
            {
                spawner.ValueRW.elapsed = 0;
                for (int i = 0; i < spawner.ValueRO.gridSize.x; i++)
                {
                    for (int j = 0; j < spawner.ValueRO.gridSize.y; j++)
                    {
                        for (int k = 0; k < spawner.ValueRO.gridSize.z; k++)
                        {
                            Entity e = ecb.Instantiate(chunkIndex, spawner.ValueRO.prefabEntity);
                            LocalTransform lt = new LocalTransform();
                            lt = trans.ValueRO.localTransform;
                            lt.Position = new float3(
                                (i + spawner.ValueRO.gridOffset.x) * spawner.ValueRO.padding.x,
                                (j + spawner.ValueRO.gridOffset.y) * spawner.ValueRO.padding.y,
                                (k + spawner.ValueRO.gridOffset.z) * spawner.ValueRO.padding.z);
                            lt.Rotation = quaternion.identity;
                            if (spawner.ValueRO.relativeToSpawner)
                            {
                                lt.Rotation = trans.ValueRO.localTransform.Rotation;
                                lt.Position += trans.ValueRO.localTransform.Position;
                            }
                            ecb.SetComponent(chunkIndex, e, lt);
                        }
                    }
                }
                spawner.ValueRW.cycleCount = spawner.ValueRO.cycleCount + 1;
            }
        }
    }
}
