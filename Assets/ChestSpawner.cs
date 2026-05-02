using System.Collections.Generic;
using UnityEngine;


// Attach one ChestSpawner per maze sector.
// Manages spawning 0 or 1 chest in this sector.
// Listens for maze regeneration and cleans up invalid chests.
public class ChestSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject chestPrefab;
    [SerializeField] private Transform[] spawnPoints;       // Valid navmesh positions for the sector
    [SerializeField][Range(0f, 1f)] private float spawnChance = 0.4f; // Chance this sector gets a chest

    [Header("Perk Pool (assign all PerkDefinitions)")]
    [SerializeField] private List<PerkDefinition> allPerks;

    [Header("Weapon Weights")]    // Lower = less common
    [SerializeField] private int pistolWeight = 1;       
    [SerializeField] private int arWeight = 3;
    [SerializeField] private int shotgunWeight = 3;

    // Static list so the maze regeneration event can notify all spawners easily
    private static List<ChestSpawner> allSpawners = new List<ChestSpawner>();

    private GameObject activeChest;

    private void OnEnable() => allSpawners.Add(this);
    private void OnDisable() => allSpawners.Remove(this);

    private void Awake()
    {
        allSpawners.Add(this);
        Debug.Log($"[ChestSpawner] Registered in Awake: {gameObject.name} (total: {allSpawners.Count})");
    }

    private void Start()
    {
       
    }

    // Call this from maze manager when the maze regenerates and nav mesh rebakes
    public static void NotifyMazeRegenerated()
    {
        foreach (ChestSpawner spawner in allSpawners)
            spawner.OnMazeRegenerated();
    }

    private void OnMazeRegenerated()
    {
        // Destroy existing chest if present
        if (activeChest != null)
        {
            Destroy(activeChest);
            activeChest = null;
        }

        // Chance to spawn a new chest in this sector
        TrySpawnChest();
    }

    private void TrySpawnChest()
    {
        Debug.Log($"[ChestSpawner] Attempting to spawn chest in sector {transform.parent.name}");// Validate spawn points and spawn chance
        if (spawnPoints == null || spawnPoints.Length == 0) return;// No valid spawn points assigned
        if (Random.value > spawnChance) return;// Failed spawn roll
        Debug.Log($"[ChestSpawner] Spawning chest in sector {transform.parent.name}");// Pick a random spawn point from the assigned list
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];// Spawn chest prefab at chosen spawn point
        activeChest = Instantiate(chestPrefab, spawnPoint.position, Quaternion.identity);// Assign generated contents to the chest

        ChestObject chest = activeChest.GetComponent<ChestObject>();
        if (chest != null)
            chest.contents = GenerateChestContents();
    }

    private WeaponInstance GenerateChestContents()
    {
        WeaponType weaponType = RollWeaponType();
        WeaponInstance weapon = new WeaponInstance(weaponType);

        // Add 1-2 random valid perks
        int perkCount = Random.Range(1, 3);
        List<PerkDefinition> validPerks = allPerks.FindAll(p => p.IsCompatibleWith(weaponType));// Filter perks to only those compatible with the rolled weapon type

        // Shuffle
        for (int i = validPerks.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            PerkDefinition tmp = validPerks[i];
            validPerks[i] = validPerks[j];
            validPerks[j] = tmp;
        }

        for (int i = 0; i < Mathf.Min(perkCount, validPerks.Count); i++)
            weapon.TryAddPerk(validPerks[i]);

        Debug.Log($"[ChestSpawner] Generated chest: {weaponType} with {weapon.perks.Count} perk(s)");
        return weapon;
    }

    private WeaponType RollWeaponType()
    {
        int total = pistolWeight + arWeight + shotgunWeight;
        int roll = Random.Range(0, total);

        if (roll < pistolWeight) return WeaponType.Pistol;
        if (roll < pistolWeight + arWeight) return WeaponType.AR;
        return WeaponType.Shotgun;
    }
}