using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    [SerializeField] private Image incomingPerkIcon;            // icon for the arriving perk

    [Header("Current Perk Slots (4 buttons)")]
    [SerializeField] private Button[] currentPerkButtons;       // 4 slot buttons
    [SerializeField] private TextMeshProUGUI[] currentPerkNameTexts;
    [SerializeField] private TextMeshProUGUI[] currentPerkDescTexts;
    [SerializeField] private Image[] currentPerkIcons;          // 4 slot icons

    [Header("Discard Button")]
    [SerializeField] private Button discardButton;

    [Header("Fallback Icon")]
    [SerializeField] private Sprite fallbackIcon;               // shown when perk has no icon

    private Action<int> onDecision; // -1 = discard, 0-3 = replace index
    [SerializeField]
    private bool perkSwapDebug = false;
    private void Start()
    {
        canvas.SetActive(false);

        WeaponManager.Instance.OnPerkSwapRequired += HandleSwapRequired;

        for (int i = 0; i < currentPerkButtons.Length; i++)
        {
            int index = i;
            currentPerkButtons[i].onClick.AddListener(() => OnReplaceChosen(index));
        }

        discardButton.onClick.AddListener(OnDiscard);

        if (perkSwapDebug)
        {
            Debug.Log("[PerkSwapUI] Initialised and subscribed to WeaponManager.OnPerkSwapRequired");
        }
    }

    private void OnDestroy()
    {
        if (WeaponManager.Instance != null)
            WeaponManager.Instance.OnPerkSwapRequired -= HandleSwapRequired;
    }

    /// Called by WeaponManager when a merge hits the 4-perk cap.
    private void HandleSwapRequired(PerkDefinition incoming, List<PerkDefinition> currentPerks, Action<int> callback)
    {
        onDecision = callback;

        if (perkSwapDebug)
        {
            Debug.Log($"[PerkSwapUI] HandleSwapRequired — incoming: {incoming.perkName} | current perks: {currentPerks.Count}");
        }

        // Show incoming perk
        incomingPerkName.text = incoming.perkName;
        incomingPerkDesc.text = incoming.description;
        if (incomingPerkIcon != null)
        {
            incomingPerkIcon.sprite = incoming.icon != null ? incoming.icon : fallbackIcon;
            incomingPerkIcon.gameObject.SetActive(incoming.icon != null || fallbackIcon != null);
        }

        // Show current perks in slot buttons
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
                {
                    Debug.Log($"[PerkSwapUI] Slot {i}: {currentPerks[i].perkName} | Icon: {(currentPerks[i].icon != null ? currentPerks[i].icon.name : "none")}");
                }
            }
            else
            {
                currentPerkButtons[i].gameObject.SetActive(false);
            }
        }

        canvas.SetActive(true);
        Time.timeScale = 0f;

        if (perkSwapDebug)
        {
            Debug.Log("[PerkSwapUI] Canvas shown — game paused");
        }
    }

    private void OnReplaceChosen(int index)
    {
        Debug.Log($"[PerkSwapUI] Player chose to replace slot {index}: {currentPerkNameTexts[index].text}");
        onDecision?.Invoke(index);
        Hide();
    }

    private void OnDiscard()
    {
        if (perkSwapDebug)
        {
            Debug.Log("[PerkSwapUI] Player discarded incoming perk");
        }
        onDecision?.Invoke(-1);
        Hide();
    }

    private void Hide()
    {
        if (perkSwapDebug)
        {
            Debug.Log("[PerkSwapUI] Hide() called — resuming game");
        }
        canvas.SetActive(false);
        Time.timeScale = 1f;
        onDecision = null;
    }
}
