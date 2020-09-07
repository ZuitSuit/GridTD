using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : Tower
{
    // Start is called before the first frame update
    public LineRenderer laser;
    float laserFiring;
    float laserTime = .3f;
    bool shootingLaser;
    Transform currentTargetBuffer;

    protected override void Awake()
    {
        base.Awake();
        shotCooldown = 5f;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();

        if (shootingLaser)
        {
            laser.SetPosition(1, laser.transform.InverseTransformPoint(currentTargetBuffer.transform.position));
            if((laserFiring += Time.deltaTime) > laserTime || !currentTargetBuffer.gameObject.activeInHierarchy)
            {
                shootingLaser = false;
                laser.SetPosition(1, laser.GetPosition(0));
            }

        }
    }

    protected override void Attack()
    {
        currentTargetBuffer = currentTarget.transform;
        shootingLaser = true;
        laserFiring = 0;
        base.Attack();
    }

}
