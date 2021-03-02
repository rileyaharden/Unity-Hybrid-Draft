using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildingPlacement : MonoBehaviour
{
    [SerializeField] private GameObject chosenBuildingGO;

    public void CreateBuilding()
    {
        Instantiate(chosenBuildingGO, RAHUtility.GetMouseWorldPosition(true), transform.rotation);
    }


}
