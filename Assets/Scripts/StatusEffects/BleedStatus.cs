using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BleedStatus : StatusEffect
{
    public BleedStatus(Fighter f, int t = 1, float i = 0.2f) : base(f, t, i) { }

    protected override IEnumerator Action()
    {
        int ticks = this.ticks;
        yield return new WaitForSeconds(1f);
        fighter.GetDamaged(ticks);
        yield return null;
    }

    protected override IEnumerator OnEnd()
    {
        //stop bleed animation
        yield return null;
    }
}
