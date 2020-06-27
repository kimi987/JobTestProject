using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Diagnostics;

public class PathFindingSetup : MonoBehaviour
{
    public static PathFindingSetup Instance { get; private set; }
    
    [SerializeField] private PathFindingVisual pathfindingVisual;

    public Grid<GridNode> pathfindingGrid;
    
    public Dictionary<int, PathPositions> PosDic = new Dictionary<int, PathPositions>();
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        pathfindingGrid = new Grid<GridNode>(30 ,15 ,1f, Vector3.zero, (Grid<GridNode> grid, int x, int y) => new GridNode(grid, x, y));
        
        pathfindingGrid.GetGridObject(2,0).SetIsWalkable(false);
        
        pathfindingVisual.SetGrid(pathfindingGrid);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            var mousePosition = Utils.GetMouseWorldPosition() + new Vector3(1,1) * pathfindingGrid.GetCellSize() * .5f;

            var gridNode = pathfindingGrid.GetGridObject(mousePosition);

            gridNode?.SetIsWalkable(!gridNode.IsWalkable());
        }
    }
    
    public struct PathPositions
    {
        public NativeArray<PathPosition> pathPoses;
    }
}
