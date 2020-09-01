using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Policy;
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
    [Header("Fighter UI")]
    public Transform cameraParent;
    public Camera FPSCamera;
    public Transform surroundParent;
    public Camera surroundCamera;
    bool surroundActive = false;
    public Image HPOutline;
    public Image CDOutline;
    public TextMeshProUGUI fighterName;
    public Image targetingModeIcon;
    //targeting icons
    [Header("Targeting icons")]
    public Sprite closestIcon;
    public Sprite fastestIcon;
    public Sprite mostHPIcon;
    public Sprite leastHPIcon;
    //build UI
    [Header("Build UI")]
    public Transform tileTowerParent;
    public GameObject tileTowerPrefab;
    GameObject tileBuffer;
    TowerButton tileScriptBuffer;
    public TextMeshProUGUI tileName;
    public TextMeshProUGUI speedModifier;

    //gamestate UI (top)
    //coins - core hp
    [Header("Coins - Core HP")]
    public TextMeshProUGUI coinCounterText;
    public TextMeshProUGUI coinPerMinuteText;
    public TextMeshProUGUI coreHealthText;
    public TextMeshProUGUI corePercentageText;
    public GameObject altBackgroundCoins;
    //wave
    [Header("Wave Info")]
    public GameObject wavesCounter;
    public GameObject nextWave;
    public TextMeshProUGUI waveMessage;
    public TextMeshProUGUI waveTimer;
    bool awaitingWave = false;
    int currentWave = 0;
    float timeLeft = 0;

    [Header("Controls")]
    public Image restartButton;
    public Image optionsButton;
    public Image exitButton;
    public GameObject restartText;
    public GameObject optionsText;
    public GameObject exitText;

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

    private void Start()
    {
    }

    private void Update()
    {
        if (surroundActive)
        {
            surroundParent.Rotate(0, Time.deltaTime * 10f, 0, Space.Self);
        }

        if (awaitingWave)
        {
            timeLeft -= Time.deltaTime;
            waveTimer.text = "Wave " + (currentWave+1) + " in " + string.Format("{0:00}:{1:00}", Mathf.FloorToInt(timeLeft / 60), Mathf.FloorToInt(timeLeft % 60));
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

    public void InitializeGameStateUI(int cash, int waves)
    {
        coinCounterText.text = cash.ToString();
    }

    public void ToggleWaveTimer(bool active = true)
    {
        waveMessage.gameObject.SetActive(!active);
        waveTimer.gameObject.SetActive(active);
        awaitingWave = active;
    }
    public void UpdateWaveInfo(int wave, float time = 0)
    {
        currentWave = wave;
        timeLeft = time;
    }
    public void ChangeWaveText(string waveText)
    {
        waveMessage.text = waveText;
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
        activeFighter = fighter;
        SetHP(activeFighter.GetHealth());
        SetCD(activeFighter.GetCooldown());
        SetTargetingMode(activeFighter.GetTargetingMode());
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

    public void ToggleWaveUI(bool toggle)
    {
        wavesCounter.SetActive(toggle);
        nextWave.SetActive(!toggle);
    }

    public void ToggleGameStateUI(bool toggle)
    {
        coinCounterText.gameObject.SetActive(toggle);
        coinPerMinuteText.gameObject.SetActive(!toggle);
        coreHealthText.gameObject.SetActive(toggle);
        corePercentageText.gameObject.SetActive(!toggle);
        altBackgroundCoins.SetActive(!toggle);
    }


    public void HighlightRestart(bool toggle)
    {
        restartButton.color = new Color(restartButton.color.r, restartButton.color.g, restartButton.color.b, toggle ? 1 : 0);
        restartText.SetActive(toggle);
    }
    public void HighlightOptions(bool toggle)
    {
        optionsButton.color = new Color(optionsButton.color.r, optionsButton.color.g, optionsButton.color.b, toggle ? 1 : 0);
        optionsText.SetActive(toggle);
    }
    public void HighlightExit(bool toggle)
    {
        exitButton.color = new Color(optionsButton.color.r, optionsButton.color.g, optionsButton.color.b, toggle ? 1 : 0);
        exitText.SetActive(toggle);
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

    public void SetCoinCounter(int coins)
    {
        coinCounterText.text = coins.ToString();
    }
    public void SetCurrentWave(int wave)
    {
        currentWave = wave;
    }

    public void SwitchTargetingMode()
    {
        activeFighter.NextTargetingMode();
    }
    public void SetTargetingMode(TargetingModes targetingMode)
    {
        switch (targetingMode)
        {
            case TargetingModes.Closest:
                targetingModeIcon.sprite = closestIcon;
                break;
            case TargetingModes.Fastest:
                targetingModeIcon.sprite = fastestIcon;
                break;
            case TargetingModes.LeastHP:
                targetingModeIcon.sprite = leastHPIcon;
                break;
            case TargetingModes.MostHP:
                targetingModeIcon.sprite = mostHPIcon;
                break;
        }
    }

    
    public void Sell()
    {
        activeFighter.Die(true);
    }

    public void Restart()
    {
        GameState.Instance.Restart();
    }
    public void Options()
    {
        //stub method
    }
    public void Exit()
    {
        //GameState.Instance.Exit(); 
    }

}
