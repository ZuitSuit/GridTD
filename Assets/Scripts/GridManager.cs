using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GridManager : MonoBehaviour
{
    public GameObject GridCellPrefab;
    public GameObject GridCellSwampPrefab;

    public GameObject TowerPrefab; //for testing remove later

    public Transform GridParent;
    List<GameObject> GridCells = new List<GameObject>();
    List<CellController> CellControllers = new List<CellController>();
    List<GameObject> GridTowers = new List<GameObject>();
    public Transform EnemyParent;
    public Transform TowerParent;

    public Transform destination;
    public Transform spawnPoint;
    //test object
    public GameObject enemyPrefab;

    Ray ray;
    RaycastHit hit;
    public LayerMask FighterVision; //to ignore fighter vision when placing/selecting
    CellController cellController;
    Fighter fighterBuffer;
    Tower towerBuffer;
    WhereIs whereIsBuffer;

    Fighter fighterInFocus;

    public static GridManager Instance;

    private void Awake()
    {
        Instance = this;
        
    }

    void Start()
    {

        int gridX = 10;
        int gridZ = 10;

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
                CellControllers.Add(cell.GetComponentInChildren<CellController>());
                GridTowers.Add(null);
                cell.GetComponentInChildren<CellController>().SetGridReference(GridCells.Count-1);
            }
        }

        GridParent.GetComponent<NavMeshSurface>().BuildNavMesh();
        GridParent.position = new Vector3(-0.5f * gridX * 10f, 0, -0.5f * gridZ * 10f);

        spawnPoint.localPosition = GridCells[0].transform.localPosition;
        destination.localPosition = GridCells[GridCells.Count - 1].transform.localPosition;

        StartCoroutine(EnemySpawn());
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
                    fighterBuffer.ToggleInFocus(true);
                    UIManager.Instance.TrackFighter(fighterInFocus, whereIsBuffer, fighterBuffer.GetFighterType() == typeof(Tower));
                }
            }
        }
    }

    //getters
    public CellController GetCell(int id) { return CellControllers[id]; }
    public Transform GetSpawnPoint() { return spawnPoint; }
    public Transform GetDestination() { return destination; }

    public float GetDistanceToCore(Transform objectPosition)
    {
        return Vector3.Distance(objectPosition.position, destination.position);
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

            UIManager.Instance.TrackFighter(towerBuffer, whereIsBuffer);
        }

        return false;
    }

    IEnumerator EnemySpawn()
    {
    
        yield return new WaitForSeconds(1f); //wait for navmesh to generate - do this properly
        GameObject testEnemy = Instantiate(enemyPrefab);
        testEnemy.transform.SetParent(EnemyParent);
        testEnemy.SetActive(true);
        //testEnemy.GetComponentInChildren<Fighter>().SpawnCheck();

        testEnemy.GetComponentInChildren<CapsuleCollider>().enabled = false;
        testEnemy.GetComponentInChildren<CapsuleCollider>().enabled = true;
        //StartCoroutine(EnemySpawn());
    }


}
