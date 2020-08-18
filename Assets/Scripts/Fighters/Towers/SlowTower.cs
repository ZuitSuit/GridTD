using System.Collections.Generic;
using UnityEngine;

public class SlowTower : Tower
{
    // Start is called before the first frame update
    protected override void Awake()
    {
        base.Awake();
        effectsOnHit.Add(new SlowStatus(null, 1, 4f));
        shotCooldown = 2f;
        attackType = AttackTypes.AoE;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
