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

    [Header("Current Perk Slots (4 buttons)")]
    [SerializeField] private Button[] currentPerkButtons;       // 4 slots
    [SerializeField] private TextMeshProUGUI[] currentPerkNameTexts;
    [SerializeField] private TextMeshProUGUI[] currentPerkDescTexts;

    [Header("Discard Button")]
    [SerializeField] private Button discardButton;

    private Action<int> onDecision; // -1 = discard, 0-3 = replace index

    private void Start()
    {
        canvas.SetActive(false);
        WeaponManager.Instance.OnPerkSwapRequired += HandleSwapRequired;

        // Wire up current perk buttons
        for (int i = 0; i < currentPerkButtons.Length; i++)
        {
            int index = i;
            currentPerkButtons[i].onClick.AddListener(() => OnReplaceChosen(index));
        }

        discardButton.onClick.AddListener(OnDiscard);
    }

    private void OnDestroy()
    {
        if (WeaponManager.Instance != null)
            WeaponManager.Instance.OnPerkSwapRequired -= HandleSwapRequired;
    }


    // Called by WeaponManager when a merge hits the cap.
    private void HandleSwapRequired(PerkDefinition incoming, List<PerkDefinition> currentPerks, Action<int> callback)
    {
        onDecision = callback;

        // Show incoming perk
        incomingPerkName.text = incoming.perkName;
        incomingPerkDesc.text = incoming.description;

        // Show current perks
        for (int i = 0; i < currentPerkButtons.Length; i++)
        {
            if (i < currentPerks.Count)
            {
                currentPerkButtons[i].gameObject.SetActive(true);
                currentPerkNameTexts[i].text = currentPerks[i].perkName;
                currentPerkDescTexts[i].text = currentPerks[i].description;
            }
            else
            {
                currentPerkButtons[i].gameObject.SetActive(false);
            }
        }

        canvas.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnReplaceChosen(int index)
    {
        onDecision?.Invoke(index);
        Hide();
    }

    private void OnDiscard()
    {
        onDecision?.Invoke(-1);
        Hide();
    }

    private void Hide()
    {
        canvas.SetActive(false);
        Time.timeScale = 1f;
        onDecision = null;
    }
}