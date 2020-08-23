using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    //prefabs
    public List<GameObject> towerPrefabs;
    public List<GameObject> enemyPrefabs;
    private List<Tower> towerScripts = new List<Tower>();
    private List<Enemy> enemyScripts = new List<Enemy>();

    //buffer variables
    Fighter fighterBuffer;
    WhereIs whereIsBuffer;


    //money
    int money = 100;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for(int i = 0; i < towerPrefabs.Count; i++)
        {
            whereIsBuffer = towerPrefabs[i].GetComponentInChildren<WhereIs>();
            fighterBuffer = whereIsBuffer.GetFighter();
            towerScripts.Add((Tower)whereIsBuffer.GetFighter());


        }

        for (int i = 0; i < enemyPrefabs.Count; i++)
        {
            whereIsBuffer = enemyPrefabs[i].GetComponentInChildren<WhereIs>();
            fighterBuffer = whereIsBuffer.GetFighter();
            enemyScripts.Add((Enemy)whereIsBuffer.GetFighter());
        }

        UIManager.Instance.InitializeBuildUI();
        UIManager.Instance.InitializeWaveUI();
    }

    //getters
    public GameObject GetTowerPrefab(int id) { return towerPrefabs[id]; }
    public List<Tower> GetTowerScripts() { return towerScripts; }
    public bool CanAfford(int towerID, bool spend = false)
    {
        if(towerScripts[towerID].GetPrice() <= money)
        {
            if (spend)
            {
                SpendMoney(towerScripts[towerID].GetPrice());
            }

            return true;
        }

        return false;
    }

    //setters
    public void GetMoney(int sum)
    {
        money += sum;
        //TODO UIManager.Instance.RedrawCashUI()
    }
    public void SpendMoney(int sum)
    {
        GetMoney(sum * -1);
    }
}
