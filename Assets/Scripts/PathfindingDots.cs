using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Burst;
using Unity.Entities;

// looks for entities with pathfinding parameters and assigns the findpath job to them.
public class PathfindingDots : ComponentSystem
{
    private const int MOVE_STRAIGHT_COST = 10;
    private const int MOVE_DIAGONAL_COST = 14;

    protected override void OnUpdate()
    {
        int2 gridSize = new int2(PathfindingGridSetup.Instance.pathfindingGrid.GetWidth(), PathfindingGridSetup.Instance.pathfindingGrid.GetHeight());
        // Everytime an entity recieves a pathfinding param, its going to run pathfinding and then
        // remove the pathfinding param
        NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
        
        Entities.ForEach((Entity entity, DynamicBuffer<PathPositionBuffer> pathPositionBuffer, ref PathfindingParams pathfindingParams) =>
        {
            //Debug.Log("Find Path!");
            FindPathJob findPathJob = new FindPathJob
            {
                pathNodeArray = GetPathNodeArray(),
                gridSize = gridSize,
                startPosition = pathfindingParams.startPosition,
                endPosition = pathfindingParams.endPosition,
                pathPositionBufferFromEntity = GetBufferFromEntity<PathPositionBuffer>(),
                pathFollowComponentDataFromEntity = GetComponentDataFromEntity<PathFollowComp>(),
                entity = entity
            };
            jobHandleList.Add(findPathJob.Schedule());
            //findPathJob.Run();

            // remove to make sure pathfinding only runs once per entity
            PostUpdateCommands.RemoveComponent<PathfindingParams>(entity);
        });
        JobHandle.CompleteAll(jobHandleList);
        jobHandleList.Dispose();
        
    }


    private NativeArray<PathNode> GetPathNodeArray()
    {
        GameGrid<GridNode> grid = PathfindingGridSetup.Instance.pathfindingGrid;
        
        int2 gridSize = new int2(grid.GetWidth(), grid.GetHeight());
        NativeArray<PathNode> pathNodeArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.TempJob);

