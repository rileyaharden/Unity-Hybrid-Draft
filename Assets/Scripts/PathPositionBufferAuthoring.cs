using UnityEngine;
using Unity.Entities;

public class PathPositionBufferAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<PathPositionBuffer>(entity);
    }
}
