using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tower : Fighter
{
    public GameObject towerParent;
    int gridReference;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        fighterType = typeof(Tower);
        //default tower settings
        targetType = typeof(Enemy);
        attackType = AttackTypes.SingleTarget;

        fighterRangeDefault = range.radius;
        range.radius = fighterRange;
        turret.LookAt(Vector3.zero);
    }

    protected override void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    override public void Die(bool money = false)
    {
        base.Die();
        if (money)
        {
            //TODO refund % of price 
        }

        //GridManager.Instance.RemoveTarget(gameObject, visibleByTowers, false);
        //do the cool animation here then despawn -> to pool
        towerParent.SetActive(false);
    }

    public virtual void Place(int reference)
    {
        gridReference = reference;
    }

    public virtual void RemoveTarget(GameObject target, bool enemy = true)
    {
        if (currentTarget == target) Retarget();

        fighterBuffer = target.GetComponentInChildren<Fighter>();
        targetsInRange[fighterBuffer.GetFighterType().ToString()].Remove(target);
    }


}