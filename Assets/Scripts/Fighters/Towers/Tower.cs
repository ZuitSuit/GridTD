using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tower : Fighter
{
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
        if(turret != null )turret.LookAt(Vector3.zero);
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