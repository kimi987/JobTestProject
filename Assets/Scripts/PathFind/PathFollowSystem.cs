using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;


public class PathFollowSystem : ComponentSystem
{

    private float _moveSpeed = 3f;
    public Grid<GridNode> gird;
    public Random random = new Random(100);
    protected override void OnUpdate()
    {   
        gird = PathFindingSetup.Instance.pathfindingGrid;
        Entities.ForEach((Entity entity, DynamicBuffer<PathPosition> pathPositionBuff, ref Translation translation, ref PathFollow pathFollow) =>
        {
            if (pathFollow.index >= 0)
            {
                var pathPosition = pathPositionBuff[pathFollow.index].Position;
                
                var targetPosition = new float3(pathPosition.x, pathPosition.y, 0);
                
                if (math.distance(targetPosition, translation.Value) < .1f)
                {
                    //next way point
                    pathFollow.index--;
                    return;
                }
                var moveDir = math.normalize(targetPosition - translation.Value);
                
                translation.Value += moveDir * _moveSpeed * Time.DeltaTime;

            } else if (pathFollow.index == -1)
            {
                //just finish last move
                pathFollow.index = -2;
                gird.GetXY(translation.Value, out var startX, out var startY);
                EntityManager.AddComponentData(entity, new PathFindingParam
                {
                    startPos = new int2(startX, startY),
                    endPos = new int2(random.NextInt(0, 30), random.NextInt(0, 15)),
                });
                
            }
        });
    }
}
