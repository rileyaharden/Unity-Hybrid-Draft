
using Unity.Entities;
using Unity.Mathematics;

// can add this component to add pathfinding params so that our pathfindingdots system can find the entity
public struct PathfindingParams : IComponentData
{

    public int2 startPosition;
    public int2 endPosition;

}
