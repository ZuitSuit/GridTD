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
        AddStatusEffect<BufferStatus>(10);
        
        //tortoise starts with 10 free hits
    }

    public override void ResetStats()
    {
        base.ResetStats();
        AddStatusEffect<BufferStatus>(10);
    }
}
