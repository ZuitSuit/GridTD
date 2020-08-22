using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameState : MonoBehaviour
{
    public static GameState Instance;


    public Dictionary<Image, GameObject> towerPrefabs;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        
    }
}
