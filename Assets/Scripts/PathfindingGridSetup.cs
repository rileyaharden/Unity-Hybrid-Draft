using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathfindingGridSetup : MonoBehaviour
{
    
    public static PathfindingGridSetup Instance { private set; get; }

    public GameGrid<GridNode> pathfindingGrid;

    private void Awake()
    {
        Instance = this;
        pathfindingGrid = new GameGrid<GridNode>(100, 100, 1f, Vector3.zero, (GameGrid<GridNode> grid, int x, int y) => new GridNode(grid, x, y));

    }


    private void Start()
    {
        //pathfindingGrid = new GameGrid<GridNode>(100, 100, 1f, Vector3.zero, (GameGrid<GridNode> grid, int x, int y) => new GridNode(grid, x, y));

        
    
    }

    // sets the nodes under the newly placed building as unwalkable
    // !!!! THIS ONLY WORKS WITH A CELL SIZE OF 1. If change cell size, need to update to include cellSize;
    public void UpdateBuildingPlacement(Vector3 originPosition, int cellsWide, int cellsLong)
    {
        // get coordinates of the lower left cell in building
        pathfindingGrid.GetXY(new Vector3(originPosition.x - cellsWide / 2 + 0.1f, originPosition.y - cellsLong / 2 + 0.1f), out int x, out int y);
        for (int i = 0; i < cellsWide; i++)
        {
            for (int j = 0; j < cellsWide; j++)
            {
                pathfindingGrid.GetGridObjectFromCoordinates(x + i, y + j).SetIsWalkable(false);
            }
        }
    
    }

    //private void Update()
    //{
    //    if (Input.GetMouseButtonDown(1))
    //    {

    //        Vector3 mousePosition = RAHUtility.GetMouseWorldPosition(true) + (new Vector3(1f, 1f) * pathfindingGrid.GetCellSize() * 0.5f);
    //        //Vector3 mousePosition = RAHUtility.GetMouseWorldPosition(true);
    //        GridNode gridNode = pathfindingGrid.GetGridObjectFromWorldPosition(mousePosition);
    //        if (gridNode != null)
    //        {
    //            gridNode.SetIsWalkable(!gridNode.IsWalkable());
    //        }
    //        if (gridNode.IsWalkable())
    //        {
    //            Debug.Log("Walkable");
    //        }
    //        else
    //        {
    //            Debug.Log("Unwalkable");
    //        }
        
    //    }

        
    //}


}
