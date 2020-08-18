using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public Transform bottomPanel;
    public Camera FPSCamera;
    public Camera surroundCamera;

    public Image HPOutline;
    public Image CDOutline;
    public TextMeshProUGUI menuName;


    private Fighter activeFighter;

    private void Awake()
    {
        Instance = this;

    }

    public void UnTrackFighter()
    {
        //stub
    }

    public void TrackFighter(Fighter fighter, WhereIs whereIs)
    {
        menuName.text = fighter.GetName();
        if (whereIs.GetCameraMount() == null)
        {
            //TODO use placeholder image
            return;
        }
        FPSCamera.transform.SetParent(whereIs.GetCameraMount());
        FPSCamera.transform.localPosition = Vector3.zero;
        FPSCamera.transform.localRotation = Quaternion.identity;
        
    }

    public void SetHP(float amount)
    {
        HPOutline.fillAmount = amount;
    }

    public void SetCD(float amount)
    {
        CDOutline.fillAmount = amount;
    }

}
