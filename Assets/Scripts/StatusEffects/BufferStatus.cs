using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BufferStatus : StatusEffect
{
    public BufferStatus(Fighter f, int t = 10, float i = 0.2f) : base(f, t, i) { }

    protected override IEnumerator Action()
    {
        //is still invulnerable
        yield return null;
    }

    protected override IEnumerator OnEnd()
    {
        //is vulnerable
        yield return null;
    }
}
