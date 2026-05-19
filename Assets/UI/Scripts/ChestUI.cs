using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    [SerializeField] private Button[] carryPerkButtons;
    [SerializeField] private TextMeshProUGUI[] carryPerkDesc;
    [SerializeField] private Image[] carryPerkIcons;
    [SerializeField] private Button randomPerkButton;
    [SerializeField] private TextMeshProUGUI randomPerkText;
    [SerializeField] private Button keepCurrentWeaponButton;
    [SerializeField] private Image newGunIcon;
    [SerializeField] private Image[] chestWeaponPerkIcons;
    //[SerializeField] private TextMeshProUGUI[] chestWeaponPerkDesc;

    [Header("Weapon Icons")]
    [SerializeField] private Sprite pistolSprite;
    [SerializeField] private Sprite arSprite;
    [SerializeField] private Sprite shotgunSprite;

    // ---- MERGE PERK MODE ----
    [Header("Merge Perk Mode")]
    [SerializeField] private GameObject mergePerkPanel;
    [SerializeField] private Button[] chestPerkButtons;
    [SerializeField] private TextMeshProUGUI[] chestPerkNameTexts;
    [SerializeField] private TextMeshProUGUI[] chestPerkDescTexts;
    [SerializeField] private Image[] chestPerkIcons;
    [SerializeField] private Button mergeDiscardButton;

    // ---- PERK SWAP MODE ----
    [Header("Perk Swap Mode")]
    [SerializeField] private GameObject perkSwapPanel;
    [SerializeField] private TextMeshProUGUI incomingPerkName;
    [SerializeField] private TextMeshProUGUI incomingPerkDesc;
    [SerializeField] private Image incomingPerkIcon;
    [SerializeField] private Button[] currentPerkButtons;
    [SerializeField] private TextMeshProUGUI[] currentPerkNameTexts;
    [SerializeField] private TextMeshProUGUI[] currentPerkDescTexts;
    [SerializeField] private Image[] currentPerkIcons;
    [SerializeField] private Button swapDiscardButton;

    [Header("Fallback Icon")]
    [SerializeField] private Sprite fallbackIcon;

    [Header("Debug")]
    [SerializeField] private bool chestUIDebug = false;

    private List<PerkDefinition> cachedAvailablePerks = new List<PerkDefinition>();
    private WeaponInstance pendingNewWeapon;
    private PerkDefinition pendingMergePerk;

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
            AddHoverSound(carryPerkButtons[i]);
        }
        randomPerkButton.onClick.AddListener(OnRandomPerkChosen);
        AddHoverSound(randomPerkButton);

        keepCurrentWeaponButton.onClick.AddListener(OnKeepCurrentWeapon);
        AddHoverSound(keepCurrentWeaponButton);

        // Merge Perk Mode
        for (int i = 0; i < chestPerkButtons.Length; i++)
        {
            int index = i;
            chestPerkButtons[i].onClick.AddListener(() => OnChestPerkChosen(index));
            AddHoverSound(chestPerkButtons[i]);
        }
        mergeDiscardButton.onClick.AddListener(OnMergeDiscard);
        AddHoverSound(mergeDiscardButton);

        // Perk Swap Mode
        for (int i = 0; i < currentPerkButtons.Length; i++)
        {
            int index = i;
            currentPerkButtons[i].onClick.AddListener(() => OnSwapOutChosen(index));
            AddHoverSound(currentPerkButtons[i]);
        }
        swapDiscardButton.onClick.AddListener(OnSwapDiscard);
        AddHoverSound(swapDiscardButton);
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
        Debug.Log($"[ChestUI] HandleNewGun fired — weapon: {chestWeapon?.weaponType}, perks: {compatiblePerks?.Count}");

        currentMode = ChestUIMode.NewGun;
        pendingNewWeapon = chestWeapon;

        headerText.text = "New Weapon Found";
        subHeaderText.text = $"Switch to {chestWeapon.weaponType}?";
        newGunNameText.text = chestWeapon.weaponType.ToString();

        if (newGunIcon != null)
            newGunIcon.sprite = GetWeaponSprite(chestWeapon.weaponType);

        for (int i = 0; i < chestWeaponPerkIcons.Length; i++)
        {
            bool hasPerk = i < chestWeapon.perks.Count;
            if (chestWeaponPerkIcons[i] != null)
            {
                SetIcon(chestWeaponPerkIcons[i], hasPerk ? chestWeapon.perks[i].icon : null);
                chestWeaponPerkIcons[i].gameObject.SetActive(true);
            }
            //if (chestWeaponPerkDesc != null && i < chestWeaponPerkDesc.Length && chestWeaponPerkDesc[i] != null)
                //chestWeaponPerkDesc[i].text = hasPerk ? chestWeapon.perks[i].description : string.Empty;
        }

        bool hasCompatible = compatiblePerks.Count > 0;
        for (int i = 0; i < carryPerkButtons.Length; i++)
        {
            bool show = hasCompatible && i < compatiblePerks.Count;
            carryPerkButtons[i].gameObject.SetActive(show);
            if (show)
            {
                carryPerkDesc[i].text = compatiblePerks[i].description;
                SetIcon(carryPerkIcons[i], compatiblePerks[i].icon);
            }
        }

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
        if (index >= compatible.Count)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiError, Vector3.zero);
            return;
        }

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiConfirm, Vector3.zero);
        WeaponManager.Instance.ConfirmWeaponSwap(pendingNewWeapon, compatible[index]);

        if (chestUIDebug)
            Debug.Log($"[ChestUI] Swapped to {pendingNewWeapon.weaponType}, carried: {compatible[index].perkName}");

        Hide();
    }

    private void OnRandomPerkChosen()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiConfirm, Vector3.zero);
        WeaponManager.Instance.ConfirmWeaponSwap(pendingNewWeapon, null);

        if (chestUIDebug)
            Debug.Log($"[ChestUI] Swapped to {pendingNewWeapon.weaponType} with random perk");

        Hide();
    }

    private void OnKeepCurrentWeapon()
    {
        if (chestUIDebug)
            Debug.Log("[ChestUI] Player kept current weapon.");

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiBack, Vector3.zero);
        Hide();
    }

    // MERGE PERK MODE
    // -------------------------------------------------------

    private void HandleMergePerk(List<PerkDefinition> availablePerks)
    {
        Debug.Log($"[ChestUI] HandleMergePerk fired — perks: {availablePerks?.Count}");

        currentMode = ChestUIMode.MergePerk;
        cachedAvailablePerks = availablePerks;

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
        if (index >= cachedAvailablePerks.Count)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiError, Vector3.zero);
            return;
        }

        PerkDefinition chosen = cachedAvailablePerks[index];

        if (WeaponManager.Instance.CurrentWeapon.HasPerkSlot)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiConfirm, Vector3.zero);
            WeaponManager.Instance.ConfirmMergePerk(chosen);

            if (chestUIDebug)
                Debug.Log($"[ChestUI] Merged perk: {chosen.perkName}");

            Hide();
        }
        else
        {
            // No free slot — move to perk swap sub-mode (no confirm yet, player still deciding)
            ShowPerkSwapMode(chosen);
        }
    }

    private void OnMergeDiscard()
    {
        if (chestUIDebug)
            Debug.Log("[ChestUI] Player discarded chest perk.");

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiBack, Vector3.zero);
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
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiConfirm, Vector3.zero);
        WeaponManager.Instance.ConfirmMergePerk(pendingMergePerk, index);

        if (chestUIDebug)
            Debug.Log($"[ChestUI] Swapped out slot {index} for {pendingMergePerk.perkName}");

        Hide();
    }

    private void OnSwapDiscard()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiBack, Vector3.zero);
        WeaponManager.Instance.ConfirmMergePerk(pendingMergePerk, -1);

        if (chestUIDebug)
            Debug.Log("[ChestUI] Player discarded incoming perk in swap mode");

        Hide();
    }

    private void ShowPanel(GameObject panel)
    {
        newGunPanel.SetActive(panel == newGunPanel);
        mergePerkPanel.SetActive(panel == mergePerkPanel);
        perkSwapPanel.SetActive(false);
        canvas.SetActive(true);
        Time.timeScale = 0f;
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.uiPause, Vector3.zero);
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
        image.gameObject.SetActive(true);
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