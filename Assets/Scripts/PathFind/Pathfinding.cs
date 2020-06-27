using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEditor.XR;
using UnityEngine;

[UpdateAfter(typeof(SpawnMonkeySystem))]
public class Pathfinding : ComponentSystem
{

    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    
    protected override void OnUpdate()
    {
        var gridSize = new int2(PathFindingSetup.Instance.pathfindingGrid.GetWidth(),
            PathFindingSetup.Instance.pathfindingGrid.GetHeight());
        
        
        var pathFindingJobs = new List<FindPathJob>();
        var jobHandlerArray = new NativeList<JobHandle>(Allocator.Temp);
        // var pathNodeAryList = new NativeList<NativeArray<PathNode>>(Allocator.Temp);
        // var pathNodeAry = GetPathNodeArray();
        Entities.ForEach((Entity entity, PathFindingParam pathFindingParam) =>
        {
            // var buff = EntityManager.GetBuffer<PathPosition>(entity);
            var pathNodeAry = GetPathNodeArray();

            var pathFindingJob = new FindPathJob
            {   
                gridSize = gridSize,
                pathNodeArray = pathNodeAry,
                startPos = pathFindingParam.startPos,
                endPos = pathFindingParam.endPos,
                // pathPositionBuff = buff,
                // bufferFromEntity = GetBufferFromEntity<PathPosition>(),
                entity = entity,
                // pathFollowDataFromEntity = GetComponentDataFromEntity<PathFollow>(),
            };
            pathFindingJobs.Add(pathFindingJob);
            
        
            // EntityManager.AddComponentData(entity, new PathComponent());
            // pathFindingJob.Run();
            jobHandlerArray.Add(pathFindingJob.Schedule());

            PostUpdateCommands.RemoveComponent<PathFindingParam>(entity);
        });
        
        JobHandle.CompleteAll(jobHandlerArray);

        for (var i = 0; i < pathFindingJobs.Count; i++)
        {
            var findJob = pathFindingJobs[i];
            var entity = findJob.entity;
            var pathFollowComponentDataFromEntity = GetComponentDataFromEntity<PathFollow>();
            var endNodeIndex = CalculateIndex(findJob.endPos.x, findJob.endPos.y, gridSize.x);
            var pathPositionBuffer = GetBufferFromEntity<PathPosition>()[entity];
            pathPositionBuffer.Clear();
            PathNode endNode = findJob.pathNodeArray[endNodeIndex];
            if (endNode.cameFromNodeIndex == -1) {
                // Didn't find a path!
                //Debug.Log("Didn't find a path!");
                pathFollowComponentDataFromEntity[entity] = new PathFollow { index = -2 };
            } else {
                // Found a path
                CalculatePath(findJob.pathNodeArray, endNode, pathPositionBuffer);
                
                pathFollowComponentDataFromEntity[entity] = new PathFollow { index = pathPositionBuffer.Length - 1 };
            }

            findJob.pathNodeArray.Dispose();
        }
        jobHandlerArray.Dispose();
    }

    private NativeArray<PathNode> GetPathNodeArray()
    {
        var grid = PathFindingSetup.Instance.pathfindingGrid;
        
        var gridSize = new int2(grid.GetWidth(),grid.GetHeight());
        
        var pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.TempJob);

        for (int i = 0; i < grid.GetWidth(); i++)
        {
            for (int j = 0; j < grid.GetHeight(); j++)
            {
                var pathNode = new PathNode
                {
                    x = i,
                    y = j,
                    index = CalculateIndex(i, j, grid.GetWidth()),
                    gCost = int.MaxValue,
                    isWalkable = grid.GetGridObject(i,j).IsWalkable(),
                    cameFromNodeIndex = -1,
                };

                pathNodeArray[pathNode.index] = pathNode;
            }
        }

