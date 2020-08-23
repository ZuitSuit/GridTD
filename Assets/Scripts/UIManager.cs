using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public GameObject fighterUI;
    public GameObject fighterUIInteractive;
    public GameObject fighterUIStatic;
    public GameObject buildUI;

    public Transform cameraParent;
    public Camera FPSCamera;
    public Transform surroundParent;
    public Camera surroundCamera;
    bool surroundActive = false;

    public Image HPOutline;
    public Image CDOutline;
    public TextMeshProUGUI menuName;


    Fighter activeFighter;
    Transform fighterParent;
    WhereIs activeWhereIs;

    private void Awake()
    {
        Instance = this;

        fighterUI.SetActive(false);
        buildUI.SetActive(false);

    }

    private void Update()
    {
        if (surroundActive)
        {
            surroundParent.Rotate(0, Time.deltaTime * 10f, 0, Space.Self);
        }
    }

    public void InitializeBuildUI(List<Tower> towers)
    {
        //get prefab with icon/price
        //get icons from Tower component
        //get name and price
        //add prefab to the list
    }

    public void InitializeWaveUI(List<Enemy> enemies)
    {

    }

    public void UnTrackFighter()
    {
        fighterUI.SetActive(false);
        surroundActive = false;
        //reparent cameras back to safety
        FPSCamera.transform.SetParent(cameraParent);
        surroundParent.SetParent(cameraParent);
    }

    public void TrackFighter(Fighter fighter, WhereIs whereIs, bool interactive = false)
    {
        buildUI.SetActive(false);
        fighterUI.SetActive(true);
        fighterUIInteractive.SetActive(interactive);
        fighterUIStatic.SetActive(!interactive);
        fighterParent = whereIs.GetParent();


        menuName.text = fighter.GetName();
        if (whereIs.GetCameraMount() != null)
        {
            FPSCamera.transform.SetParent(whereIs.GetCameraMount());
            FPSCamera.transform.localPosition = Vector3.zero;
            FPSCamera.transform.localRotation = Quaternion.identity;
        }

        if (fighterParent != null)
        {
            surroundParent.SetParent(fighterParent);
            surroundParent.localPosition = Vector3.zero;
            surroundParent.rotation = Quaternion.identity;
            surroundActive = true;
        }
        
    }

    public void BuildUI(CellController controller)
    {
        buildUI.SetActive(true);
        fighterUI.SetActive(false);
    }

    public void HideUI()
    {
        buildUI.SetActive(false);
        fighterUI.SetActive(false);
    }

    public void SetHP(float amount)
    {
        HPOutline.fillAmount = amount;
    }

    public void SetCD(float amount)
    {
        CDOutline.fillAmount = amount;
    }

    public enum FighterType
    {
        Controlled,
        TargetingInfo,
        None
    }

}
