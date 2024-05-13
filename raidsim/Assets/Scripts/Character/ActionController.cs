using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActionController : MonoBehaviour
{
    Animator animator;
    CharacterState characterState;

    public List<CharacterAction> actions = new List<CharacterAction>();
    public bool instantCast = false;
    public bool isAnimationLocked = false;
    public bool isCasting = false;
    public bool waitInterruptedCasts = false;
    public bool lockActionsWhenCasting = true;
    private bool previousCanDoActions;
    private bool previousIsCasting;

    public CharacterState currentTarget;

    [Header("Personal")]
    public bool useCastBar;
    public Slider castBar;
    public TextMeshProUGUI castNameText;
    public TextMeshProUGUI castLengthText;
    public CanvasGroup interruptText;
    public HudElement castBarElement;

    [Header("Party")]
    public bool usePartyCastBar;
    public Slider castBarParty;
    public TextMeshProUGUI castNameTextParty;

    private CanvasGroup castBarGroupParty;
    private CanvasGroup castBarGroup;

    private CharacterAction lastAction;
    private float castTime;
    private float lastCastTime;
    private bool interrupted;

    void Awake()
    {
        animator = GetComponent<Animator>();
        characterState = GetComponent<CharacterState>();

        if (castBar != null)
            castBarGroup = castBar.GetComponent<CanvasGroup>();
        if (castBarParty != null)
            castBarGroupParty = castBarParty.GetComponentInParent<CanvasGroup>();

        if (interruptText != null)
        {
            interruptText.alpha = 0f;
        }

        if (characterState != null )
        {
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].Initialize(this);
            }
        }
        previousCanDoActions = characterState.canDoActions;
    }

    void Update()
    {
        if (previousCanDoActions != characterState.canDoActions || previousIsCasting != isCasting)
        {
            previousCanDoActions = characterState.canDoActions;
            previousIsCasting = isCasting;
            if (characterState.canDoActions && ((!isCasting && lockActionsWhenCasting) || (!lockActionsWhenCasting)))
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    actions[i].isDisabled = false;
                }
            }
            else
            {
                for (int i = 0; i < actions.Count; i++)
                {
                    actions[i].isDisabled = true;
                }
            }
        }

        if (castTime > 0f && !interrupted)
        {
            // Simulate FFXIV slidecasting to an extent (Not perfect)
            if (!characterState.still && castTime > (lastCastTime / 5f))
            {
                Interrupt();
            }
            if (!characterState.still)
            {
                if (lastAction != null)
                {
                    if (!lastAction.data.canBeSlideCast)
                        Interrupt();
                }
            }

            castTime -= Time.deltaTime;
            if (castBar != null)
            {
                castBar.value = lastCastTime - castTime;

                if (castBarGroup.alpha == 0f)
                {
                    castBarGroup.LeanAlpha(1f, 0.1f);
                }
                if (interruptText != null)
                {
                    interruptText.alpha = 0f;
                }
            }
            if (castBarParty != null)
            {
                castBarParty.value = lastCastTime - castTime;

                if (castBarGroupParty.alpha == 0f)
                {
                    castBarGroupParty.alpha = 1f;
                }
            }
            if (castLengthText != null)
            {
                castLengthText.text = castTime.ToString("00.00");
            }
        }
        else
        {
            if (interrupted)
            {
                if (castBar != null && castBarGroup.alpha == 1f)
                {
                    Utilities.FunctionTimer.Create(() => castBarGroup.LeanAlpha(0f, 0.5f), 2f, $"{this}_castBar_fade_out_if_interrupted", true);
                }
                if (castBarParty != null && castBarGroupParty.alpha == 1f)
                {
                    Utilities.FunctionTimer.Create(() => castBarGroupParty.alpha = 0f, 2f, $"{this}_castBarParty_fade_out_if_interrupted", true);
                }
                Utilities.FunctionTimer.Create(() => ResetCastBar(), 2.5f, $"{this}_interrupted_status", true);
            }
            else
            {
                if (castBar != null && castBarGroup.alpha == 1f)
                {
                    castBarGroup.LeanAlpha(0f, 0.5f);
                }
                if (castBarParty != null && castBarGroupParty.alpha == 1f)
                {
                    castBarGroupParty.alpha = 0f;
                }
            }
        }
    }

    public void PerformAction(string name)
    {
        for (int i = 0; i < actions.Count; i++)
        {
            if (actions[i].data.actionName == name)
            {
                PerformAction(actions[i]);
            }
        }
    }

    public void PerformAction(CharacterAction action)
    {
        if (action == null)
            return;

        action.OnPointerClick(null);

        if (lockActionsWhenCasting && isCasting)
            return;

        interrupted = false;
        if (castBarElement != null)
            castBarElement.ChangeColors(false);

        if (action.isAvailable && !action.isDisabled)
        {
            if (action.data.cast <= 0f)
            {
                action.ExecuteAction(new ActionInfo(characterState, currentTarget));
                action.ActivateCooldown();
                if (animator != null)
                {
                    animator.SetBool("Casting", false);
                }
            }
            else
            {
                Utilities.FunctionTimer.StopTimer($"{this}_castBar_fade_out_if_interrupted");
                Utilities.FunctionTimer.StopTimer($"{this}_castBarParty_fade_out_if_interrupted");
                Utilities.FunctionTimer.StopTimer($"{this}_interrupted_status");
                ResetCastBar();
                if (castBarGroup != null)
                    castBarGroup.alpha = 0f;
                if (castBarGroupParty != null)
                    castBarGroupParty.alpha = 0f;

                isCasting = true;
                lastAction = action;
                castTime = action.data.cast;
                lastCastTime = castTime;
                action.ActivateCooldown();
                StartCoroutine(Cast(castTime, () => { action.ExecuteAction(new ActionInfo(characterState, currentTarget)); }));
                if (castBar != null)
                {
                    castBar.maxValue = action.data.cast;
                    castBar.value = 0f;
                }
                if (castLengthText != null)
                {
                    castLengthText.text = castTime.ToString("00.00");
                }
                if (castNameText != null)
                {
                    castNameText.text = action.data.actionName;
                }
                if (castBarParty != null)
                {
                    castBarParty.maxValue = action.data.cast;
                    castBarParty.value = 0f;
                }
                if (castNameTextParty != null)
                {
                    castNameTextParty.text = action.data.actionName;
                }
                if (interruptText != null)
                {
                    interruptText.alpha = 0f;
                }
                if (animator != null)
                {
                    animator.SetBool("Casting", true);
                }
            }
        }
        else if (characterState.canDoActions && !action.isDisabled)
        {
            FailAction(action, "Action not ready yet.");
        }
        else if (characterState.canDoActions && action.isDisabled)
        {
            FailAction(action, "Action not available right now.");
        }
        else
        {
            FailAction(action, "Actions not available right now.");
        }
    }

    public void FailAction(CharacterAction action, string reason)
    {
        // IDK
    }

    private void Interrupt()
    {
        interrupted = true;
        isCasting = false;
        if (interruptText != null)
        {
            interruptText.LeanAlpha(1f, 0.5f);
        }
        if (lastAction != null)
        {
            lastAction.ResetCooldown();
            lastAction = null;
        }
        if (animator != null)
        {
            animator.SetBool("Casting", false);
        }
        if (castBarElement != null)
            castBarElement.ChangeColors(true);
        StopAllCoroutines();
        //Utilities.FunctionTimer.Create(() => { ResetCastBar(); }, 4f, $"{this}_interrupt", true);
    }

    private IEnumerator Cast(float length, Action action)
    {
        yield return new WaitForSeconds(length);
        action.Invoke();
        isCasting = false;
        lastAction = null;
        if (animator != null)
        {
            animator.SetBool("Casting", false);
        }
    }

    private void ResetCastBar()
    {
        interrupted = false;
        castTime = 0f;
        if (castBar != null)
        {
            castBar.value = 0f;
        }
        if (castBarParty != null)
        {
            castBarParty.value = 0f;
        }
        if (interruptText != null)
        {
            interruptText.alpha = 0f;
        }
    }

    public struct ActionInfo
    {
        public CharacterState source;
        public CharacterState target;

        public ActionInfo(CharacterState source, CharacterState target)
        {
            this.source = source;
            this.target = target;
        }
    }
}