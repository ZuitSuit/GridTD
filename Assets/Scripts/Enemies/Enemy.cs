using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Fighter
{
    public NavMeshAgent agent;
    Transform _destination;
    Transform _spawn;

    float defaultSpeed;

    bool _isBerserk = false;
    float _pathCheckTimer = 2f;

    //public Animator animator;

    protected override void Awake()
    {
        base.Awake();
        targetType = FighterTypes.Tower;
        attackType = AttackTypes.RandomTarget;
        defaultSpeed = agent.speed;
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        agent.enabled = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        _isBerserk = false;

        if (!agent.enabled)
        {
            _spawn = GridManager.Instance.GetSpawnPoint();
            agent.Warp(_spawn.position);
            agent.enabled = true;
            SetDestination(GridManager.Instance.GetDestination());
        }

        agent.isStopped = false;
    }

    public void SetDestination(Transform destination)
    {
        _destination = destination;
        agent.SetDestination(destination.position);
    }

    public void SetSpeed(float modifier = 1.0f)
    {
        agent.speed = defaultSpeed / modifier;
    }
    
    //TODO param to spawn gibs?
    public override void  Die()
    {
        //tell the towers it's in range of that it's no longer a valid target
        GridManager.Instance.RemoveTarget(gameObject, visibleByTowers, true);

        gameObject.SetActive(false);
    }

    public bool CheckPath()
    {
        return !(agent.pathStatus == NavMeshPathStatus.PathPartial);
    }

    // Update is called once per frame
    void Update()
    {
        //TODO rework this mb?
        _pathCheckTimer -= Time.deltaTime;
        if(_pathCheckTimer < 0)
        {
            _pathCheckTimer = 2f;
            _isBerserk = !CheckPath();
        }

        if (!agent.pathPending)
        {
            distanceToDestination = agent.remainingDistance;
        }


    }

}
