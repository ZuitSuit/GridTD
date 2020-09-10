using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellManager : MonoBehaviour
{
    public CellController controller;
    private Material cellMaterial;
    private Color defaultColor;
    private Color selectedColor;
    private string materialNameBuffer;

    private void Awake()
    {
        materialNameBuffer = GetComponent<MeshRenderer>().material.name;
        cellMaterial = Instantiate(GetComponent<MeshRenderer>().material);
        cellMaterial.name = materialNameBuffer + gameObject.GetInstanceID(); //unique material for each cell material
        GetComponent<MeshRenderer>().material = cellMaterial;
        defaultColor = cellMaterial.GetColor("_SideColors");
        selectedColor = new Color(0.3f,0.9f,0.3f);
    }

    public void Selected(bool toggle = true)
    {
        cellMaterial.SetColor("_SideColors", toggle ? selectedColor : defaultColor);
    }
}
