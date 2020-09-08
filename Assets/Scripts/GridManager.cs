using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    [Header("Cell prefabs")]
    public GameObject GridCellPrefab;
    public GameObject GridCellSwampPrefab;

    List<GameObject> GridCells = new List<GameObject>();
    List<CellController> CellControllers = new List<CellController>();
    List<GameObject> GridTowers = new List<GameObject>(); //active towers 1:1 with cells
    List<GameObject> GridEnemies = new List<GameObject>(); //active enemies
    List<NavMeshSurface> GridSurfaces = new List<NavMeshSurface>(); //list of surface components for different agents

    //track queues by prefab ids in gamestate
    Dictionary<int, Queue<GameObject>> EnemyPools = new Dictionary<int, Queue<GameObject>>();
    Dictionary<int, Queue<GameObject>> TowerPools = new Dictionary<int, Queue<GameObject>>();
    Queue<int> currentWave = new Queue<int>(); // Queue of GameState ids for enemies

    [Header("GO parents")]
    public Transform GridParent;
    public Transform EnemyParent;
    public Transform TowerParent;
    public Transform EffectsParent;

    [Header("Path endpoints")]
    public Transform destination;
    public Transform spawnPoint;

    Ray ray;
    RaycastHit hit;
    public LayerMask FighterVision;
    CellController cellController;

    //buffers
    Fighter fighterBuffer;
    Tower towerBuffer;
    WhereIs whereIsBuffer;
    GameObject gameObjectBuffer;
    NavMeshSurface navmeshSurfaceBuffer;

    Fighter fighterInFocus;

    public List<ParticleSystem> particlePrefabs;
    Dictionary<string, Queue<ParticleSystem>> particleQueues = new Dictionary<string, Queue<ParticleSystem>>();
    Queue<ParticleSystem> particleQueueBuffer = new Queue<ParticleSystem>();
    ParticleSystem particleSystemBuffer;
    string particleNameBuffer;

    private void Awake()
    {
        Instance = this;

        //particle system setup
        foreach (ParticleSystem particle in particlePrefabs)
        {
            particleQueueBuffer = new Queue<ParticleSystem>();
            for (int i = 0; i < 50; i++)
            {
                particleNameBuffer = particle.name;
                particleSystemBuffer = Instantiate(particle, EffectsParent);
                particleSystemBuffer.name = particleNameBuffer;
                particleQueueBuffer.Enqueue(particleSystemBuffer);
            }

            particleQueues.Add(particleNameBuffer, particleQueueBuffer);
        }


    }

    void Start()
    {
        GenerateGrid(7, 7);

    }

    // Update is called once per frame
    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Input.GetButtonDown("Fire"))
        {
            if (Physics.Raycast(ray, out hit, 1000f, FighterVision - 5)) //-1 to invert mask, -4 to ignore raycast
            {
                if (hit.collider.gameObject.GetComponent<CellManager>() != null)
                {
                    cellController = hit.collider.gameObject.GetComponent<CellManager>().controller;
                    UIManager.Instance.BuildUI(cellController);
                    //TODO controller calls build UI
                }

                else if (hit.collider.gameObject.GetComponent<WhereIs>() != null)
                {
                    whereIsBuffer = hit.collider.gameObject.GetComponent<WhereIs>();
                    fighterBuffer = whereIsBuffer.GetFighter();
                    if (fighterInFocus != null) fighterInFocus.ToggleInFocus(false);
                    fighterInFocus = fighterBuffer;
                    fighterInFocus.ToggleInFocus(true);
                    UIManager.Instance.TrackFighter(fighterInFocus, whereIsBuffer, fighterBuffer.GetFighterType() == typeof(Tower));
                }
            }
        }
    }

    public void GenerateGrid(int gridX, int gridZ)
    {
        //get grid array from the scene

        for (int ix = 0; ix < gridX; ix++)
        {
            for (int iz = 0; iz < gridZ; iz++)
            {
                GameObject cell = Instantiate(Random.Range(0, 7) == 1 ? GridCellSwampPrefab : GridCellPrefab);
                cell.name = "cell_" + ix + ":" + iz;
                cell.transform.SetParent(GridParent);
                cell.transform.localPosition = new Vector3(ix * 10f, 0, iz * 10f);
                GridCells.Add(cell);
                GridTowers.Add(null);
                CellControllers.Add(cell.GetComponentInChildren<CellController>());
                cell.GetComponentInChildren<CellController>().SetGridReference(GridCells.Count - 1);
            }
        }

        List<int> agents = GameState.Instance.GetGridAgents();
        for (int i = 0; i < agents.Count; i++)
        {

            navmeshSurfaceBuffer = GridParent.gameObject.AddComponent(typeof(NavMeshSurface)) as NavMeshSurface;
            navmeshSurfaceBuffer.agentTypeID = agents[i];
            navmeshSurfaceBuffer.layerMask = 1 << LayerMask.NameToLayer("GridGround");
            navmeshSurfaceBuffer.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            navmeshSurfaceBuffer.BuildNavMesh();
            GridSurfaces.Add(navmeshSurfaceBuffer);
            StartCoroutine(CheckGridGeneration(GridSurfaces.Count - 1));

        }



        GridParent.position = new Vector3(-0.5f * gridX * 10f, 0, -0.5f * gridZ * 10f);

        spawnPoint.localPosition = GridCells[0].transform.localPosition;
        destination.localPosition = GridCells[GridCells.Count - 1].transform.localPosition;
    }

    public void InitializeWave(Queue<int> enemies)
    {
        currentWave = enemies;
    }

    //getters
    public CellController GetCell(int id) { return CellControllers[id]; }
    public Transform GetSpawnPoint() { return spawnPoint; }
    public Transform GetDestination() { return destination; }

    public float GetDistanceToCore(Transform objectPosition)
    {
        return Vector3.Distance(objectPosition.position, destination.position);
    }


    public void PlayEffect(string effectName, Vector3 position)
    {
        if (particleQueues.ContainsKey(effectName) && particleQueues.Count > 0)
        {
            particleSystemBuffer = particleQueues[effectName].Dequeue();
            particleSystemBuffer.transform.position = position;
            particleSystemBuffer.Play();
            StartCoroutine(EnqueueParticle(particleSystemBuffer));
        }
    }

    public bool Build(int gridID, int towerID)
    {
        if (CellControllers[gridID].CanBuild() && GameState.Instance.CanAfford(towerID, true))
        {

            GameObject tower = Instantiate(GameState.Instance.GetTowerPrefab(towerID));
            tower.transform.SetParent(TowerParent);
            tower.transform.position = CellControllers[gridID].transform.position;
            whereIsBuffer = tower.GetComponent<WhereIs>();
            towerBuffer = (Tower)whereIsBuffer.GetFighter();
            towerBuffer.Place(gridID);
            //tower.GetComponentInChildren<Fighter>().SpawnCheck();

            GridTowers[cellController.GetGridReference()] = tower;
            cellController.SetTower(tower.GetComponentInChildren<Tower>().GetGameStateID());

            UIManager.Instance.TrackFighter(towerBuffer, whereIsBuffer, true);
            towerBuffer.ToggleInFocus(true);
        }

        return false;
    }

    public void ResetCamera()
    {
        //TODO do all the camera centering her
    }

    IEnumerator EnqueueParticle(ParticleSystem particleSystem)
    {
        yield return new WaitForSeconds(particleSystem.main.duration + 0.1f);
        if (particleQueues.ContainsKey(particleSystem.name))
        {
            particleQueues[particleSystem.name].Enqueue(particleSystem);
        }
    }

    //fires on every enemy death
    public void CheckWaveEnd()
    {
        Debug.Log(GridEnemies.Count);
        if (GridEnemies.Count == 0)
        {
            GameState.Instance.NextWave();
        }
    }

    public bool SpawnEnemy()
    {
        gameObjectBuffer = EnemyPools[currentWave.Dequeue()].Dequeue();
        gameObjectBuffer.transform.SetParent(EnemyParent);
        gameObjectBuffer.SetActive(true);
        GridEnemies.Add(gameObjectBuffer);

        return currentWave.Count > 0;
    }

    public void DespawnEnemy(GameObject enemy, int gameStateID)
    {
        EnemyPools[gameStateID].Enqueue(enemy);
        if (GridEnemies.Contains(enemy)) GridEnemies.Remove(enemy);
    }

    public void PopulateEnemyPool(int id, Queue<GameObject> pool)
    {
        EnemyPools[id] = pool;
    }

    public IEnumerator CheckGridGeneration(int id)
    {
        
        yield return new WaitForSeconds(0.1f);
        if (GridSurfaces[id].navMeshDataInstance.valid)
        {
            GameState.Instance.NavMeshGenerated();
        }
        else
        {
            StartCoroutine(CheckGridGeneration(id));
        }
    }
}