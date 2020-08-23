using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    //prefabs
    public List<GameObject> towerPrefabs;
    List<Tower> towersSelection;
    public List<GameObject> enemyPrefabs;
    List<Enemy> enemySelection;

    //buffer variables
    Fighter fighterBuffer;
    WhereIs whereIsBuffer;


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        foreach(GameObject tower in towerPrefabs)
        {
            whereIsBuffer = tower.GetComponentInChildren<WhereIs>();
            fighterBuffer = whereIsBuffer.GetFighter();
            towersSelection.Add((Tower)whereIsBuffer.GetFighter());
        }

        foreach (GameObject enemy in enemyPrefabs)
        {
            whereIsBuffer = enemy.GetComponentInChildren<WhereIs>();
            fighterBuffer = whereIsBuffer.GetFighter();
            enemySelection.Add((Enemy)whereIsBuffer.GetFighter());
        }



        UIManager.Instance.InitializeBuildUI(towersSelection);
        UIManager.Instance.InitializeWaveUI(enemySelection);
    }
}
