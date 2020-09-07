using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public class TortoiseEnemy : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        AddStatusEffect(new BufferStatus(this, 1, -1f), 10);
        //tortoise starts with 10 free hits
    }
}
