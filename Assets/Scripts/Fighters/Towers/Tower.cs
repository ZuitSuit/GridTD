using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Tower : Fighter
{
    int gridReference; // position on grid
    int gameStateReference; // id in gamestate for outside calls

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
        if (turret != null) turret.LookAt(Vector3.zero);
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }

    public override void Die(bool money = false)
    {
        
        if (money)
        {
            GridManager.Instance.PlayEffect("CoinBurst", transform.position);
            //give cash
            GameState.Instance.GetMoney(price/3); //get third of the money if tower is sold
        }

        base.Die();
        GridManager.Instance.GetCell(gridReference).RemoveTower();

    }

    //getters
    public virtual int GetGameStateID() { return gameStateReference; }


    //setters
    public virtual void Place(int reference)
    {
        gridReference = reference;
    }

    public virtual void SetGameStateID(int id)
    {
        gameStateReference = id;
    }

    public virtual void Sell()
    {
        Die(true);
    }

}