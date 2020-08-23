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

    //fighter UI
    public Transform cameraParent;
    public Camera FPSCamera;
    public Transform surroundParent;
    public Camera surroundCamera;
    bool surroundActive = false;
    public Image HPOutline;
    public Image CDOutline;
    public TextMeshProUGUI fighterName;

    //build UI
    public Transform tileTowerParent;
    public GameObject tileTowerPrefab;
    GameObject tileBuffer;
    TowerButton tileScriptBuffer;
    public TextMeshProUGUI tileName;
    public TextMeshProUGUI speedModifier;

    Fighter activeFighter;
    Transform fighterParent;
    WhereIs activeWhereIs;
    int activeGridCell = -1;

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

    public void InitializeBuildUI()
    {
        //get prefab for the button

        foreach (Tower tower in GameState.Instance.GetTowerScripts())
        {
            tileBuffer = Instantiate(tileTowerPrefab);
            tileBuffer.transform.SetParent(tileTowerParent);
            tileBuffer.transform.localPosition = Vector3.zero;
            tileScriptBuffer = tileBuffer.GetComponent<TowerButton>();
            tileScriptBuffer.InitializedButton(tower); //feed the button all the necessary data
        }
    }

    public void InitializeWaveUI()
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


        fighterName.text = fighter.GetName();
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
        tileName.text = controller.tileName;
        speedModifier.text = "Speed: " + System.Math.Round(1f / controller.GetSpeedModifier(), 2);
        activeGridCell = controller.GetGridReference();
        buildUI.SetActive(true);
        fighterUI.SetActive(false);
    }

    public void HideUI()
    {
        buildUI.SetActive(false);
        fighterUI.SetActive(false);
    }

    //getters
    public int GetActiveGridCell() { return activeGridCell; }

    //setters
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
