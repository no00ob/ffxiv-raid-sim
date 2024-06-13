using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static GlobalStructs;

public class CleansableDebuff : StatusEffect
{
    [Header("Function")]
    public StatusEffectData[] cleanedBy;
    public int tags = 0;
    public bool esunable = true;
    public bool killsOnExpire = false;

    public void Reset()
    {
        damage = new Damage(100, true, true, Damage.DamageType.unique, Damage.ElementalAspect.unaspected, Damage.PhysicalAspect.none, Damage.DamageApplicationType.percentageFromMax, string.Empty);
    }

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
            // We need to add a small delay to the health modification or else the fly text for the debuff appears twice, this is a simple unnoticable fix for it.
            Utilities.FunctionTimer.Create(() => state.ModifyHealth(damage, true), 0.1f, $"{gameObject}_{GetHashCode()}_ModifyHealth_Delay", false, true);
        }
        base.OnExpire(state);
    }
}
