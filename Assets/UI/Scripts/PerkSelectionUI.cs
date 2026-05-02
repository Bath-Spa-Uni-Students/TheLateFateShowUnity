using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PerkSelectionUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject canvas;
    [SerializeField] private Button[] perkButtons; // assign all 3 in Inspector
    [SerializeField] private TextMeshProUGUI[] perkNameTexts; // one per button
    [SerializeField] private TextMeshProUGUI[] perkDescTexts; // one per button
    [SerializeField] private PistolPerks pistolPerks;

    // Full perk pool
    private PerkOption[] allPerks = new PerkOption[]
    {
        new PerkOption("Crit Chance",       "Adds a chance to deal double damage"),
        new PerkOption("Poison Rounds",     "Bullets apply a damage over time effect"),
        new PerkOption("Slow Rounds",       "Bullets slow enemies on hit"),
        new PerkOption("Power Cell",        "Increases gun damage"),
        new PerkOption("Speed Cell",        "Chance to temporarily double fire rate"),
        new PerkOption("Shockwave Loader",  "Chance to knock back enemies on hit"),
        new PerkOption("Scatter",           "Fires a spread of bullets on hit"),
        new PerkOption("Thorns",            "Reflect melee damage back to attacker"),
        new PerkOption("Pierce",            "Bullets pass through enemies"),
        new PerkOption("Ricochet",          "Bullets bounce off walls"),
        new PerkOption("Hit Reload",        "Chance to restore ammo on hit"),
        new PerkOption("Life Steal",        "Heal a portion of damage dealt"),
    };

    private PerkOption[] currentSelection = new PerkOption[3];

    private void Start()
    {
        canvas.SetActive(false);

        // Adds listeners to buttons
        for (int i = 0; i < perkButtons.Length; i++)
        {
            int index = i; // capture for lambda
            perkButtons[i].onClick.AddListener(() => OnPerkSelected(index));
        }
    }

    // Show perk screen 
    public void Show()
    {
        currentSelection = GetRandomPerks(3);

        for (int i = 0; i < perkButtons.Length; i++)
        {
            perkNameTexts[i].text = currentSelection[i].name;
            perkDescTexts[i].text = currentSelection[i].description;
        }

        // Freeze game
        canvas.SetActive(true);
        Time.timeScale = 0f;

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.perkSelect, Vector3.zero);
    }

    // Hide perk screen
    private void Hide()
    {
        canvas.SetActive(false);
        Time.timeScale = 1f;
    }

    // When a perk is selected apply it and close the screen
    private void OnPerkSelected(int index)
    {
        //ApplyPerk(currentSelection[index].name);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.perkTriggerGeneric, Vector3.zero);
        Hide();
    }

    // Apply perk to pistol
  /*  private void ApplyPerk(string perkName)
    {
        switch (perkName)
        {
            case "Crit Chance": pistolPerks.critChance = true; break;
            case "Poison Rounds": pistolPerks.poisonRounds = true; break;
            case "Slow Rounds": pistolPerks.slowRounds = true; break;
            case "Power Cell": pistolPerks.powerCell = true; break;
            case "Speed Cell": pistolPerks.speedCell = true; break;
            case "Shockwave Loader": pistolPerks.shockwaveLoader = true; break;
            case "Scatter": pistolPerks.scatter = true; break;
            case "Thorns": pistolPerks.thorns = true; break;
            case "Pierce": pistolPerks.pierce = true; break;
            case "Ricochet": pistolPerks.ricochet = true; break;
            case "Hit Reload": pistolPerks.hitReload = true; break;
            case "Life Steal": pistolPerks.lifeSteal = true; break;
        }
    }*/

    // Get random perks from the pool
    private PerkOption[] GetRandomPerks(int count)
    {
        PerkOption[] pool = (PerkOption[])allPerks.Clone();

        // Shuffles perk pool
        for (int i = pool.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            PerkOption temp = pool[i];
            pool[i] = pool[j];
            pool[j] = temp;
        }

        PerkOption[] selection = new PerkOption[count];
        for (int i = 0; i < count; i++)
            selection[i] = pool[i];

        return selection;
    }
}

// Hold perk data
public class PerkOption
{
    public string name;
    public string description;

    public PerkOption(string name, string description)
    {
        this.name = name;
        this.description = description;
    }
}