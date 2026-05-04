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
    [Header("Fallback Icon")]
    [SerializeField] private Sprite fallbackIcon;

    [Header("Debug")]
    [SerializeField] private bool chestUIDebug = false;

    // Internal state
    private WeaponInstance pendingNewWeapon;
    private PerkDefinition pendingMergePerk;   // perk chosen in merge, waiting for swap decision

    private void Start()
    {
        canvas.SetActive(false);

        WeaponManager.Instance.OnChestNewGun += HandleNewGun;
        WeaponManager.Instance.OnChestMergePerk += HandleMergePerk;

        // New Gun Mode buttons
        for (int i = 0; i < carryPerkButtons.Length; i++)
        {
            int index = i;
            carryPerkButtons[i].onClick.AddListener(() => OnCarryPerkChosen(index));
        }
        randomPerkButton.onClick.AddListener(OnRandomPerkChosen);
        keepCurrentWeaponButton.onClick.AddListener(OnKeepCurrentWeapon);

       
    }


    // NEW GUN MODE
    // -------------------------------------------------------

    private void HandleNewGun(WeaponInstance chestWeapon, List<PerkDefinition> compatiblePerks)
    {
        currentMode = ChestUIMode.NewGun;
        pendingNewWeapon = chestWeapon;

        headerText.text = "New Weapon Found";
        subHeaderText.text = $"Switch to {chestWeapon.weaponType}?";
        newGunNameText.text = chestWeapon.weaponType.ToString();

        bool hasCompatible = compatiblePerks.Count > 0;

        // Show carry perk buttons
        for (int i = 0; i < carryPerkButtons.Length; i++)
        {
            bool show = hasCompatible && i < compatiblePerks.Count;
            carryPerkButtons[i].gameObject.SetActive(show);
            if (show)
            {
                carryPerkTexts[i].text = $"Carry over: {compatiblePerks[i].perkName}";
                SetIcon(carryPerkIcons[i], compatiblePerks[i].icon);
            }
        }

        // Show random perk button if no compatible perks
        randomPerkButton.gameObject.SetActive(!hasCompatible);
        if (!hasCompatible)
            randomPerkText.text = "No compatible perks - receive a random one";

        ShowPanel(newGunPanel);

        if (chestUIDebug)
            Debug.Log($"[ChestUI] New Gun Mode {chestWeapon.weaponType} | compatible perks: {compatiblePerks.Count}");
    }

    private void OnCarryPerkChosen(int index)
    {
        // Get the compatible perks list again from current weapon
        List<PerkDefinition> compatible = WeaponManager.Instance.CurrentWeapon.perks
            .FindAll(p => p.IsCompatibleWith(pendingNewWeapon.weaponType));

        if (index >= compatible.Count) return;

        WeaponManager.Instance.ConfirmWeaponSwap(pendingNewWeapon, compatible[index]);
        Hide();
    }

    private void OnRandomPerkChosen()
    {
        // WeaponManager will assign random after swap
        WeaponManager.Instance.ConfirmWeaponSwap(pendingNewWeapon, null);
        Hide();
    }

    private void OnKeepCurrentWeapon()
    {
        if (chestUIDebug)
            Debug.Log("[ChestUI] Player kept current weapon.");
        Hide();
    }

    // MERGE PERK MODE
    // -------------------------------------------------------

    private void HandleMergePerk(List<PerkDefinition> availablePerks)
    {
        currentMode = ChestUIMode.MergePerk;

        headerText.text = "Perk Found";
        subHeaderText.text = WeaponManager.Instance.CurrentWeapon.HasPerkSlot
            ? "Choose a perk to add"
            : "Choose a perk — you'll need to swap one out";

        for (int i = 0; i < chestPerkButtons.Length; i++)
        {
            bool show = i < availablePerks.Count;
            chestPerkButtons[i].gameObject.SetActive(show);
            if (show)
            {
                chestPerkNameTexts[i].text = availablePerks[i].perkName;
                chestPerkDescTexts[i].text = availablePerks[i].description;
                SetIcon(chestPerkIcons[i], availablePerks[i].icon);
            }
        }

        ShowPanel(mergePerkPanel);

        if (chestUIDebug)
            Debug.Log($"[ChestUI] Merge Perk Mode — {availablePerks.Count} perk(s) available");
    }


    private void OnMergeDiscard()
    {
        if (chestUIDebug)
            Debug.Log("[ChestUI] Player discarded chest perk.");
        Hide();
    }


    // PERK SWAP MODE 
    // -------------------------------------------------------

    private void ShowPerkSwapMode(PerkDefinition incoming)
    {
        currentMode = ChestUIMode.PerkSwap;
        pendingMergePerk = incoming;

        headerText.text = "Perk Slots Full";
        subHeaderText.text = "Replace a perk or discard";

        incomingPerkName.text = incoming.perkName;
        incomingPerkDesc.text = incoming.description;
        SetIcon(incomingPerkIcon, incoming.icon);

        List<PerkDefinition> current = WeaponManager.Instance.CurrentWeapon.perks;
        for (int i = 0; i < currentPerkButtons.Length; i++)
        {
            bool show = i < current.Count;
            currentPerkButtons[i].gameObject.SetActive(show);
            if (show)
            {
                currentPerkNameTexts[i].text = current[i].perkName;
                currentPerkDescTexts[i].text = current[i].description;
                SetIcon(currentPerkIcons[i], current[i].icon);
            }
        }

        mergePerkPanel.SetActive(false);
        perkSwapPanel.SetActive(true);

        if (chestUIDebug)
            Debug.Log($"[ChestUI] Perk Swap Mode — incoming: {incoming.perkName}");
    }

    private List<PerkDefinition> cachedAvailablePerks = new List<PerkDefinition>();

 
    private void ShowPanel(GameObject panel)
    {
        newGunPanel.SetActive(panel == newGunPanel);
        mergePerkPanel.SetActive(panel == mergePerkPanel);
        perkSwapPanel.SetActive(false); // only shown as sub-state of merge
        canvas.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Hide()
    {
        canvas.SetActive(false);
        newGunPanel.SetActive(false);
        mergePerkPanel.SetActive(false);
        perkSwapPanel.SetActive(false);
        Time.timeScale = 1f;
        pendingNewWeapon = null;
        pendingMergePerk = null;
    }

    private void SetIcon(Image image, Sprite icon)
    {
        if (image == null) return;
        image.sprite = icon != null ? icon : fallbackIcon;
        image.gameObject.SetActive(icon != null || fallbackIcon != null);
    }
}
