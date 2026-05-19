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
        // Always do an initial chest spawn when the scene load
        OnMazeRegenerated(-1); // -1 = no sector to skip on first load
    }

    public static void NotifyMazeRegenerated(int skipSectorIndex = -1)
    {
        if (Instance != null)
            Instance.OnMazeRegenerated(skipSectorIndex);
        else
            Debug.LogWarning("[ChestSpawner] No instance found!");
    }

    private void OnMazeRegenerated(int skipSectorIndex)
    {
        // Only destroy chests outside the player's sector
        List<GameObject> chestsToRemove = new List<GameObject>();
        foreach (GameObject chest in activeChests)
        {
            if (chest == null) { chestsToRemove.Add(chest); continue; }

            bool inPlayerSector = skipSectorIndex >= 0
                && skipSectorIndex < sectors.Length
                && IsInsideSector(chest, sectors[skipSectorIndex]);

            if (!inPlayerSector)
            {
                Destroy(chest);
                chestsToRemove.Add(chest);
            }
        }
        foreach (GameObject c in chestsToRemove)
            activeChests.Remove(c);

        // Spawn chests, skipping the player's sector
        for (int i = 0; i < sectors.Length; i++)
        {
            if (i == skipSectorIndex) continue;  // leave player's sector alone

            GameObject sector = sectors[i];
            if (sector == null) continue;

            GameObject activeRoom = GetActiveRoom(sector);
            if (activeRoom == null) continue;

            List<Transform> spawnPoints = GetSpawnPoints(activeRoom);
            if (spawnPoints.Count == 0) continue;

            if (Random.value > spawnChance) continue;

            Transform chosen = spawnPoints[Random.Range(0, spawnPoints.Count)];
            GameObject activeChest = Instantiate(chestPrefab, chosen.position, Quaternion.identity);
            ChestObject chest = activeChest.GetComponent<ChestObject>();

            if (chest != null)
                chest.contents = GenerateChestContents();

            activeChests.Add(activeChest);
        }
    }

    // Checks if a GameObject sits within a sector's active room collider
    private bool IsInsideSector(GameObject obj, GameObject sector)
    {
        GameObject activeRoom = GetActiveRoom(sector);
        if (activeRoom == null) return false;

        Collider2D col = activeRoom.GetComponentInChildren<Collider2D>();
        if (col == null) return false;

        return col.OverlapPoint(obj.transform.position);
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