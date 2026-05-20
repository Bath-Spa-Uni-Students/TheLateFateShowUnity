using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

// Shown at level-up milestones (levels 3, 6, 9) and possibly at level 1 during the tutorial
// Presents 3 random perks valid for the current weapon.
// Player picks one and its sent to the WeaponManager.

public class PerkSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private Button[] perkButtons;              // 3 buttons
    [SerializeField] private TextMeshProUGUI[] perkNameTexts;   // 3 name labels
    [SerializeField] private TextMeshProUGUI[] perkDescTexts;   // 3 description labels

    [Header("Fallback Icon")]
    [SerializeField] private Sprite fallbackIcon;

    private List<PerkDefinition> currentSelection = new List<PerkDefinition>();
    [SerializeField] private bool perkSelectionDebug = false;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (perkSelectionDebug)
            {
                Debug.Log("[PerkSelectionUI] DEBUG: Forcing Show() via P key");
                Show();
            }
        }
    }

    private void Start()
    {
        canvas.SetActive(false);

        for (int i = 0; i < perkButtons.Length; i++)
        {
            int index = i;
            perkButtons[i].onClick.AddListener(() => OnPerkSelected(index));
            AddHoverSound(perkButtons[i]);
        }
    }

    public void Show()
    {
        currentSelection = WeaponManager.Instance.GetRandomPerkSelection(3);

        if (perkSelectionDebug)
            Debug.Log($"[PerkSelectionUI] Show() called — got {currentSelection.Count} perks for weapon: {WeaponManager.Instance.CurrentWeapon.weaponType}");

        if (currentSelection.Count == 0)
        {
            if (perkSelectionDebug)
                Debug.LogWarning("[PerkSelectionUI] No valid perks returned — check WeaponManager All Perks list and perk compatibility settings.");

            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiError, Vector3.zero);
            return;
        }

        for (int i = 0; i < perkButtons.Length; i++)
        {
            bool hasOption = i < currentSelection.Count;
            perkButtons[i].gameObject.SetActive(hasOption);

            if (hasOption)
            {
                PerkDefinition perk = currentSelection[i];
                Image buttonImage = perkButtons[i].GetComponent<Image>();
                buttonImage.sprite = perk.icon != null ? perk.icon : fallbackIcon;
                perkNameTexts[i].text = perk.perkName;
                perkDescTexts[i].text = perk.description;

                if (perkSelectionDebug)
                    Debug.Log($"[PerkSelectionUI] Slot {i}: {perk.perkName} | Icon: {(perk.icon != null ? perk.icon.name : "none")}");
            }
        }

        canvas.SetActive(true);
        Time.timeScale = 0f;

        // perkSelect is an existing gameplay event — pause sound plays on top for the UI open
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiPause, Vector3.zero);
    }

    private void Hide()
    {
        if (perkSelectionDebug)
            Debug.Log("[PerkSelectionUI] Hide() called — resuming game");

        canvas.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnPerkSelected(int index)
    {
        if (index >= currentSelection.Count)
        {
            if (perkSelectionDebug)
                Debug.LogWarning($"[PerkSelectionUI] OnPerkSelected called with out-of-range index: {index}");

            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiError, Vector3.zero);
            return;
        }

        PerkDefinition chosen = currentSelection[index];
        if (perkSelectionDebug)
            Debug.Log($"[PerkSelectionUI] Player chose: {chosen.perkName}");

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiConfirm, Vector3.zero);
        WeaponManager.Instance.ReceiveLevelUpPerk(chosen);
        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            TutorialManager.Instance.AdvanceStep();
        }
        Hide();
    }

    // Adds a hover sound listener to a button via EventTrigger
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