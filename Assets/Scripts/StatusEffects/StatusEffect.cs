using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class StatusEffect
{
    protected Fighter fighter;
    protected float time = 0;
    protected int ticks = 0;
    protected float interval = 1.0f;


    public StatusEffect(Fighter f, int t = 1, float i = 1.0f)
    {
        fighter = f;
        interval = i;
        ticks = t;
    }

    public void Init(Fighter f, int t = 1, float i = 1.0f)
    {
        fighter = f;
        interval = i;
        ticks = t;
    }

    public virtual bool Tick()
    {
        if (ticks <= 0)
        {
            ticks = 0;
            time = 0;
            GameState.Instance.StartCoroutine(OnEnd());
            return false;
        }

        time += Time.deltaTime;

        if (time > interval)
        {
            GameState.Instance.StartCoroutine(Action());
            ticks--;
            time = 0;
        }

        return true;
    }

    public virtual void TickUp(int amount = 1)
    {
        ticks += amount;
    }
    public virtual void TickDown(int amount = 1)
    {
        TickUp(amount * -1);
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
