using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleansableDebuff : StatusEffect
{
    [Header("Function")]
    public StatusEffectData[] cleanedBy;
    public int tags = 0;
    public bool esunable = true;
    public bool killsOnExpire = false;

    public override void OnUpdate(CharacterState state)
    {
        uniqueTag = tags;
        for (int i = 0; i < cleanedBy.Length; i++)
        {
            if (state.HasEffect(cleanedBy[i].statusName))
            {
                state.RemoveEffect(data, false, tags);
                return;
            }
        }
        base.OnUpdate(state);
    }

    public override void OnExpire(CharacterState state)
    {
        if (killsOnExpire)
        {
            state.ModifyHealth(0, true);
        }
        base.OnExpire(state);
    }
}
