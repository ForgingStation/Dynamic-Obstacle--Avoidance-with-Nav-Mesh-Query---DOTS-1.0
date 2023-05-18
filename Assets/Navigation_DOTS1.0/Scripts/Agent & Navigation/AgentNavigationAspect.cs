using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine.Experimental.AI;
using Unity.Collections;

public readonly partial struct AgentNavigationAspect : IAspect
{
    public readonly RefRW<Agent> agent;
    public readonly RefRW<AgentMovement> agentMovement;
    public readonly DynamicBuffer<AgentBuffer> agentBuffer;
    public readonly DynamicBuffer<AgentPathValidityBuffer> agentPathValidityBuffer;
    public readonly RefRW<LocalTransform> trans;

    public void moveAgent(float deltaTime, float minDistanceReached, float agentSpeed, float agentRotationSpeed)
    {
        if (agentBuffer.Length > 0 && agent.ValueRO.pathCalculated && !agentMovement.ValueRO.reached)
        {
            agentMovement.ValueRW.waypointDirection = math.normalize(agentBuffer[agentMovement.ValueRO.currentBufferIndex].wayPoints - trans.ValueRO.Position);
            if (!float.IsNaN(agentMovement.ValueRW.waypointDirection.x))
            {
                trans.ValueRW.Position += agentMovement.ValueRW.waypointDirection * agentSpeed * deltaTime;
                trans.ValueRW.Rotation = math.slerp(
                                        trans.ValueRW.Rotation, 
                                        quaternion.LookRotation(agentMovement.ValueRW.waypointDirection, math.up()), 
                                        deltaTime * agentRotationSpeed);
                if (math.distance(trans.ValueRO.Position, agentBuffer[agentBuffer.Length - 1].wayPoints) <= minDistanceReached)
                {
                    agentMovement.ValueRW.reached = true;
                }
                else if (math.distance(trans.ValueRO.Position, agentBuffer[agentMovement.ValueRO.currentBufferIndex].wayPoints) <= minDistanceReached)
                {
                    agentMovement.ValueRW.currentBufferIndex = agentMovement.ValueRW.currentBufferIndex + 1;
                }
            }
            else if (!agentMovement.ValueRO.reached)
            {
                agentMovement.ValueRW.currentBufferIndex = agentMovement.ValueRW.currentBufferIndex + 1;
            }
        }
    }
}

[BurstCompile]
public struct PathValidityJob : IJob
{
    public NavMeshQuery query;
    public float3 extents;
    public int currentBufferIndex;
    public LocalTransform trans;
    public float unitsInDirection;
    [NativeDisableContainerSafetyRestriction] public DynamicBuffer<AgentBuffer> ab;
    [NativeDisableContainerSafetyRestriction] public DynamicBuffer<AgentPathValidityBuffer> apvb;
    NavMeshLocation startLocation;
    UnityEngine.AI.NavMeshHit navMeshHit;
    PathQueryStatus status;

    public void Execute()
    {
        if (currentBufferIndex < ab.Length)
        {
            if (!query.IsValid(query.MapLocation(ab.ElementAt(currentBufferIndex).wayPoints, extents, 0)))
            {
                apvb.Add(new AgentPathValidityBuffer { isPathInvalid = true });
            }
            else
            {
                startLocation = query.MapLocation(trans.Position + (trans.Forward() * unitsInDirection), extents, 0);
                status = query.Raycast(out navMeshHit, startLocation, ab.ElementAt(currentBufferIndex).wayPoints);
                
                if (status == PathQueryStatus.Success)
                {
                    if ((math.ceil(navMeshHit.position).x != math.ceil(ab.ElementAt(currentBufferIndex).wayPoints.x)) &&
                        (math.ceil(navMeshHit.position).z != math.ceil(ab.ElementAt(currentBufferIndex).wayPoints.z)))
                    {
                        apvb.Add(new AgentPathValidityBuffer { isPathInvalid = true });
                    }
                }
                else
                {
                    apvb.Add(new AgentPathValidityBuffer { isPathInvalid = true });
                }
            }
        }
    }
}

[BurstCompile]
public struct NavigateJob : IJob
{
    public NavMeshQuery query;
    [NativeDisableContainerSafetyRestriction] public DynamicBuffer<AgentBuffer> ab;
    public float3 fromLocation;
    public float3 toLocation;
    public float3 extents;
    public int maxIteration;
    public int maxPathSize;
    NavMeshLocation nml_FromLocation;
    NavMeshLocation nml_ToLocation;
    PathQueryStatus status;
    PathQueryStatus returningStatus;

    public void Execute()
    {
        nml_FromLocation = query.MapLocation(fromLocation, extents, 0);
        nml_ToLocation = query.MapLocation(toLocation, extents, 0);
        if (query.IsValid(nml_FromLocation) && query.IsValid(nml_ToLocation))
        {
            status = query.BeginFindPath(nml_FromLocation, nml_ToLocation, -1);
            if (status == PathQueryStatus.InProgress)
            {
                status = query.UpdateFindPath(maxIteration, out int iterationPerformed);
                if (status == PathQueryStatus.Success)
                {
                    status = query.EndFindPath(out int polygonSize);
                    NativeArray<NavMeshLocation> res = new NativeArray<NavMeshLocation>(polygonSize, Allocator.Temp);
                    NativeArray<StraightPathFlags> straightPathFlag = new NativeArray<StraightPathFlags>(maxPathSize, Allocator.Temp);
                    NativeArray<float> vertexSide = new NativeArray<float>(maxPathSize, Allocator.Temp);
                    NativeArray<PolygonId> polys = new NativeArray<PolygonId>(polygonSize, Allocator.Temp);
                    int straightPathCount = 0;
                    int a = query.GetPathResult(polys);
                    returningStatus = PathUtils.FindStraightPath(
                        query,
                        fromLocation,
                        toLocation,
                        polys,
                        polygonSize,
                        ref res,
                        ref straightPathFlag,
                        ref vertexSide,
                        ref straightPathCount,
                        maxPathSize
                    );
                    if (returningStatus == PathQueryStatus.Success)
                    {
                        for (int i = 0; i < straightPathCount; i++)
                        {
                            if (!(math.distance(fromLocation, res[i].position) < 1) && query.IsValid(query.MapLocation(res[i].position, extents, 0)))
                            {
                                ab.Add(new AgentBuffer { wayPoints = new float3(res[i].position.x, fromLocation.y, res[i].position.z) });
                            }
                        }
                    }
                    res.Dispose();
                    straightPathFlag.Dispose();
                    polys.Dispose();
                    vertexSide.Dispose();
                }
            }
        }
    }
}
