using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


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
    [SerializeField] private Image[] perkIconImages;            // 3 icon images (optional)


    [Header("Fallback Icon")]
    [SerializeField] private Sprite fallbackIcon; // shown if perk has no icon assigned

    private List<PerkDefinition> currentSelection = new List<PerkDefinition>();
    [SerializeField]
    private bool perkSelectionDebug = false;
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
        }
    }

    public void Show()
    {
        currentSelection = WeaponManager.Instance.GetRandomPerkSelection(3);

        if (perkSelectionDebug)
        {
            Debug.Log($"[PerkSelectionUI] Show() called — got {currentSelection.Count} perks for weapon: {WeaponManager.Instance.CurrentWeapon.weaponType}");
        }

        if (currentSelection.Count == 0)
        {
            if (perkSelectionDebug)
            {
                Debug.LogWarning("[PerkSelectionUI] No valid perks returned — check WeaponManager All Perks list and perk compatibility settings.");
            }
            return;
        }

        for (int i = 0; i < perkButtons.Length; i++)
        {
            bool hasOption = i < currentSelection.Count;
            perkButtons[i].gameObject.SetActive(hasOption);

            if (hasOption)
            {
                PerkDefinition perk = currentSelection[i];

                perkNameTexts[i].text = perk.perkName;
                perkDescTexts[i].text = perk.description;

                // Icon — use perk icon if available, fallback otherwise
                if (perkIconImages != null && i < perkIconImages.Length && perkIconImages[i] != null)
                {
                    perkIconImages[i].sprite = perk.icon != null ? perk.icon : fallbackIcon;
                    perkIconImages[i].gameObject.SetActive(perk.icon != null || fallbackIcon != null);
                }

                if (perkSelectionDebug)
                {
                    Debug.Log($"[PerkSelectionUI] Slot {i}: {perk.perkName} | Icon: {(perk.icon != null ? perk.icon.name : "none")}");
                }
            }
        }

        canvas.SetActive(true);
        Time.timeScale = 0f;

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.perkSelect, Vector3.zero);
    }

    private void Hide()
    {
        if (perkSelectionDebug)
        {
            Debug.Log("[PerkSelectionUI] Hide() called — resuming game");
        }
        canvas.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnPerkSelected(int index)
    {
        if (index >= currentSelection.Count)
        {
            if (perkSelectionDebug)
            {
                Debug.LogWarning($"[PerkSelectionUI] OnPerkSelected called with out-of-range index: {index}");
            }
            return;
        }

        PerkDefinition chosen = currentSelection[index];
        if (perkSelectionDebug)
        {
            Debug.Log($"[PerkSelectionUI] Player chose: {chosen.perkName}");
        }   

        WeaponManager.Instance.ReceiveLevelUpPerk(chosen);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.perkTriggerGeneric, Vector3.zero);
        Hide();
    }
}
