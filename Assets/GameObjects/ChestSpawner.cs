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
    private void OnDestroy() => allSpawners.Remove(this);

    private void Awake()
    {
        if (!allSpawners.Contains(this))
        {
            Debug.Log($"[ChestSpawner] Registered: {gameObject.name} (total: {allSpawners.Count})");
        }
    }

 

    private void Start()
    {
       
    }

    // Call this from maze manager when the maze regenerates and nav mesh rebakes
    public static void NotifyMazeRegenerated()
    {
        Debug.Log($"[ChestSpawner] NotifyMazeRegenerated — {allSpawners.Count} spawner(s) registered");

        // Iterate a copy in case the list changes mid-loop
        foreach (ChestSpawner spawner in new List<ChestSpawner>(allSpawners))
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
        //  prefab must be assigned
        if (chestPrefab == null)
        {
            Debug.LogError($"[ChestSpawner] {gameObject.name} — chestPrefab is null! Assign it in the Inspector.");
            return;
        }

        // spawn points must be assigned
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning($"[ChestSpawner] {gameObject.name} — no spawn points assigned, skipping.");
            return;
        }

        //  perk list must have entries
        if (allPerks == null || allPerks.Count == 0)
        {
            Debug.LogWarning($"[ChestSpawner] {gameObject.name} — allPerks is empty! Assign PerkDefinitions in the Inspector.");
            return;
        }

        // Spawn chance roll
        if (Random.value > spawnChance)
        {
            Debug.Log($"[ChestSpawner] {gameObject.name} — spawn roll failed, no chest this cycle.");
            return;
        }

        // Filter out any null spawn points
        List<Transform> validPoints = new List<Transform>();
        foreach (Transform t in spawnPoints)
            if (t != null) validPoints.Add(t);

        if (validPoints.Count == 0)
        {
            Debug.LogWarning($"[ChestSpawner] {gameObject.name} — all assigned spawn points are null!");
            return;
        }

        Transform chosen = validPoints[Random.Range(0, validPoints.Count)];
        activeChest = Instantiate(chestPrefab, chosen.position, Quaternion.identity);

        ChestObject chest = activeChest.GetComponent<ChestObject>();
        if (chest != null)
        {
            chest.contents = GenerateChestContents();
            Debug.Log($"[ChestSpawner] {gameObject.name} — spawned {chest.contents.weaponType} chest with {chest.contents.perks.Count} perk(s) at {chosen.name}");
        }
        else
        {
            Debug.LogWarning($"[ChestSpawner] {gameObject.name} — spawned chest prefab has no ChestObject component!");
        }


        Debug.Log($"[ChestSpawner] Before assign — chest has {chest.contents?.perks?.Count ?? -1} perks");
        chest.contents = GenerateChestContents();
        Debug.Log($"[ChestSpawner] After assign — chest has {chest.contents.perks.Count} perks");
    }

    private WeaponInstance GenerateChestContents()
    {
        WeaponType weaponType = RollWeaponType();
        WeaponInstance weapon = new WeaponInstance(weaponType);

        int perkCount = Random.Range(1, 3);
        List<PerkDefinition> validPerks = allPerks.FindAll(p => p != null && p.IsCompatibleWith(weaponType));

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