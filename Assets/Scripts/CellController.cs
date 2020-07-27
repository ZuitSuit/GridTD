using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CellController : MonoBehaviour
{
    //get a bunch of info about the tile here
    bool canBuild = true;
    int gridReference;
    float speedModifier;

    List<GameObject> enemies = new List<GameObject>();

    private void Start()
    {
        if(gameObject.GetComponentInParent<NavMeshModifier>() != null && gameObject.GetComponentInParent<NavMeshModifier>().overrideArea)
        {
            speedModifier = NavMesh.GetAreaCost(gameObject.GetComponentInParent<NavMeshModifier>().area);
        }
        else
        {
            speedModifier = 1.0f;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        if (other.GetComponentInChildren<Enemy>() != null)
        {
            Enemy enemy = other.GetComponentInChildren<Enemy>();
            enemy.SetSpeed(speedModifier);
            enemies.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.isTrigger) return;
        if (other.GetComponentInChildren<Enemy>() != null)
        {
            enemies.Remove(other.gameObject);
        }
    }

    //getters
    public bool CheckBuild()
    {
        return canBuild && enemies.Count == 0;
    }
    public int GetGridReference()
    {
        return gridReference;
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

