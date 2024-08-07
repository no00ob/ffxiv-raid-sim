using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using static ActionController;

public class TimedMechanic : FightMechanic
{
    public float delay = 1f;
    public bool activateOnStart;
    public UnityEvent onFinish;

    void Start()
    {
        if (activateOnStart)
        {
            TriggerMechanic(new ActionInfo(null, null, null));
        }
    }

    public override void TriggerMechanic(ActionInfo actionInfo)
    {
        base.TriggerMechanic(actionInfo);

        if (delay > 0f)
        {
            Utilities.FunctionTimer.Create(() => onFinish.Invoke(), delay, $"TriggerMechanic_{this}_{GetHashCode()}_{mechanicName}_Activation_Delay", false, true);
        }
        else
        {
            onFinish.Invoke();
        }
    }

    public override void InterruptMechanic(ActionInfo actionInfo)
    {
        base.InterruptMechanic(actionInfo);

        Utilities.FunctionTimer.StopTimer($"TriggerMechanic_{this}_{GetHashCode()}_{mechanicName}_Activation_Delay");
    }
}
