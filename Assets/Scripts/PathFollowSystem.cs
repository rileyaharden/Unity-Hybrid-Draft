using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

// looks for entities with a valid path and makes them follow it

// now that I'm using game objects should be able to remove this script except it makes it so 
//      the start position in UnitMoveOrderSystem gets translation.Value as 0,0,0. The fix is to just add
//       the contents of the UnitMoveOrderSystem script to the GO script but im too lazy; 
public class PathFollowSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        //Entities.ForEach((DynamicBuffer<PathPositionBuffer> pathPositionBuffer, ref Translation translation, ref PathFollowComp pathFollowComp) =>
        //{
        //    if (pathFollowComp.pathIndex >= 0)
        //    {
        //        int2 pathPosition = pathPositionBuffer[pathFollowComp.pathIndex].position;

        //        float3 targetPosition = new float3(pathPosition.x, pathPosition.y, 0);
        //        float3 moveDir = math.normalizesafe(targetPosition - translation.Value);
        //        float moveSpeed = 3f;

        //        translation.Value += moveDir * moveSpeed * Time.DeltaTime;

        //        if (math.distance(translation.Value, targetPosition) < 0.1f)
        //        {
        //            // next waypoint
        //            pathFollowComp.pathIndex--;
        //        }
        //    }
        //});
    }
}
