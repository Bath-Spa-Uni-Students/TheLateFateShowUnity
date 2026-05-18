using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

// Shown when the player is at the 4-perk cap and receives a new perk.
// Displays current 4 perks + the incoming perk.
// Player can replace one existing perk or discard the incoming one.

public class PerkSwapUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject canvas;

    [Header("Incoming Perk Display")]
    [SerializeField] private TextMeshProUGUI incomingPerkName;
    [SerializeField] private TextMeshProUGUI incomingPerkDesc;
    [SerializeField] private Image incomingPerkIcon;

    [Header("Current Perk Slots (4 buttons)")]
    [SerializeField] private Button[] currentPerkButtons;
    [SerializeField] private TextMeshProUGUI[] currentPerkNameTexts;
    [SerializeField] private TextMeshProUGUI[] currentPerkDescTexts;
    [SerializeField] private Image[] currentPerkIcons;

    [Header("Discard Button")]
    [SerializeField] private Button discardButton;

    [Header("Fallback Icon")]
    [SerializeField] private Sprite fallbackIcon;

    private Action<int> onDecision; // -1 = discard, 0-3 = replace index
    [SerializeField] private bool perkSwapDebug = false;

    private void Start()
    {
        canvas.SetActive(false);

        WeaponManager.Instance.OnPerkSwapRequired += HandleSwapRequired;

        for (int i = 0; i < currentPerkButtons.Length; i++)
        {
            int index = i;
            currentPerkButtons[i].onClick.AddListener(() => OnReplaceChosen(index));
            AddHoverSound(currentPerkButtons[i]);
        }

        discardButton.onClick.AddListener(OnDiscard);
        AddHoverSound(discardButton);

        if (perkSwapDebug)
            Debug.Log("[PerkSwapUI] Initialised and subscribed to WeaponManager.OnPerkSwapRequired");
    }

    private void OnDestroy()
    {
        if (WeaponManager.Instance != null)
            WeaponManager.Instance.OnPerkSwapRequired -= HandleSwapRequired;
    }

    private void HandleSwapRequired(PerkDefinition incoming, List<PerkDefinition> currentPerks, Action<int> callback)
    {
        onDecision = callback;

        if (perkSwapDebug)
            Debug.Log($"[PerkSwapUI] HandleSwapRequired — incoming: {incoming.perkName} | current perks: {currentPerks.Count}");

        incomingPerkName.text = incoming.perkName;
        incomingPerkDesc.text = incoming.description;
        if (incomingPerkIcon != null)
        {
            incomingPerkIcon.sprite = incoming.icon != null ? incoming.icon : fallbackIcon;
            incomingPerkIcon.gameObject.SetActive(incoming.icon != null || fallbackIcon != null);
        }

        for (int i = 0; i < currentPerkButtons.Length; i++)
        {
            if (i < currentPerks.Count)
            {
                currentPerkButtons[i].gameObject.SetActive(true);
                currentPerkNameTexts[i].text = currentPerks[i].perkName;
                currentPerkDescTexts[i].text = currentPerks[i].description;

                if (currentPerkIcons != null && i < currentPerkIcons.Length && currentPerkIcons[i] != null)
                {
                    currentPerkIcons[i].sprite = currentPerks[i].icon != null ? currentPerks[i].icon : fallbackIcon;
                    currentPerkIcons[i].gameObject.SetActive(currentPerks[i].icon != null || fallbackIcon != null);
                }

                if (perkSwapDebug)
                    Debug.Log($"[PerkSwapUI] Slot {i}: {currentPerks[i].perkName} | Icon: {(currentPerks[i].icon != null ? currentPerks[i].icon.name : "none")}");
            }
            else
            {
                currentPerkButtons[i].gameObject.SetActive(false);
            }
        }

        canvas.SetActive(true);
        Time.timeScale = 0f;
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiPause, Vector3.zero);

        if (perkSwapDebug)
            Debug.Log("[PerkSwapUI] Canvas shown — game paused");
    }

    private void OnReplaceChosen(int index)
    {
        Debug.Log($"[PerkSwapUI] Player chose to replace slot {index}: {currentPerkNameTexts[index].text}");
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiConfirm, Vector3.zero);
        onDecision?.Invoke(index);
        Hide();
    }

    private void OnDiscard()
    {
        if (perkSwapDebug)
            Debug.Log("[PerkSwapUI] Player discarded incoming perk");

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiBack, Vector3.zero);
        onDecision?.Invoke(-1);
        Hide();
    }

    private void Hide()
    {
        if (perkSwapDebug)
            Debug.Log("[PerkSwapUI] Hide() called — resuming game");

        canvas.SetActive(false);
        Time.timeScale = 1f;
        onDecision = null;
    }

    private void AddHoverSound(Button button)
    {
        EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener((_) => AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiHover, Vector3.zero));
        trigger.triggers.Add(entry);
    }
}