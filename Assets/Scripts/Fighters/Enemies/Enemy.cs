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
    float pathCheckTimer = 2f;

    //public Animator animator;

    protected override void Awake()
    {
        base.Awake();

        fighterType = typeof(Enemy);
        targetType = typeof(Tower);
        attackType = AttackTypes.RandomTarget;
        defaultSpeed = agent.speed;
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

/*    public override void SetSpeed(float modifier)
    {
        base.SetSpeed(modifier);
        
    }*/

    //TODO param to spawn gibs?
    public override void Die(bool money = true)
    {
        base.Die();
        gameObject.SetActive(false);
        //move back to spawn
        //doesn't clear the cell on which it died
        if (money)
        {
            //give cash
            //GameStateInstance.AddMoney(compensationAmount);
        }
        //tell the towers in range that this is no longer a valid target

    }

    public bool CheckPath()
    {
        return !(agent.pathStatus == NavMeshPathStatus.PathPartial);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        pathCheckTimer -= Time.deltaTime;
        if (pathCheckTimer < 0)
        {
            
            pathCheckTimer = 2f;
            isBerserk = !CheckPath();
        }

        if (!agent.pathPending)
        {
            distanceToDestination = agent.remainingDistance;
        }
    }

}
