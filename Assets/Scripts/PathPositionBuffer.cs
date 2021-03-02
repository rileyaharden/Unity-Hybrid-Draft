using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

public struct PathPositionBuffer : IBufferElementData
{
    public int2 position;
}
