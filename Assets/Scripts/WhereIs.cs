using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhereIs : MonoBehaviour
{
    public Transform prefabParent;
    public Fighter fighter;
    public Transform cameraMount;

    private void Awake()
    {
        if (prefabParent == null)
        {
            Debug.LogWarning("no parent Transform linked for " + gameObject.name + " id: " + gameObject.GetInstanceID());
        }
        if (fighter == null)
        {
            Debug.LogWarning("no linked Fighter component found on " + prefabParent.gameObject.name);
        }
    }
    public Transform GetParent()
    {
        return prefabParent;
    }
    public Fighter GetFighter()
    {
        return fighter;
    }
    public Transform GetCameraMount()
    {
        return cameraMount;
    }
}
