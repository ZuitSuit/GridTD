using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tower : Fighter
{
    public GameObject towerParent;
    public Transform turret;
    int gridReference;
    float price;
    float rotationSpeed = 0.2f;

    //targeting variables
    GameObject potentialTarget;
    float comparisonValue;
    float comparisonValueCheck;
    bool firstComparison = false;
    bool comparisonMore = true;
    List<GameObject> targetsBuffer;
    float timeFromLastTarget = 0f;

    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();

        //default tower settings
        targetType = FighterTypes.Enemy;
        attackType = AttackTypes.SingleTarget;

        radiusDefault = range.radius;
        range.radius *= radius;
        turret.LookAt(Vector3.zero);
    }

    protected override void Start()
    {

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        timeFromLastTarget += Time.deltaTime;
        timeSinceLastShot += Time.deltaTime;

        if(attackType != AttackTypes.AoE)
        {
            if (currentTarget != null && currentTarget.activeInHierarchy)
            {
                turret.transform.localRotation = Quaternion.Slerp(turret.transform.localRotation, Quaternion.LookRotation(currentTarget.transform.position - turret.transform.position), rotationSpeed * timeFromLastTarget);

            }
            else
            {
                Retarget();
            }
        }
    }

    override public void Die(bool money = false)
    {
        if (money)
        {
            //TODO refund % of price 
        }

        GridManager.Instance.RemoveTarget(gameObject, visibleByTowers, false);
        //do the cool animation here then despawn -> to pool
        towerParent.SetActive(false);
    }



    public virtual void Retarget()
    {
        currentTarget = null;
        //random targeting
        if (attackType == AttackTypes.RandomTarget)
        {
            targetsBuffer = GetCurrentTargets();
            currentTarget = targetsBuffer[Random.Range(0, targetsBuffer.Count)];
            return;
        }

        if ((targetType == FighterTypes.Enemy && enemiesInRange.Count != 0) || (targetType == FighterTypes.Tower && towersInRange.Count != 0))
        {
            firstComparison = true;
            foreach (GameObject possibleTarget in GetCurrentTargets())
            {
                switch (targetingMode)
                {
                    case TargetingModes.Closest:
                        comparisonValueCheck = possibleTarget.GetComponentInChildren<Fighter>().GetDistance();
                        comparisonMore = true; //TODO remove from this place?
                        break;
                    case TargetingModes.Fastest:
                        comparisonValueCheck = possibleTarget.GetComponentInChildren<Fighter>().GetSpeed();
                        comparisonMore = true;
                        break;
                    case TargetingModes.MostHP:
                        comparisonValueCheck = possibleTarget.GetComponentInChildren<Fighter>().GetHealth();
                        comparisonMore = true;
                        break;
                    case TargetingModes.LeastHP:
                        comparisonValueCheck = possibleTarget.GetComponentInChildren<Fighter>().GetHealth();
                        comparisonMore = false;
                        break;
                }

                if (firstComparison || (comparisonMore ? (comparisonValueCheck > comparisonValue) : (comparisonValueCheck < comparisonValue)))
                {
                    potentialTarget = possibleTarget;
                    comparisonValue = comparisonValueCheck;
                }
            }

            //TODO choses the same target twice

            timeFromLastTarget = 0;
            firstComparison = false;
            currentTarget = potentialTarget;
        }
    }
    //Shoot
    //Cooldown
    //Retarget
    //SetPriority

    public virtual void Place(int reference)
    {
        gridReference = reference;
    }

    public virtual void RemoveTarget(GameObject target, bool enemy = true)
    {
        if (currentTarget == target) Retarget();

        if (enemy)
        {
            enemiesInRange.Remove(target);
            return;
        }
        towersInRange.Remove(target);
    }

    private void OnTriggerEnter(Collider other)
    {
        //show the object that it is seen by a tower
        //add to objects in range

        switch (other.gameObject.tag) 
        {
            case "Enemy":
                other.gameObject.GetComponentInChildren<Enemy>().ToggleTowerVisibility(gridReference, true);
                enemiesInRange.Add(other.gameObject);
                break;
            case "Tower":
                other.gameObject.GetComponentInChildren<Tower>().ToggleTowerVisibility(gridReference, true);
                towersInRange.Add(other.gameObject);
                break;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //show the object that it has left the tower range
        //remove object from available targets

        switch (other.gameObject.tag)
        {
            case "Enemy":
                other.gameObject.GetComponentInChildren<Enemy>().ToggleTowerVisibility(gridReference, false);
                enemiesInRange.Remove(other.gameObject);
                if (currentTarget == other.gameObject) Retarget();
                break;
            case "Tower":
                other.gameObject.GetComponentInChildren<Tower>().ToggleTowerVisibility(gridReference, false);
                towersInRange.Remove(other.gameObject);
                if (currentTarget == other.gameObject) Retarget();
                break;
        }
    }
}