using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class PathFindingVisual : MonoBehaviour
{

    private Grid<GridNode> grid;

    private Mesh mesh;

    private bool updateMesh;

    private void Awake()
    {
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
    }

    public void SetGrid(Grid<GridNode> grid)
    {
        this.grid = grid;
        UpdateVisual();

        grid.OnGridObjectChanged += Grid_OnGridValueChanged;
    }


    private void Grid_OnGridValueChanged(object sender, Grid<GridNode>.OnGridObjectChangedEventArgs e)
    {
        updateMesh = true;
    }

    private void LateUpdate()
    {
        if (updateMesh)
        {
            updateMesh = false;
            UpdateVisual();
        }
    }

    private void UpdateVisual()
    {
        MeshUtils.CreateEmptyMeshArrays(grid.GetWidth() * grid.GetHeight(), out Vector3[] vetices, out Vector2[] uv, out int[] triangles);

        for (int i = 0; i < grid.GetWidth(); i++)
        {
            for (int j = 0; j < grid.GetHeight(); j++)
            {
                var index = i * grid.GetHeight() + j;
                var quadSize = new Vector3(1, 1) * grid.GetCellSize();
                var gridNode = grid.GetGridObject(i, j);
                var uv00 = new Vector2(0,0);
                var uv11 = new Vector2(.5f,.5f);
                if (!gridNode.IsWalkable())
                {
                    uv00 = new Vector2(.5f, .5f);
                    uv11 = new Vector2(1f,1f);
                }
                
                MeshUtils.AddToMeshArrays(vetices, uv ,triangles, index, grid.GetWorldPosition(i,j)+ quadSize  * .0f, 0f, quadSize, uv00, uv11);
            }
        }

        mesh.vertices = vetices;
        mesh.uv = uv;
        mesh.triangles = triangles;
    }
}
