using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Core : Fighter
{
    public CoreTypes coreType;
    Enemy enemyBuffer;
    //3 types
    protected override void Update()
    {
        //no base.Update() because it deals with status effects and retargeting
        //if you want the core to attack or have statuses reimplement it here

        //if enemy reaches the core and core type is TowerLike switch targeting to core
    }


    protected override void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger) return;
        fighterBuffer = other.gameObject.GetComponent<WhereIs>().GetFighter();
        
        if (fighterBuffer == null || fighterBuffer.GetFighterType() != typeof(Enemy)) return; //only deal with enemies entering the collider
        enemyBuffer = (Enemy)fighterBuffer;

        switch (coreType)
        {
            case CoreTypes.TowerLike:
                //changes enemy targeting to core, force attack
                enemyBuffer.SetTargetType(typeof(Core));
                enemyBuffer.ForceBerserk(true);
                break;
            case CoreTypes.EnemiesTotal:
                //each enemy deals 1 damage and dies. Use low health values for core of this type
                GetDamaged(1);
                fighterBuffer.Die(false);
                break;
            case CoreTypes.HealthToDamage:
                //all remaining enemy health is dealt as damage
                GetDamaged(fighterBuffer.GetHealth());
                fighterBuffer.Die(false);
                break;

        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        //nothing here because core doesn't track enemies
    }

    public override void Die(bool coin)
    {
        base.Die(false);
        GameState.Instance.Lose();
    }

    public enum CoreTypes
    {
        HealthToDamage,
        EnemiesTotal,
        TowerLike
    }
}
