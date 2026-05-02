using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PerkDisplayUI : MonoBehaviour
{
    [Header("Perk Icon Slots (assign 4 UI Images)")]
    [SerializeField] private Image[] perkSlots; // size 4

    [Header("Empty Slot Appearance")]
    [SerializeField] private Sprite emptySlotSprite; // greyed-out placeholder
    [SerializeField] private Color emptySlotColor = new Color(1f, 1f, 1f, 0.2f);
    [SerializeField] private Color filledSlotColor = new Color(1f, 1f, 1f, 1f);

    private void Start()
    {
        WeaponManager.Instance.OnPerksChanged += RefreshDisplay;
        WeaponManager.Instance.OnWeaponChanged += _ => RefreshDisplay(WeaponManager.Instance.CurrentWeapon);

        // Initialise to empty
        ClearAll();
    }

    private void OnDestroy()
    {
        if (WeaponManager.Instance != null)
        {
            WeaponManager.Instance.OnPerksChanged -= RefreshDisplay;
        }
    }

    private void RefreshDisplay(WeaponInstance weapon)
    {
        for (int i = 0; i < perkSlots.Length; i++)
        {
            if (i < weapon.perks.Count)
            {
                // Slot has a perk
                PerkDefinition perk = weapon.perks[i];
                perkSlots[i].sprite = perk.icon != null ? perk.icon : emptySlotSprite;
                perkSlots[i].color = filledSlotColor;
                perkSlots[i].gameObject.SetActive(true);
            }
            else
            {
                // Empty slot
                perkSlots[i].sprite = emptySlotSprite;
                perkSlots[i].color = emptySlotColor;
                perkSlots[i].gameObject.SetActive(emptySlotSprite != null);
            }
        }
    }

    private void ClearAll()
    {
        foreach (var slot in perkSlots)
        {
            slot.sprite = emptySlotSprite;
            slot.color = emptySlotColor;
            slot.gameObject.SetActive(emptySlotSprite != null);
        }
    }
}