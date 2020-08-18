using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowStatus : StatusEffect
{
    public SlowStatus(Fighter f, int t = 1, float i = 2f) : base(f, t, i) { }

    protected override IEnumerator Action()
    {
        int ticks = this.ticks;
        yield return new WaitForSeconds(1f);
        fighter.SetSpeedModifier(1f - 0.5f * (1f + (ticks -1)/ticks));
        fighter.RecalculateSpeed();
        yield return null;
    }

    protected override IEnumerator OnEnd()
    {
        fighter.SetSpeedModifier(1.0f);
        fighter.RecalculateSpeed();
        yield return null;
    }
}
