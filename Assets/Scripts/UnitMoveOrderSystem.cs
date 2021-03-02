using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;

public class UnitMoveOrderSystem : ComponentSystem
{
    private Entity playerEntity;


    protected override void OnStartRunning()
    {
        //playerEntity = GetEntityQuery(typeof(PlayerIDComp)).GetSingletonEntity();

    }
    


    protected override void OnUpdate()
    {
        //Unity.Mathematics.Random mathRandom = new Unity.Mathematics.Random(1429);

        //if (Input.GetMouseButtonDown(2))
        //{
        //    // change this to find nearest building
        //    Vector3 commandCenterPos = GameObject.FindGameObjectWithTag("Building").transform.position;

        //    float cellSize = PathfindingGridSetup.Instance.pathfindingGrid.GetCellSize();

        //    PathfindingGridSetup.Instance.pathfindingGrid.GetXY(commandCenterPos + new Vector3(1, 1) * cellSize * 0.5f, out int endX, out int endY);
        //    ValidateGridPosition(ref endX, ref endY);

        //    Entities.ForEach((Entity entity, DynamicBuffer<PathPositionBuffer> PathPositionBuffer, ref Translation translation) =>
        //    {
        //        PathfindingGridSetup.Instance.pathfindingGrid.GetXY(translation.Value + new float3(1, 1, 0) * cellSize * 0.5f, out int startX, out int startY);
        //        ValidateGridPosition(ref startX, ref startY);

        //        // Add pathfinding params
        //        EntityManager.AddComponentData(entity, new PathfindingParams
        //        {
        //            startPosition = new int2(startX, startY),
        //            endPosition = new int2(endX, endY)
        //            //endPosition = new int2(98, 98)
        //        });
        //    });
        //}

        //if (Input.GetMouseButtonDown(0))
        //{
        //    Vector3 mousePosition = RAHUtility.GetMouseWorldPosition(true);

        //    float cellSize = PathfindingGridSetup.Instance.pathfindingGrid.GetCellSize();

        //    PathfindingGridSetup.Instance.pathfindingGrid.GetXY(mousePosition + new Vector3(1, 1) * cellSize * 0.5f, out int endX, out int endY);
        //    ValidateGridPosition(ref endX, ref endY);


        //    // For all entities with hostile comp tag that have pathposition buffer and translation, add params
        //    Entities.WithAll<HostileCompTag>().ForEach((Entity entity, DynamicBuffer<PathPositionBuffer> PathPositionBuffer, ref Translation translation) =>
        //    {
        //        PathfindingGridSetup.Instance.pathfindingGrid.GetXY(translation.Value + new float3(1, 1, 0) * cellSize * 0.5f, out int startX, out int startY);
        //        ValidateGridPosition(ref startX, ref startY);

        //        // Add pathfinding params
        //        EntityManager.AddComponentData(entity, new PathfindingParams
        //        {
        //            startPosition = new int2(startX, startY),
        //            endPosition = new int2(endX, endY)
        //            //endPosition = new int2(98, 98)
        //        });
        //    });
        //}

        //if (Input.GetMouseButtonDown(1))
        //{
        //    Vector3 mousePosition = RAHUtility.GetMouseWorldPosition(true);

        //    float cellSize = PathfindingGridSetup.Instance.pathfindingGrid.GetCellSize();

        //    PathfindingGridSetup.Instance.pathfindingGrid.GetXY(mousePosition + new Vector3(1, 1) * cellSize * 0.5f, out int endX, out int endY);
        //    ValidateGridPosition(ref endX, ref endY);

        //    Translation playerTranslation = EntityManager.GetComponentData<Translation>(playerEntity);


        //    PathfindingGridSetup.Instance.pathfindingGrid.GetXY(playerTranslation.Value + new float3(1, 1, 0) * cellSize * 0.5f, out int startX, out int startY);
        //    ValidateGridPosition(ref startX, ref startY);

        //    // Add pathfinding params
        //    EntityManager.AddComponentData(playerEntity, new PathfindingParams
        //    {
        //        startPosition = new int2(startX, startY),
        //        endPosition = new int2(endX, endY)
        //    });

            
        //}
    }

    //private void ValidateGridPosition(ref int x, ref int y)
    //{
    //    x = math.clamp(x, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetWidth() - 1);
    //    y = math.clamp(y, 0, PathfindingGridSetup.Instance.pathfindingGrid.GetHeight() - 1);

    //}


}
