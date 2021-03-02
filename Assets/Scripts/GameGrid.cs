using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GameGrid<TGridObject>
{

    private int width;
    private int height;
    private float cellSize;
    private Vector3 gridOrigin;
    private TGridObject[,] gridArray;

    public GameGrid(int width, int height, float cellSize, Vector3 gridOrigin, Func<GameGrid<TGridObject>, int, int, TGridObject> createGridObject)
    {
        this.width = width;
        this.height = height;
        this.cellSize = cellSize;
        this.gridOrigin = gridOrigin;

        gridArray = new TGridObject[width, height];

        // instantiate default object in grid
        for (int i = 0; i < gridArray.GetLength(0); i++)
        {
            for (int j = 0; j < gridArray.GetLength(1); j++)
            {
                gridArray[i, j] = createGridObject(this, i, j);
            }
        }



    }

    // returns world position from grid coordinates
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(x, y) * cellSize + gridOrigin;
    }

    // returns grid coordinates from world position
    // "out" basically allows returning multiple values from a single method
    public void GetXY(Vector3 worldPosition, out int x, out int y)
    {
        x = Mathf.FloorToInt((worldPosition - gridOrigin).x / cellSize);
        y = Mathf.FloorToInt((worldPosition - gridOrigin).y / cellSize);
    }

    public int GetWidth()
    {
        return width;
    }

    public int GetHeight()
    {
        return height;
    }

    //public void SetValueFromGridCoordinates(int x, int y, TGridObject value)
    //{
    //    if (x >= 0 && y >= 0 && x < width && y < height)
    //    {
    //        gridArray[x, y] = value;

    //        // if event has subsrcibers, then call event
    //        if (OnGridValueChanged != null)
    //        {
    //            OnGridValueChanged(this, new OnGridValueChangedEventArgs { a = x, b = y });

    //        }

    //    }
    //}

    //public void SetValueFromWorldPosition(Vector3 worldPosition, TGridObject value)
    //{
    //    // Must always initialize "out" vars
    //    int x, y;
    //    GetXY(worldPosition, out x, out y);

    //    // run SetValue with now modified x and y
    //    SetValueFromGridCoordinates(x, y, value);
    //}

    public TGridObject GetValueFromGridCoordinates(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default;
        }
    }

    public TGridObject GetValueFromWorldPosition(Vector3 worldPosition)
    {
        int x, y;
        GetXY(worldPosition, out x, out y);
        return GetValueFromGridCoordinates(x, y);
    }

    public TGridObject GetGridObjectFromCoordinates(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < width && y < height)
        {
            return gridArray[x, y];
        }
        else
        {
            return default(TGridObject);
        }
    }

    public TGridObject GetGridObjectFromWorldPosition(Vector3 worldPos)
    {
        int x, y;
        GetXY(worldPos, out x, out y);
        return GetGridObjectFromCoordinates(x, y);
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public int GetCellSizex10()
    {
        return Mathf.RoundToInt(cellSize * 10);
    }

}
