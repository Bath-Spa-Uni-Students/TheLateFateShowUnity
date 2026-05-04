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
    [SerializeField] private Image newGunIcon;                  // weapon sprite
    [SerializeField] private Image[] chestWeaponPerkIcons;      // 1-2 perk icons on the chest weapon
    [SerializeField] private TextMeshProUGUI[] chestWeaponPerkDesc; 

    [Header("Weapon Icons")]
    [SerializeField] private Sprite pistolSprite;
    [SerializeField] private Sprite arSprite;
    [SerializeField] private Sprite shotgunSprite;

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

    [Header("Fallback Icon")]
    [SerializeField] private Sprite fallbackIcon;

    [Header("Debug")]
    [SerializeField] private bool chestUIDebug = false;

    private List<PerkDefinition> cachedAvailablePerks = new List<PerkDefinition>();

    // Internal state
    private WeaponInstance pendingNewWeapon;
    private PerkDefinition pendingMergePerk;   // perk chosen in merge, waiting for swap decision

    private void Start()
    {
        canvas.SetActive(false);

        WeaponManager.Instance.OnChestNewGun += HandleNewGun;
        WeaponManager.Instance.OnChestMergePerk += HandleMergePerk;

        // New Gun Mode
        for (int i = 0; i < carryPerkButtons.Length; i++)
        {
            int index = i;
            carryPerkButtons[i].onClick.AddListener(() => OnCarryPerkChosen(index));
        }
        randomPerkButton.onClick.AddListener(OnRandomPerkChosen);
        keepCurrentWeaponButton.onClick.AddListener(OnKeepCurrentWeapon);

        // Merge Perk Mode
        for (int i = 0; i < chestPerkButtons.Length; i++)
        {
            int index = i;
            chestPerkButtons[i].onClick.AddListener(() => OnChestPerkChosen(index));
        }
        mergeDiscardButton.onClick.AddListener(OnMergeDiscard);

        // Perk Swap Mode
        for (int i = 0; i < currentPerkButtons.Length; i++)
        {
            int index = i;
            currentPerkButtons[i].onClick.AddListener(() => OnSwapOutChosen(index));
        }
        swapDiscardButton.onClick.AddListener(OnSwapDiscard);
    }

    private void OnDestroy()
    {
        if (WeaponManager.Instance == null) return;
        WeaponManager.Instance.OnChestNewGun -= HandleNewGun;
        WeaponManager.Instance.OnChestMergePerk -= HandleMergePerk;
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

        // Show weapon icon
        if (newGunIcon != null)
            newGunIcon.sprite = GetWeaponSprite(chestWeapon.weaponType);

        // Show the perks already on the chest weapon
        for (int i = 0; i < chestWeaponPerkIcons.Length; i++)
        {
            bool hasPerk = i < chestWeapon.perks.Count;
            if (chestWeaponPerkIcons[i] != null)
            {
                SetIcon(chestWeaponPerkIcons[i], hasPerk ? chestWeapon.perks[i].icon : null);
                chestWeaponPerkIcons[i].gameObject.SetActive(true);
            }
            if (chestWeaponPerkDesc != null && i < chestWeaponPerkDesc.Length && chestWeaponPerkDesc[i] != null)
                chestWeaponPerkDesc[i].text = hasPerk ? chestWeapon.perks[i].description : string.Empty;
        }

        // Show carry over perk buttons
        bool hasCompatible = compatiblePerks.Count > 0;
        for (int i = 0; i < carryPerkButtons.Length; i++)
        {
            bool show = hasCompatible && i < compatiblePerks.Count;
            carryPerkButtons[i].gameObject.SetActive(show);
            if (show)
            {
                carryPerkTexts[i].text = compatiblePerks[i].perkName;
                SetIcon(carryPerkIcons[i], compatiblePerks[i].icon);
            }
        }

        // Show random button if no compatible perks
        randomPerkButton.gameObject.SetActive(!hasCompatible);
        if (!hasCompatible)
            randomPerkText.text = "Receive a random perk";

        ShowPanel(newGunPanel);
    }

    private Sprite GetWeaponSprite(WeaponType type)
    {
        return type switch
        {
            WeaponType.Pistol => pistolSprite,
            WeaponType.AR => arSprite,
            WeaponType.Shotgun => shotgunSprite,
            _ => null
        };
    }
    private void OnCarryPerkChosen(int index)
    {
        List<PerkDefinition> compatible = WeaponManager.Instance.CurrentWeapon.perks.FindAll(p => p.IsCompatibleWith(pendingNewWeapon.weaponType));
        if (index >= compatible.Count) return;

        WeaponManager.Instance.ConfirmWeaponSwap(pendingNewWeapon, compatible[index]);

        if (chestUIDebug)
            Debug.Log($"[ChestUI] Swapped to {pendingNewWeapon.weaponType}, carried: {compatible[index].perkName}");

        Hide();
    }

    private void OnRandomPerkChosen()
    {
        // WeaponManager will assign random after swap
        WeaponManager.Instance.ConfirmWeaponSwap(pendingNewWeapon, null);

        if (chestUIDebug)
            Debug.Log($"[ChestUI] Swapped to {pendingNewWeapon.weaponType} with random perk");
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

    private void OnChestPerkChosen(int index)
    {
        if (index >= cachedAvailablePerks.Count) return;
        PerkDefinition chosen = cachedAvailablePerks[index];

        if (WeaponManager.Instance.CurrentWeapon.HasPerkSlot)
        {
            WeaponManager.Instance.ConfirmMergePerk(chosen);

            if (chestUIDebug)
                Debug.Log($"[ChestUI] Merged perk: {chosen.perkName}");

            Hide();
        }
        else
        {
            // No free slot move to perk swap mode
            ShowPerkSwapMode(chosen);
        }
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

    
    private void OnSwapOutChosen(int index)
    {
        // Index corresponds to the perk slot the player wants to replace with the incoming perk
        WeaponManager.Instance.ConfirmMergePerk(pendingMergePerk, index);

        if (chestUIDebug)
            Debug.Log($"[ChestUI] Swapped out slot {index} for {pendingMergePerk.perkName}");

        Hide();
    }

    private void OnSwapDiscard()
    {
        // Player chooses to discard the incoming perk instead of swapping
        WeaponManager.Instance.ConfirmMergePerk(pendingMergePerk, -1);

        if (chestUIDebug)
            Debug.Log("[ChestUI] Player discarded incoming perk in swap mode");

        Hide();
    }

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
        Debug.Log($"[ChestUI] SetIcon — image null: {image == null} | icon null: {icon == null} | fallback null: {fallbackIcon == null}");

        if (image == null) return;
        image.sprite = icon != null ? icon : fallbackIcon;
        image.gameObject.SetActive(true); // always visible
    }
}
