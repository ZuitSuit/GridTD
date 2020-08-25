using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TowerButton : MonoBehaviour
{
    int towerID;
    public TextMeshProUGUI towerName;
    public TextMeshProUGUI description;
    public TextMeshProUGUI price;

    public GameObject informationPopup;

    public Image icon;

    public void InitializedButton(Tower tower)
    {
        towerID = tower.GetGameStateID();
        towerName.text = tower.fighterName == null ? "Tower" : tower.fighterName;
        //get all other data from the tower
        //set the icon from the tower script (if there is one, keep the placeholder if there isn't one)
        if (tower.fighterIcon != null) icon.sprite = tower.fighterIcon;
        price.text = tower.GetPrice().ToString();
    }

    public void BuildTower()
    {
        GridManager.Instance.Build(UIManager.Instance.GetActiveGridCell(), towerID);
        TogglePopup(false);
    }

    public void TogglePopup(bool toggle)
    {
        informationPopup.SetActive(toggle);
    }
}
