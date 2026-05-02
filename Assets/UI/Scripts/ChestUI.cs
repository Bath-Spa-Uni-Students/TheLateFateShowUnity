using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class ChestUI : MonoBehaviour
{


    private enum ChestUIMode { NewGun, MergePerk, PerkSwap }
    private ChestUIMode currentMode;

    [Header("Panel Root")]
    [SerializeField] private GameObject canvas;

    [Header("Header")]
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI subHeaderText;

    // ---- NEW GUN MODE ----
    [Header("New Gun Mode")]
    [SerializeField] private GameObject newGunPanel;
    [SerializeField] private TextMeshProUGUI newGunNameText;
    [SerializeField] private Button[] carryPerkButtons;         // one per compatible perk (max 4)
    [SerializeField] private TextMeshProUGUI[] carryPerkTexts;
    [SerializeField] private Image[] carryPerkIcons;
    [SerializeField] private Button randomPerkButton;           // shown if no compatible perks
    [SerializeField] private TextMeshProUGUI randomPerkText;
    [SerializeField] private Button keepCurrentWeaponButton;    // player declines swap

    // ---- MERGE PERK MODE ----
    [Header("Merge Perk Mode")]
    [SerializeField] private GameObject mergePerkPanel;
    [SerializeField] private Button[] chestPerkButtons;         // perks on the chest weapon
    [SerializeField] private TextMeshProUGUI[] chestPerkNameTexts;
    [SerializeField] private TextMeshProUGUI[] chestPerkDescTexts;
    [SerializeField] private Image[] chestPerkIcons;
    [SerializeField] private Button mergeDiscardButton;

    // ---- PERK SWAP MODE ----
    // Shown inside Merge Perk Mode after player picks a perk but has no slot
    [Header("Perk Swap Mode")]
    [SerializeField] private GameObject perkSwapPanel;
    [SerializeField] private TextMeshProUGUI incomingPerkName;
    [SerializeField] private TextMeshProUGUI incomingPerkDesc;
    [SerializeField] private Image incomingPerkIcon;
    [SerializeField] private Button[] currentPerkButtons;       // player's current 4 perks
    [SerializeField] private TextMeshProUGUI[] currentPerkNameTexts;
    [SerializeField] private TextMeshProUGUI[] currentPerkDescTexts;
    [SerializeField] private Image[] currentPerkIcons;
    [SerializeField] private Button swapDiscardButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
