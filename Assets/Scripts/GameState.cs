using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    //prefabs
    public List<GameObject> towerPrefabs; //available towers 
    List<GameObject> enemyPrefabs = new List<GameObject>(); //gotten from waves
    private List<Tower> towerScripts = new List<Tower>();
    private List<Enemy> enemyScripts = new List<Enemy>();

    //buffer variables
    Fighter fighterBuffer;
    Enemy enemyBuffer;
    WhereIs whereIsBuffer;
    GameObject gameObjectBuffer;
    Queue<GameObject> queueGameObjectBuffer = new Queue<GameObject>();
    Dictionary<GameObject, int> maxEnemies = new Dictionary<GameObject, int>(); //GO, amount. Used to track how many simultaneous enemies can spawn
    Dictionary<GameObject, int> maxEnemiesWave = new Dictionary<GameObject, int>(); //^ for check within wave
    [Header("GameState info")]
    int money = 100; //starting cash
    public List<SpawnListWrapper> waveSpawns; //wave info
    List<Queue<int>> waveQueues = new List<Queue<int>>();
    int currentWave = -1;
    float untilNextWave;
    float untilNextSpawn;
    bool spawnsAvailable;
    public float betweenWaves = 20f;
    public float betweenSpawns = 2f;

    public int gridX = 7;
    public int gridZ = 7;

    GameStates currentState = GameStates.Waveincoming;


    private void Awake()
    {
        Instance = this;

        untilNextWave = betweenWaves;
        untilNextSpawn = betweenSpawns;
    }

    private void Start()
    {


        for (int i = 0; i < towerPrefabs.Count; i++)
        {
            whereIsBuffer = towerPrefabs[i].GetComponentInChildren<WhereIs>();
            fighterBuffer = whereIsBuffer.GetFighter();
            towerScripts.Add((Tower)whereIsBuffer.GetFighter());
        }

        UIManager.Instance.InitializeBuildUI();
        UIManager.Instance.InitializeGameStateUI(money, waveSpawns.Count);
        GridManager.Instance.GenerateGrid(gridX, gridZ);

        for (int i = 0; i < waveSpawns.Count; i++)
        {
            waveQueues.Add(new Queue<int>());

            foreach (EnemySpawn enemy in waveSpawns[i].spawnList)
            {
                enemyBuffer = (Enemy)enemy.prefab.GetComponent<WhereIs>().fighter;
                if (!enemyPrefabs.Contains(enemy.prefab))
                {
                    enemyPrefabs.Add(enemy.prefab);
                    enemyBuffer.SetGameStateID(enemyPrefabs.Count - 1);
                }

                waveQueues[i].Enqueue(enemyBuffer.GetGameStateID());

                maxEnemiesWave = new Dictionary<GameObject, int>();
                for (int amount = 0; amount < enemy.amount; amount++)
                {
                    if (maxEnemiesWave.ContainsKey(enemy.prefab))
                    {
                        maxEnemiesWave[enemy.prefab] += enemy.amount;
                    }
                    else
                    {
                        maxEnemiesWave.Add(enemy.prefab, enemy.amount);
                    }
                }

                foreach (KeyValuePair<GameObject, int> entry in maxEnemiesWave)
                {
                    if (maxEnemies.ContainsKey(entry.Key))
                    {
                        maxEnemies[entry.Key] = Mathf.Max(maxEnemies[entry.Key], entry.Value);
                    }
                    else
                    {
                        maxEnemies.Add(entry.Key, entry.Value);
                    }
                }
            }

        }

        //queue of max enemies within each wave
        foreach (KeyValuePair<GameObject, int> entry in maxEnemies)
        {
            enemyBuffer = (Enemy)entry.Key.GetComponent<WhereIs>().fighter;
            queueGameObjectBuffer = new Queue<GameObject>();
            for (int i = 0; i < entry.Value; i++)
            {
                gameObjectBuffer = Instantiate(entry.Key);
                gameObjectBuffer.SetActive(false);
                queueGameObjectBuffer.Enqueue(gameObjectBuffer);
            }

            GridManager.Instance.PopulateEnemyPool(enemyBuffer.GetGameStateID(), queueGameObjectBuffer);
        }

        Restart();

    }

    private void Update()
    {
        //check if no enemies left bool is set
        //start countdown to next wave
        if (currentState == GameStates.Waveincoming)
        {
            untilNextWave -= Time.deltaTime;
            if (untilNextWave < 0)
            {
                ChangeState(GameStates.WaveActive);
                untilNextWave = betweenWaves;
                currentWave++;
                GridManager.Instance.InitializeWave(waveQueues[currentWave]);
                spawnsAvailable = true;
            }

        }

        if (currentState == GameStates.WaveActive && spawnsAvailable)
        {
            untilNextSpawn -= Time.deltaTime;
            if (untilNextSpawn < 0)
            {
                spawnsAvailable = GridManager.Instance.SpawnEnemy(); // checks if there is anything left in the queue for the current wave
            }
        }
        //start next spawn
        //
    }

    public void ChangeState(GameStates state)
    {
        currentState = state;

        switch (state)
        {
            case GameStates.WaveActive:
                UIManager.Instance.ChangeWaveText("Wave " + currentWave + "/" + waveSpawns.Count);
                break;
            case GameStates.Won:
                UIManager.Instance.ChangeWaveText("Won");
                break;
            case GameStates.Lost:
                UIManager.Instance.ChangeWaveText("Lost");
                break;
            case GameStates.Waveincoming:
                UIManager.Instance.UpdateWaveInfo(currentWave, betweenWaves);
                break;
        }

        UIManager.Instance.ToggleWaveTimer(state == GameStates.Waveincoming);
    }

    public void Win()
    {
        ChangeState(GameStates.Won);
    }
    public void Lose()
    {
        ChangeState(GameStates.Lost);
    }

    public void Restart()
    {
        if (waveQueues == null || waveQueues.Count == 0)
        {
            Debug.LogWarning("No waves found. Fill in some in GameState");
            ChangeState(GameStates.Won);
            return;
        }

        GridManager.Instance.InitializeWave(waveQueues[currentWave + 1]);

        //TODO restart the whole thing

        //despawn all the active enemies
        //reset UI
        //regenerate the map
        //go through the start methods 1 by 1
    }

    public void Exit()
    {
        Application.Quit();
    }

    //getters
    public GameObject GetTowerPrefab(int id) { return towerPrefabs[id]; }
    public List<Tower> GetTowerScripts() { return towerScripts; }
    public bool CanAfford(int towerID, bool spend = false)
    {
        if (towerScripts[towerID].GetPrice() <= money)
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
        UIManager.Instance.SetCoinCounter(money);
    }
    public void SpendMoney(int sum)
    {
        GetMoney(sum * -1);
    }

}

public enum GameStates
{
    WaveActive,
    Waveincoming,
    Won,
    Lost
}
