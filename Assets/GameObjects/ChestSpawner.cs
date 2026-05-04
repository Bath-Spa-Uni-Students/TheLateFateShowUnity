using System.Collections.Generic;
using UnityEngine;


// Attach one ChestSpawner per maze sector.
// Manages spawning 0 or 1 chest in this sector.
// Listens for maze regeneration and cleans up invalid chests.
public class ChestSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject chestPrefab;
    [SerializeField][Range(0f, 1f)] private float spawnChance = 0.4f; // Chance this sector gets a chest

    [Header("Perk Pool (assign all PerkDefinitions)")]
    [SerializeField] private List<PerkDefinition> allPerks;

    [Header("Weapon Weights")]    // Lower = less common
    [SerializeField] private int pistolWeight = 1;       
    [SerializeField] private int arWeight = 3;
    [SerializeField] private int shotgunWeight = 3;

    [Header("Sector Roots (assign all 9 sector GameObjects)")]
    [SerializeField] private GameObject[] sectors;

    private List<GameObject> activeChests = new List<GameObject>();

    public static ChestSpawner Instance { get; private set; }


    private void Awake()
    {
        Instance = this;// Register this spawner in the static list
    }

    private void Start()
    {
       
    }

    // Call this from maze manager when the maze regenerates and nav mesh rebakes
    public static void NotifyMazeRegenerated()
    {
        if (Instance != null)
            Instance.OnMazeRegenerated();
        else
            Debug.LogWarning("[ChestSpawner] No instance found!");
    }

    private void OnMazeRegenerated()
    {
        // Clear existing chests
        foreach (GameObject chest in activeChests)
            if (chest != null) Destroy(chest);
        activeChests.Clear();

        // Try to spawn one chest per sector
        foreach (GameObject sector in sectors)
        {
            if (sector == null) continue;

            // Find the active room in this sector
            GameObject activeRoom = GetActiveRoom(sector);
            if (activeRoom == null)
            {
                Debug.LogWarning($"[ChestSpawner] No active room found in sector: {sector.name}");
                continue;
            }

            // Collect spawn points from the active room's children
            List<Transform> spawnPoints = GetSpawnPoints(activeRoom);
            if (spawnPoints.Count == 0)
            {
                Debug.LogWarning($"[ChestSpawner] No spawn points found in room: {activeRoom.name}");
                continue;
            }

            // Roll spawn chance
            if (Random.value > spawnChance) continue;

            // Pick a random spawn point
            Transform chosen = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject activeChest = Instantiate(chestPrefab, chosen.position, Quaternion.identity);
            ChestObject chest = activeChest.GetComponent<ChestObject>();

            if (chest != null)
            {
                chest.contents = GenerateChestContents();
                Debug.Log($"[ChestSpawner] Spawned {chest.contents.weaponType} chest in {activeRoom.name} at {chosen.name}");
            }

            activeChests.Add(activeChest);
        }

        Debug.Log($"[ChestSpawner] Spawned {activeChests.Count} chest(s) across {sectors.Length} sector(s)");
    }

    private GameObject GetActiveRoom(GameObject sector)
    {
        foreach (Transform child in sector.transform)// Active room should be the only active child of the sector
            if (child.gameObject.activeSelf) return child.gameObject;
        return null;
    }

    private List<Transform> GetSpawnPoints(GameObject room)
    {
        List<Transform> points = new List<Transform>();
        foreach (Transform child in room.transform)
            if (child.CompareTag("ChestSpawnPoint"))
                points.Add(child);
        return points;
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