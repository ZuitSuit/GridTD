using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CellController : MonoBehaviour
{
    //get a bunch of info about the tile here
    bool canBuild = true;
    int gridReference;
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
            Debug.Log("speed mod is fine here: "+speedModifier);
        }
        else
        {
            speedModifier = 1.0f;
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

        Debug.Log("but breaks somewhere here: " + speedModifier);
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
    public bool CheckBuild()
    {
        return canBuild && fighters.Count == 0;
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
    public void ToggleBuild(bool toggle)
    {
        canBuild = toggle;
    }
    public void SetGridReference(int reference)
    {
        gridReference = reference;
    }
}

