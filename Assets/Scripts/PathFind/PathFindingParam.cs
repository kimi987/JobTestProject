
using Unity.Entities;
using Unity.Mathematics;

public class PathFindingParam : IComponentData
{
    public int2 startPos;
    public int2 endPos;
}
