
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class UnityMoveOrderSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var mousePosition = Utils.GetMouseWorldPosition();
            float cellSize = PathFindingSetup.Instance.pathfindingGrid.GetCellSize();

            PathFindingSetup.Instance.pathfindingGrid.GetXY(mousePosition + new Vector3(1, 1) * cellSize * +.5f, out int endX, out int endY);
            
            Entities.ForEach((Entity entity, ref Translation translation, ref PathFollow pathFollow) =>
            {
                PathFindingSetup.Instance.pathfindingGrid.GetXY(translation.Value + new float3(1, 1, 0) * cellSize * +.5f, out int startX, out int startY);
                
                //Add Path Finding Param
                EntityManager.AddComponentData(entity, new PathFindingParam
                {
                    startPos = new int2(startX, startY),
                    endPos = new int2(endX, endY),
                });
                
            });
        }
    }
}
