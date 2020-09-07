using System.Collections.Generic;
using UnityEngine;

public class NeedleTower : Tower
{
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        effectsOnHit.Add(new BleedStatus(null, 1, 0.2f));
        shotCooldown = 0.3f;
        attackType = AttackTypes.SingleTarget;

    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
