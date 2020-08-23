using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CellController : MonoBehaviour
{
    //get a bunch of info about the tile here
    int towerID = -1;
    int gridReference;
    public string tileName;

    float speedModifier = 1.0f;
    Fighter fighterBuffer;
    WhereIs whereIsBuffer;

    public NavMeshModifier navMeshModifier;

    List<GameObject> fighters = new List<GameObject>();

    private void Start()
    {
        if(navMeshModifier != null && navMeshModifier.overrideArea)
        {
            
            speedModifier = NavMesh.GetAreaCost(navMeshModifier.area);
        }
        else
        {
            speedModifier = 1.0f;
        }

        if(tileName == null || tileName == "")
        {
            tileName = "Tile";
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (other.GetComponent<WhereIs>() != null)
        {
            whereIsBuffer = other.GetComponent<WhereIs>();
            fighterBuffer = whereIsBuffer.GetFighter();
            fighterBuffer.SetTerrainSpeedModifier(speedModifier);
            fighterBuffer.RecalculateSpeed();
            fighters.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;
        if (other.GetComponent<WhereIs>() != null)
        {
            fighters.Remove(other.gameObject);
        }
    }

    //getters
    public bool CanBuild()
    {
        //can only build if there is no tower on top of this grid cell
        return towerID == -1;
    }
    public int GetGridReference()
    {
        return gridReference;
    }

    public float GetSpeedModifier()
    {
        return speedModifier;
    }

    //setters

    public void SetTower(int reference)
    {
        towerID = reference;
    }
    public void RemoveTower()
    {
        towerID = -1;
    }
    public void SetGridReference(int reference)
    {
        gridReference = reference;
    }
}

