using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacementManager : MonoBehaviour
{

    public bool isBuildingPlaced;
    private PathfindingGridSetup pathfindingGridSetupScript;

    private void Start()
    {
        pathfindingGridSetupScript = GameObject.Find("PathfindingGridSetup").GetComponent<PathfindingGridSetup>();
    }


    // called from individual building scripts update until building is placed
    public void PlaceBuilding(GameObject buildingGO, int widthInCells, int heightInCells)
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(buildingGO);
        }

        if (Input.GetMouseButtonDown(0))
        {
            isBuildingPlaced = true;
            buildingGO.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
            pathfindingGridSetupScript.UpdateBuildingPlacement(buildingGO.transform.position, widthInCells, heightInCells);
        }

        Vector3 mousePosition = RAHUtility.GetMouseWorldPosition(true);
        Vector3 newPosition = new Vector3(Mathf.Round(mousePosition.x), Mathf.Round(mousePosition.y));
        buildingGO.transform.position = newPosition;

        



    }

}