        return pathNodeArray;
    }
    // private void Start()
    // {
    //     var startTime = Time.realtimeSinceStartup;
    //     // for (int i = 0; i < 5; i++)
    //     // {
    //     //     FindPath(new int2(0, 0), new int2(19, 19));    
    //     // }
    //     //
    //     var jobHanderArray = new NativeArray<JobHandle>(10, Allocator.TempJob);
    //     for (int i = 0; i < 10; i++)
    //     {
    //         var findPathJob = new FindPathJob
    //         {
    //             startPos = new int2(0,0),
    //             endPos = new int2(19, 19),
    //         };
    //         jobHanderArray[i] = findPathJob.Schedule();
    //     }
    //
    //     JobHandle.CompleteAll(jobHanderArray);
    //     jobHanderArray.Dispose();
    //     
    //     Debug.Log("Time: " + (Time.realtimeSinceStartup - startTime)* 1000f);
    // }
    //
    [BurstCompile]
    private struct FindPathJob : IJob
    {
        public int2 startPos;
        public int2 endPos;
        public int2 gridSize;
        public NativeArray<PathNode> pathNodeArray;
        public Entity entity;
        // [NativeDisableContainerSafetyRestriction]
        // public ComponentDataFromEntity<PathFollow> pathFollowDataFromEntity;
        // public DynamicBuffer<PathPosition> pathPositionBuff;
        // public BufferFromEntity<PathPosition> bufferFromEntity;
        
        public void Execute()
        {
            
            // var gridSize = new int2(30, 15);

        // var pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

        // for (int i = 0; i < gridSize.x; i++)
        // {
        //     for (int j = 0; j < gridSize.y; j++)
        //     {
        //         var pathNode = new PathNode
        //         {
        //             x = i,
        //             y = j,
        //             index = CalculateIndex(i, j, gridSize.x),
        //             gCost = int.MaxValue,
        //             
        //             hCost = CalculateDistanceCost(new int2(i,j), endPos),
        //             
        //             isWalkable = true,
        //             
        //             cameFromNodeIndex = -1,
        //         };
        //
        //         pathNode.CalculateFCost();
        //         pathNodeArray[pathNode.index] = pathNode;
        //     }
        // }

        var neighbourOffsetArray = new NativeArray<int2>(8, Allocator.Temp);


        neighbourOffsetArray[0] = new int2(-1, 0);
        neighbourOffsetArray[1] = new int2(1, 0);
        neighbourOffsetArray[2] = new int2(0, 1);
        neighbourOffsetArray[3] = new int2(0, -1);
        neighbourOffsetArray[4] = new int2(-1, -1);
        neighbourOffsetArray[5] = new int2(-1, 1);
        neighbourOffsetArray[6] = new int2(1, -1);
        neighbourOffsetArray[7] = new int2(1, 1); 


        var endPosIndex = CalculateIndex(endPos.x, endPos.y, gridSize.x);
        var startNode = pathNodeArray[CalculateIndex(startPos.x, startPos.y, gridSize.x)];
        startNode.gCost = 0;
        startNode.CalculateFCost();

        pathNodeArray[startNode.index] = startNode;
        
        var openList = new NativeList<int>(Allocator.Temp);
        var closeList = new NativeList<int>(Allocator.Temp);
        
        openList.Add(startNode.index);

        while (openList.Length > 0)
        {
            var currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
            var currentNode = pathNodeArray[currentNodeIndex];

            if (currentNodeIndex == endPosIndex)
            {
                // reach end point
                
                break;
            }

            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentNodeIndex)
                {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }
            closeList.Add(currentNodeIndex);

            for (int i = 0; i < neighbourOffsetArray.Length; i++)
            {
                var neighbourOffset = neighbourOffsetArray[i];
                var neighbourNodeIndex = CalculateIndex(currentNode.x + neighbourOffset.x,
                    currentNode.y + neighbourOffset.y,
                    gridSize.x);
                if (neighbourNodeIndex >= pathNodeArray.Length || neighbourNodeIndex < 0)
                {
                    continue;
                }

                if (closeList.Contains(neighbourNodeIndex))
                {
                    continue;
                }
                
                var neighbourNode = pathNodeArray[neighbourNodeIndex];

                if (!neighbourNode.isWalkable)
                {
                    closeList.Add(neighbourNodeIndex);
                    continue;
                }

                var currentNodePos = new int2(currentNode.x, currentNode.y);
                var neighbourNodePos = new int2(neighbourNode.x, neighbourNode.y);
                var tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePos, neighbourNodePos);

                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNodeIndex = currentNodeIndex;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.CalculateFCost();
                    pathNodeArray[neighbourNodeIndex] = neighbourNode;

                    if (!openList.Contains(neighbourNode.index)) 
                    {
                        openList.Add(neighbourNode.index);
                    }
                }


            }
        }
        
        // pathPositionBuff.Clear();
        var endNode = pathNodeArray[endPosIndex];
        if (endNode.cameFromNodeIndex == -1)
        {
            //didn't find the path
            Debug.Log("Did't find a path");
            // pathFollowDataFromEntity[entity] = new PathFollow{index = -1};
        }
        else
        {
            // Debug.Log("fin a path");
            //find a path
            // var path = CalculatePath(pathNodeArray, endNode);
            // foreach (var p in path)
            // {
            //     Debug.Log(p);
            // }
            // path.Dispose();
            //
            // CalculatePath(pathNodeArray, endNode, pathPositionBuff);
            // pathFollowDataFromEntity[entity] = new PathFollow{index = pathPositionBuff.Length - 1};
        }
        
        openList.Dispose();
        closeList.Dispose();
        
        neighbourOffsetArray.Dispose();
        // pathNodeArray.Dispose();
        }

      
    }
    
    private static void CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode, DynamicBuffer<PathPosition> positionBuff)
    {
        if (endNode.cameFromNodeIndex == -1)
        {
            //can't find the path
        }
        else
        {
            //found a path
            // var path =new NativeList<int2>(Allocator.Temp);
            // path.Add(new int2(endNode.x, endNode.y));
            positionBuff.Add(new PathPosition {Position = new int2(endNode.x, endNode.y)});
            var currentNode = endNode;

            while (currentNode.cameFromNodeIndex != -1)
            {
                var cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                positionBuff.Add(new PathPosition {Position = new int2(cameFromNode.x, cameFromNode.y)});
                currentNode = cameFromNode;
            }
        }
    }
    
    
    private static NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
    {
        if (endNode.cameFromNodeIndex == -1)
        {
            return new NativeList<int2>(Allocator.Temp);
        }
        else
        {
            var path =new NativeList<int2>(Allocator.Temp);
            path.Add(new int2(endNode.x, endNode.y));

            var currentNode = endNode;

            while (currentNode.cameFromNodeIndex != -1)
            {
                var cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                path.Add(new int2(cameFromNode.x, cameFromNode.y));
                currentNode = cameFromNode;
            }

            return path;
        }
    }

    private static int CalculateDistanceCost(int2 aPos, int2 bPos)
    {
        var xDis = math.abs(aPos.x - bPos.x);
        var yDis = math.abs(aPos.y - bPos.y);
        var remaining = math.abs(xDis - yDis);

        return MOVE_DIAGONAL_COST * math.min(xDis, yDis) + remaining * MOVE_STRAIGHT_COST;
    }
    
    private static int CalculateIndex(int x, int y, int gridSize)
    {
        return x + y * gridSize;
    }

    private static int GetLowestCostFNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
    {
        var lowestCostPathNode = pathNodeArray[openList[0]];

        for (var i = 1; i < openList.Length; i++)
        {
            var testPathNode = pathNodeArray[openList[i]];
            if (lowestCostPathNode.fCost > testPathNode.fCost)
            {
                lowestCostPathNode = testPathNode;
            }
        }

        return lowestCostPathNode.index;
    }
    
    private void FindPath(int2 startPos, int2 endPos)
    {
        var gridSize = new int2(20, 20);

        var pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                var pathNode = new PathNode
                {
                    x = i,
                    y = j,
                    index = CalculateIndex(i, j, gridSize.x),
                    gCost = int.MaxValue,
                    
                    hCost = CalculateDistanceCost(new int2(i,j), endPos),
                    
                    isWalkable = true,
                    
                    cameFromNodeIndex = -1,
                };

                pathNode.CalculateFCost();
                pathNodeArray[pathNode.index] = pathNode;
            }
        }
        
        var neighbourOffsetArray = new NativeArray<int2>(new int2[]
        {
            new int2(-1, 0 ),    
            new int2(1, 0 ),   
            new int2(0, 1 ),    
            new int2(0, -1),   
            new int2(-1, -1 ),    
            new int2(-1, 1 ),    
            new int2(1, -1 ),    
            new int2(1, 1 ), 
        }, Allocator.Temp);

        var endPosIndex = CalculateIndex(endPos.x, endPos.y, gridSize.x);
        var startNode = pathNodeArray[CalculateIndex(startPos.x, startPos.y, gridSize.x)];
        startNode.gCost = 0;
        startNode.CalculateFCost();

        pathNodeArray[startNode.index] = startNode;
        
        var openList = new NativeList<int>(Allocator.Temp);
        var closeList = new NativeList<int>(Allocator.Temp);
        
        openList.Add(startNode.index);

        while (openList.Length > 0)
        {
            var currentNodeIndex = GetLowestCostFNodeIndex(openList, pathNodeArray);
            var currentNode = pathNodeArray[currentNodeIndex];

            if (currentNodeIndex == endPosIndex)
            {
                // reach end point
                
                break;
            }

            for (int i = 0; i < openList.Length; i++)
            {
                if (openList[i] == currentNodeIndex)
                {
                    openList.RemoveAtSwapBack(i);
                    break;
                }
            }
            closeList.Add(currentNodeIndex);

            for (int i = 0; i < neighbourOffsetArray.Length; i++)
            {
                var neighbourOffset = neighbourOffsetArray[i];
                var neighbourNodeIndex = CalculateIndex(currentNode.x + neighbourOffset.x,
                    currentNode.y + neighbourOffset.y,
                    gridSize.x);

                if (neighbourNodeIndex >= pathNodeArray.Length || neighbourNodeIndex < 0)
                {
                    continue;
                }

                if (closeList.Contains(neighbourNodeIndex))
                {
                    continue;
                }
                
                var neighbourNode =
                    pathNodeArray[
                        CalculateIndex(currentNode.x + neighbourOffset.x, currentNode.y + neighbourOffset.y,
                            gridSize.x)];

                if (!neighbourNode.isWalkable)
                {
                    closeList.Add(neighbourNodeIndex);
                    continue;
                }

                var currentNodePos = new int2(currentNode.x, currentNode.y);
                var neighbourNodePos = new int2(neighbourNode.x, neighbourNode.y);
                var tentativeGCost = currentNode.gCost + CalculateDistanceCost(currentNodePos, neighbourNodePos);

                if (tentativeGCost < neighbourNode.gCost)
                {
                    neighbourNode.cameFromNodeIndex = currentNodeIndex;
                    neighbourNode.gCost = tentativeGCost;
                    neighbourNode.CalculateFCost();
                    pathNodeArray[neighbourNodeIndex] = neighbourNode;

                    if (!openList.Contains(neighbourNode.index)) 
                    {
                        openList.Add(neighbourNode.index);
                    }
                }


            }
        }

        var endNode = pathNodeArray[endPosIndex];
        if (endNode.cameFromNodeIndex == -1)
        {
            //didn't find the path
            Debug.Log("Did't find a path");
        }
        else
        {
            //find a path
            var path = CalculatePath(pathNodeArray, endNode);
            foreach (var p in path)
            {
                Debug.Log(p);
            }
            path.Dispose();
        }
        
        pathNodeArray.Dispose();
        openList.Dispose();
        closeList.Dispose();
        neighbourOffsetArray.Dispose();
    }

  
    
    private struct PathNode
    {
        public int x;
        public int y;
        public int index;
        public int gCost;
        public int hCost;
        public int fCost;

        public bool isWalkable;

        public int cameFromNodeIndex;

        public void CalculateFCost()
        {
            fCost = gCost + hCost;
        }
    }
    
}
