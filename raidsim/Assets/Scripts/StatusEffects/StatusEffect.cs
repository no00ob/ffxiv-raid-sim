using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static GlobalStructs;

public class StatusEffect : MonoBehaviour
{
    [Header("Base")]
    public StatusEffectData data;
    public float duration;
    public int stacks;
    public int uniqueTag;
    public Damage damage;

    [Header("Events")]
    public UnityEvent<CharacterState> onApplication;
    public UnityEvent<CharacterState> onTick;
    public UnityEvent<CharacterState> onUpdate;
    public UnityEvent<CharacterState> onExpire;
    public UnityEvent<CharacterState> onCleanse;
    public UnityEvent<CharacterState> onReduce;

    private bool hasHudElement = false;
    private GameObject hudElement;
    private TextMeshProUGUI hudTimer;
    private Image hudIcon;
    private bool hasPartyHudElement = false;
    private GameObject partyHudElement;
    private TextMeshProUGUI partyHudTimer;
    private Image partyHudIcon;

    public void Initialize(Transform hudElementParent, Transform partyHudElementParent, Color labelColor, int tag = 0)
    {
        onApplication.AddListener(OnApplication);
        onTick.AddListener(OnTick);
        onUpdate.AddListener(OnUpdate);
        onExpire.AddListener(OnExpire);
        onCleanse.AddListener(OnCleanse);
        onReduce.AddListener(OnReduce);

        if (hudElementParent != null)
        {
            hasHudElement = true;
            hudElement = Instantiate(data.hudElement, hudElementParent);
            hudTimer = hudElement.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            hudIcon = hudElement.transform.GetChild(0).GetComponent<Image>();
            hudTimer.color = labelColor;
        }
        if (partyHudElementParent != null)
        {
            hasPartyHudElement = true;
            partyHudElement = Instantiate(data.hudElement, partyHudElementParent);
            partyHudTimer = partyHudElement.transform.GetChild(1).GetComponentInChildren<TextMeshProUGUI>();
            partyHudIcon = partyHudElement.transform.GetChild(0).GetComponent<Image>();
            partyHudTimer.color = labelColor;
        }

        duration = data.length;
        stacks += data.appliedStacks;
        uniqueTag = tag;

        if (stacks > data.maxStacks)
            stacks = data.maxStacks;

        if (hasHudElement)
        {
            if (!data.infinite && duration >= 1)
                hudTimer.text = duration.ToString("F0");
            else
                hudTimer.text = "";
            if (data.icons.Count > 0)
            {
                hudIcon.sprite = data.icons[stacks - 1];
            }
        }
        if (hasPartyHudElement && duration >= 1)
        {
            if (!data.infinite)
                partyHudTimer.text = duration.ToString("F0");
            else
                partyHudTimer.text = "";
            if (data.icons.Count > 0)
            {
                partyHudIcon.sprite = data.icons[stacks - 1];
            }
        }
    }

    public void Refresh(int appliedStacks = 0, int tag = 0)
    {
        uniqueTag = tag;
        duration += data.length;
        if (appliedStacks == 0)
            stacks += data.appliedStacks;
        else if (appliedStacks > 0)
            stacks += stacks;

        if (duration > data.maxLength)
            duration = data.maxLength;
        if (stacks > data.maxStacks)
            stacks = data.maxStacks;

        if (data.icons.Count > 0)
        {
            hudIcon.sprite = data.icons[stacks - 1];
        }
    }

    public void Remove()
    {
        if (hasHudElement)
            Destroy(hudElement, 0.1f);
        if (hasPartyHudElement)
            Destroy(partyHudElement, 0.1f);
        Destroy(gameObject, 0.1f);
    }

    public virtual void OnApplication(CharacterState state)
    {

    }

    public virtual void OnTick(CharacterState state)
    {
        if (data.infinite)
            Refresh(-1);
    }

    public virtual void OnUpdate(CharacterState state)
    {
        if (hasHudElement)
        {
            if (!data.infinite && duration >= 1)
                hudTimer.text = duration.ToString("F0");
            else
                hudTimer.text = "";
            if (data.icons.Count > 0)
            {
                hudIcon.sprite = data.icons[stacks - 1];
            }
        }
        if (hasPartyHudElement)
        {
            if (!data.infinite && duration >= 1)
                partyHudTimer.text = duration.ToString("F0");
            else
                partyHudTimer.text = "";
            if (data.icons.Count > 0)
            {
                partyHudIcon.sprite = data.icons[stacks - 1];
            }
        }
    }

    public virtual void OnExpire(CharacterState state)
    {
        if (hasHudElement)
            hudTimer.text = "";
        if (hasPartyHudElement)
            partyHudTimer.text = "";
    }

    public virtual void OnCleanse(CharacterState state)
    {
        if (hasHudElement)
            hudTimer.text = "";
        if (hasPartyHudElement)
            partyHudTimer.text = "";
    }

    public virtual void OnReduce(CharacterState state)
    {

    }

    void OnDestroy()
    {
        onApplication.RemoveListener(OnApplication);
        onTick.RemoveListener(OnTick);
        onUpdate.RemoveListener(OnUpdate);
        onExpire.RemoveListener(OnExpire);
        onCleanse.RemoveListener(OnCleanse);
        onReduce.RemoveListener(OnReduce);
    }
}
