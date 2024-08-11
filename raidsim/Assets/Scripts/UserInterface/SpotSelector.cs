using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static PartyList;

public class SpotSelector : MonoBehaviour
{
    TMP_Dropdown dropdown;
    
    public PartyList partyList;
    public bool autoObtainPlayer;
    public PartyMember player;
    public AIController[] bots;
    public BotNode[] spots;

#if UNITY_EDITOR
    void OnValidate()
    {
        if (autoObtainPlayer && partyList != null && partyList.members != null && partyList.members.Count > 0)
        {
            player = partyList.members[0];
        }
    }
#endif

    void Awake()
    {
        if (autoObtainPlayer && partyList != null && partyList.members != null && partyList.members.Count > 0)
        {
            player = partyList.members[0];
        }
    }

    void Start()
    {
        dropdown = GetComponentInChildren<TMP_Dropdown>();
        Select(0);
    }

    void Update()
    {
        dropdown.interactable = !FightTimeline.Instance.playing;
    }

    public void Select(int value)
    {
        if (player.playerController == null)
            Debug.LogError("PlayerController is unassigned!");

        player.playerController.clockSpot = spots[value];

        int botNameIndex = 0; 

        for (int i = 0; i < bots.Length; i++)
        {
            if (bots[i].clockSpot == player.playerController.clockSpot)
            {
                bots[i].state.characterName = "Disabled";
                bots[i].gameObject.SetActive(false);
            }
            else
            {
                bots[i].state.characterName = $"AI{botNameIndex + 1}";
                bots[i].gameObject.SetActive(true);
                botNameIndex++;
            }
        }

        partyList.UpdatePartyList();
    }
}
