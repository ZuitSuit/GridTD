using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class PigeonEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        isFlying = true; // will ignore terrain
    }
}
