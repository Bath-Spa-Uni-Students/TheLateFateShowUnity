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
    [SerializeField] private Button[] perkButtons;
    [SerializeField] private TextMeshProUGUI[] perkNameTexts;
    [SerializeField] private TextMeshProUGUI[] perkDescTexts;

    private List<PerkDefinition> currentSelection = new List<PerkDefinition>();


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P)) Show();
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
    //called from level up logic
    public void Show()
    {
        currentSelection = WeaponManager.Instance.GetRandomPerkSelection(3);
        Debug.Log($"[PerkSelectionUI] Got {currentSelection.Count} perks to display");
        foreach (var p in currentSelection) Debug.Log($"  - {p.perkName}");

        // Hide buttons if fewer than 3 valid perks remain
        for (int i = 0; i < perkButtons.Length; i++)
        {
            bool hasOption = i < currentSelection.Count;
            perkButtons[i].gameObject.SetActive(hasOption);
            if (hasOption)
            {
                perkNameTexts[i].text = currentSelection[i].perkName;
                perkDescTexts[i].text = currentSelection[i].description;
            }
        }

        canvas.SetActive(true);
        Time.timeScale = 0f;

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.perkSelect, Vector3.zero);
    }

    private void Hide()
    {
        canvas.SetActive(false);
        Time.timeScale = 1f;
    }

    private void OnPerkSelected(int index)
    {
        if (index >= currentSelection.Count) return;
        WeaponManager.Instance.ReceiveLevelUpPerk(currentSelection[index]);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.perkTriggerGeneric, Vector3.zero);
        Hide();
    }
}