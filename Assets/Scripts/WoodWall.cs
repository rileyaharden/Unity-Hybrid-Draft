using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WoodWall : BuildingPlacementManager
{
    [SerializeField] int woodWallWidth;
    [SerializeField] int woodWallHeight;

    private void Update()
    {


        if (isBuildingPlaced == false)
        {
            PlaceBuilding(gameObject, woodWallWidth, woodWallHeight);
        }

    }


}
