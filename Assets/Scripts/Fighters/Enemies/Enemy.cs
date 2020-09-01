using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : Fighter
{
    
    Transform destination;
    Transform spawn;

    bool isBerserk = false;
    bool forceBerserk = false;
    float pathCheckTimer = 2f;
    int gameStateReference;

    //public Animator animator;

    protected override void Awake()
    {
        base.Awake();

        fighterType = typeof(Enemy);
        targetType = typeof(Tower);
        attackType = AttackTypes.RandomTarget;
        defaultSpeed = agent.speed;
    }
    protected override void Update()
    {
        base.Update();

        pathCheckTimer -= Time.deltaTime;
        if (pathCheckTimer < 0)
        {

            pathCheckTimer = 2f;
            isBerserk = forceBerserk || !CheckPath();
        }

        if (!agent.pathPending)
        {
            distanceToDestination = agent.remainingDistance;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        agent.enabled = false;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        isBerserk = false;

        if (!agent.enabled)
        {
            spawn = GridManager.Instance.GetSpawnPoint();
            agent.Warp(spawn.position);
            agent.enabled = true;
            SetDestination(GridManager.Instance.GetDestination());
        }

        agent.isStopped = false;
    }

    protected override void Attack()
    {
        if (isBerserk)
        {
            base.Attack();
        }
    }

    public void SetDestination(Transform destination)
    {
        this.destination = destination;
        agent.SetDestination(destination.position);
    }

    public void ForceBerserk(bool toggle = true)
    {
        forceBerserk = toggle;
    }

    //TODO param to spawn gibs?
    public override void Die(bool money = true)
    {
        money = true; //TODO placeholder because OOP failed me here
        if (money)
        {
            GridManager.Instance.PlayEffect("CoinBurst", transform.position);
            //give cash
            GameState.Instance.GetMoney(price);
        }
        agent.Warp(spawn.position); //move enemy back to spawn before it's death

        GridManager.Instance.DespawnEnemy(GetFighterParent().gameObject, gameStateReference);
        GridManager.Instance.CheckWaveEnd();
        base.Die();
        //move back to spawn
        //doesn't clear the cell on which it died

    }

    //getters
    public bool CheckPath()
    {
        return !(agent.pathStatus == NavMeshPathStatus.PathPartial);
    }
    public virtual int GetGameStateID()
    {
        return gameStateReference;
    }

    //setters
    public virtual void SetGameStateID(int id)
    {
        gameStateReference = id;
    }

}