        for (int i = 0; i < gridSize.x; i++)
        {
            for (int j = 0; j < gridSize.y; j++)
            {
                PathNode pathNode = new PathNode();
                pathNode.x = i;
                pathNode.y = j;
                pathNode.index = CalculateIndex(i, j, gridSize.x);

                pathNode.gCost = int.MaxValue;

                pathNode.isWalkable = grid.GetGridObjectFromCoordinates(i, j).IsWalkable();
                pathNode.cameFromNodeIndex = -1;

                pathNodeArray[pathNode.index] = pathNode;
            }
        }
        return pathNodeArray;
    }


    [BurstCompile]
    private struct FindPathJob: IJob
    {
        // starting now
        public int2 gridSize;
        [DeallocateOnJobCompletion]
        public NativeArray<PathNode> pathNodeArray;
        public int2 startPosition;
        public int2 endPosition;

        public Entity entity;
        [NativeDisableContainerSafetyRestriction]
        public ComponentDataFromEntity<PathFollowComp> pathFollowComponentDataFromEntity;

        [NativeDisableContainerSafetyRestriction]
        public BufferFromEntity<PathPositionBuffer> pathPositionBufferFromEntity;

        public void Execute()
        {
            for (int i = 0; i < pathNodeArray.Length; i++)
            {
                PathNode pathNode = pathNodeArray[i];
                pathNode.hCost = CalculateHCost(new int2(pathNode.x, pathNode.y), endPosition);
                pathNode.cameFromNodeIndex = -1;
            }

            NativeArray<int2> neighborOffsetArray = new NativeArray<int2>(8, Allocator.Temp);
            neighborOffsetArray[0] = new int2(-1, 0); // left
            neighborOffsetArray[1] = new int2(1, 0); //right
            neighborOffsetArray[2] = new int2(0, 1); // up
            neighborOffsetArray[3] = new int2(0, -1); //down
            neighborOffsetArray[4] = new int2(-1, 1); // left up
            neighborOffsetArray[5] = new int2(-1, -1); // left down
            neighborOffsetArray[6] = new int2(1, 1); // right up
            neighborOffsetArray[7] = new int2(1, -1); // right down

            NativeArray<bool> areStraightNeighborsWalkableArray = new NativeArray<bool>(4, Allocator.Temp);



            int endNodeIndex = CalculateIndex(endPosition.x, endPosition.y, gridSize.x);

            PathNode startNode = pathNodeArray[CalculateIndex(startPosition.x, startPosition.y, gridSize.x)];
            startNode.gCost = 0;
            startNode.CalculateFCost();
            pathNodeArray[startNode.index] = startNode;


            NativeList<int> openList = new NativeList<int>(Allocator.Temp);
            //NativeList<int> closedList = new NativeList<int>(Allocator.Temp);
            // 4 instances of closed list including this one ^
            NativeArray<bool> checkedArray = new NativeArray<bool>(gridSize.x * gridSize.y, Allocator.Temp);

            openList.Add(startNode.index);

            while (openList.Length > 0)
            {
                int currentNodeIndex = GetLowestFCostNodeIndex(openList, pathNodeArray);
                PathNode currentNode = pathNodeArray[currentNodeIndex];

                if (currentNode.index == endNodeIndex)
                {
                    // YIPEE we reached our destination
                    break;
                }

                // remove current node from open list
                for (int i = 0; i < openList.Length; i++)
                {
                    if (openList[i] == currentNodeIndex)
                    {
                        openList.RemoveAtSwapBack(i);
                        break;
                    }
                }

                //closedList.Add(currentNodeIndex);
                checkedArray[currentNodeIndex] = true;

                for (int i = 0; i < neighborOffsetArray.Length; i++)
                {
                    int2 neighborOffset = neighborOffsetArray[i];
                    int2 neighborPosition = new int2(currentNode.x + neighborOffset.x, currentNode.y + neighborOffset.y);

                    // if neighbor is a diagonal, check to make sure appropriate straight cells are walkable
                    if (i >= 4)
                    {
                        if (neighborOffset.x == -1)
                        {
                            if (areStraightNeighborsWalkableArray[0] == false)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (areStraightNeighborsWalkableArray[1] == false)
                            {
                                continue;
                            }
                        }
                        if (neighborOffset.y == -1)
                        {
                            if (areStraightNeighborsWalkableArray[3] == false)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (areStraightNeighborsWalkableArray[2] == false)
                            {
                                continue;
                            }
                        }
                    }


                    if (IsPositionInsideGrid(neighborPosition, gridSize) == false)
                    {
                        // neighbor not valid position
                        continue;
                    }

                    int neighborNodeIndex = CalculateIndex(neighborPosition.x, neighborPosition.y, gridSize.x);

                    //if (closedList.Contains(neighborNodeIndex))
                    //{
                    //    // already searched this node
                    //    continue;
                    //}

                    if (checkedArray[neighborNodeIndex] == true)
                    {
                        // already searched this node
                        continue;
                    }

                    PathNode neighborNode = pathNodeArray[neighborNodeIndex];
                    if (neighborNode.isWalkable == false)
                    {
                        // not walkable
                        if (i < 4) areStraightNeighborsWalkableArray[i] = false;

                        continue;
                    }
                    else
                    {
                        // cache the fact that this straight neighbor is walkable
                        if (i < 4) areStraightNeighborsWalkableArray[i] = true;
                    }

                    int2 currentNodePosition = new int2(currentNode.x, currentNode.y);

                    // tentativeG is current g plus distance cost to get to neighbor node
                    int tentativeGCost = currentNode.gCost + CalculateHCost(currentNodePosition, neighborPosition);
                    if (tentativeGCost < neighborNode.gCost)
                    {
                        neighborNode.cameFromNodeIndex = currentNodeIndex;
                        neighborNode.gCost = tentativeGCost;
                        neighborNode.CalculateFCost();
                        pathNodeArray[neighborNodeIndex] = neighborNode;

                        if (openList.Contains(neighborNode.index) == false)
                        {
                            openList.Add(neighborNode.index);
                        }
                    }


                }

            }

            pathPositionBufferFromEntity[entity].Clear();

            PathNode endNode = pathNodeArray[endNodeIndex];
            if (endNode.cameFromNodeIndex == -1)
            {
                // Did not find path
                Debug.Log("Failed to find path");
                pathFollowComponentDataFromEntity[entity] = new PathFollowComp { pathIndex = -1 };
            }
            else
            {
                // Found path! Set pathIndex to buffer length - 1 because we read path from end to start
                CalculatePathVoid(pathNodeArray, endNode, pathPositionBufferFromEntity[entity]);
                pathFollowComponentDataFromEntity[entity] = new PathFollowComp { pathIndex = pathPositionBufferFromEntity[entity].Length - 1 };

            }




            areStraightNeighborsWalkableArray.Dispose();
            neighborOffsetArray.Dispose();
            openList.Dispose();
            //closedList.Dispose();
            checkedArray.Dispose();
        }


    }


    private static void CalculatePathVoid(NativeArray<PathNode> pathNodeArray, PathNode endNode, DynamicBuffer<PathPositionBuffer> pathPositionBuffer)
    {
        if (endNode.cameFromNodeIndex == -1)
        {
            // couldnt find path
        }
        else
        {
            // Found a path
            //pathPositionBuffer.Add(new PathPositionBuffer { position = new int2(endNode.x, endNode.y) });

            PathNode currentNode = endNode;
            while (currentNode.cameFromNodeIndex != -1)
            {
                //PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                //pathPositionBuffer.Add(new PathPositionBuffer { position = new int2(cameFromNode.x, cameFromNode.y) });
                //currentNode = cameFromNode;
                pathPositionBuffer.Add(new PathPositionBuffer { position = new int2(currentNode.x, currentNode.y) });
                currentNode = pathNodeArray[currentNode.cameFromNodeIndex];
            }


        }
    }

    private static NativeList<int2> CalculatePath(NativeArray<PathNode> pathNodeArray, PathNode endNode)
    {
        if (endNode.cameFromNodeIndex == -1)
        {
            // couldnt find path
            return new NativeList<int2>(Allocator.Temp);
        }
        else
        {
            // Found a path
            NativeList<int2> retPath = new NativeList<int2>(Allocator.Temp);
            retPath.Add(new int2(endNode.x, endNode.y));

            PathNode currentNode = endNode;
            while (currentNode.cameFromNodeIndex != -1)
            {
                PathNode cameFromNode = pathNodeArray[currentNode.cameFromNodeIndex];
                retPath.Add(new int2(cameFromNode.x, cameFromNode.y));
                currentNode = cameFromNode;
            }

            return retPath;

        }
    }


    private static bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize)
    {
        return
            gridPosition.x >= 0 &&
            gridPosition.y >= 0 &&
            gridPosition.x < gridSize.x &&
            gridPosition.y < gridSize.y;
    }

    private static int CalculateIndex(int x, int y, int gridWidth)
    {
        return x + y * gridWidth;
    }

    private static int CalculateHCost(int2 startPosition, int2 endPosition)
    {
        int xDistance = math.abs(startPosition.x - endPosition.x);
        int yDistance = math.abs(startPosition.y - endPosition.y);
        int remaining = math.abs(xDistance - yDistance);

        return MOVE_DIAGONAL_COST * math.min(xDistance, yDistance) + MOVE_STRAIGHT_COST * remaining;
    }


    private static int GetLowestFCostNodeIndex(NativeList<int> openList, NativeArray<PathNode> pathNodeArray)
    {
        PathNode lowestCostPathNode = pathNodeArray[openList[0]];
        for (int i = 1; i < openList.Length; i++)
        {
            PathNode testerNode = pathNodeArray[openList[i]];
            if (testerNode.fCost < lowestCostPathNode.fCost)
            {
                lowestCostPathNode = testerNode;
            }
        }
        return lowestCostPathNode.index;
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
