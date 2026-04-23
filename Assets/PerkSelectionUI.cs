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

