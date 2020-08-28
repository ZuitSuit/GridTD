using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect
{
    protected Fighter fighter;
    protected float time = 0;
    protected int ticks = 0;
    protected float interval = 1.0f;
    protected bool infinite = false;


    public StatusEffect(Fighter f, int t = 1, float i = 1)
    {
        fighter = f;
        interval = i;
        ticks = t;
        if (i < 0) infinite = true; 
    }

    public void Init(Fighter f, int t = 1, float i = 1.0f)
    {
        fighter = f;
        interval = i;
        ticks = t;
        if (i < 0) infinite = true;
        
    }

    public virtual bool Tick()
    {
        if (infinite) return true;

        time += Time.deltaTime;

        if (time > interval)
        {

            GameState.Instance.StartCoroutine(Action());
            time = 0;
            if (!TickDown(1)) return false;
        }

        return true;
    }

    public virtual void TickUp(int amount = 1)
    {
        ticks += amount;
    }
    public virtual bool TickDown(int amount = 1)
    {
        TickUp(amount * -1);
        if(ticks < 0)
        {
            GameState.Instance.StartCoroutine(OnEnd());
            return false;
        }
        return true;
    }

    protected abstract IEnumerator Action();
    protected abstract IEnumerator OnEnd();

    public virtual int GetTicks()
    {
        return ticks;
    }


    //setters
    public virtual void SetFighter(Fighter f)
    {
        fighter = f;
    }
}

public enum StatusTypes
{
    Bleed,
    Poison,
    Burn,
    Freeze,
    Shock,
    Buffer,
    Regen
}
