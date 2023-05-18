using Unity.Burst;
using Unity.Entities;

public partial struct SpawnerSystem : ISystem, ISystemStartStop
{
    BeginSimulationEntityCommandBufferSystem.Singleton bi_ecb;

    public void OnStartRunning(ref SystemState state)
    {
        bi_ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        new SpawnJob
        {
            ecb = bi_ecb.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter(),
            deltaTime = SystemAPI.Time.DeltaTime
        }.ScheduleParallel();
    }
    public void OnStopRunning(ref SystemState state) { }

    [BurstCompile]
    partial struct SpawnJob : IJobEntity
    {
        public EntityCommandBuffer.ParallelWriter ecb;
        public float deltaTime;

        public void Execute([ChunkIndexInQuery] int chunkIndex, SpawnerAspect sa)
        {
            sa.spawn(ecb, chunkIndex, deltaTime);
        }
    }
}
