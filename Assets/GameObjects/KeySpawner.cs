using System.Collections.Generic;
using UnityEngine;

public class KeySpawner : MonoBehaviour
{
    public static KeySpawner Instance { get; private set; }

    [Header("Key Prefab")]
    [SerializeField] private GameObject keyPrefab;

    [Header("Sector Roots")]
    [SerializeField] private GameObject[] sectors;

    [Header("Merge UI")]
    [Tooltip("Drag the KeyMergeUI GameObject here (can live on a canvas or in world space).")]
    [SerializeField] private KeyMergeUI keyMergeUI;

    // How many keys the player still needs to collect
    private int keysRemaining;
    private int totalKeys = 3;

    // Tracks the live key GameObjects currently in the world
    private List<GameObject> activeKeys = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // Called once from GameManager when the main game begins so KeySpawner
    // knows how many keys are needed
    public void Initialise(int keysRequired)
    {
        totalKeys = keysRequired;
        keysRemaining = keysRequired;
    }

    // Called by MazeChanger (same place ChestSpawner.NotifyMazeRegenerated is called)
    public static void NotifyMazeRegenerated()
    {
        if (Instance != null)
            Instance.OnMazeRegenerated();
        else
            Debug.LogWarning("[KeySpawner] No instance found!");
    }

    private void OnMazeRegenerated()
    {
        // Destroy any uncollected keys still floating in the world
        // they will be re-placed at fresh spawn points below
        ClearActiveKeys();

        if (keysRemaining <= 0) return; // All keys already collected, nothing to place

        PlaceKeys();
    }

    // Collect all KeySpawnPoints from every currently active room then randomly assign one point per remaining key
    private void PlaceKeys()
    {
        List<Transform> allPoints = GatherAllKeySpawnPoints();

        if (allPoints.Count == 0)
        {
            Debug.LogWarning("[KeySpawner] No KeySpawnPoints found in any active room!");
            return;
        }

        // Shuffle the list
        for (int i = allPoints.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (allPoints[i], allPoints[j]) = (allPoints[j], allPoints[i]);
        }

        int toPlace = Mathf.Min(keysRemaining, allPoints.Count);

        for (int i = 0; i < toPlace; i++)
        {
            GameObject keyObj = Instantiate(keyPrefab, allPoints[i].position, Quaternion.identity);

            KeyPickup pickup = keyObj.GetComponent<KeyPickup>();
            if (pickup != null)
                pickup.Init(this);
            else
                Debug.LogWarning("[KeySpawner] Key prefab is missing a KeyPickup component!");

            activeKeys.Add(keyObj);
        }

        Debug.Log($"[KeySpawner] Placed {toPlace} key(s). Keys remaining to collect: {keysRemaining}");
    }
    // Called by KeyPickup when the player walks over a key
    public void OnKeyCollected(GameObject keyObj)
    {
        if (activeKeys.Contains(keyObj))
            activeKeys.Remove(keyObj);

        Destroy(keyObj);

        keysRemaining--;
        int collectedSoFar = totalKeys - keysRemaining;

        Debug.Log($"[KeySpawner] Key collected! Remaining: {keysRemaining}/{totalKeys}");

        // Tell the merge UI to slot in the newly collected fragment
        if (keyMergeUI != null)
            keyMergeUI.OnFragmentCollected(collectedSoFar, totalKeys);

        // If all keys are now in — play the merge SFX then let GameManager know
        if (keysRemaining <= 0)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.keyMerge,
                Camera.main != null ? Camera.main.transform.position : Vector3.zero);
        }

        // Forward to GameManager which handles boss unlock and win condition
        GameManager.Instance.OnKeyCollected();
    }

    // Helpers
    private void ClearActiveKeys()
    {
        foreach (GameObject key in activeKeys)
            if (key != null) Destroy(key);
        activeKeys.Clear();
    }

    // Walk every sector find the single active room find all children tagged "KeySpawnPoint" inside that room.
    private List<Transform> GatherAllKeySpawnPoints()
    {
        List<Transform> points = new List<Transform>();

        foreach (GameObject sector in sectors)
        {
            if (sector == null) continue;

            GameObject activeRoom = GetActiveRoom(sector);
            if (activeRoom == null) continue;

            foreach (Transform child in activeRoom.transform)
                if (child.CompareTag("KeySpawnPoint"))
                    points.Add(child);
        }

        return points;
    }

    private GameObject GetActiveRoom(GameObject sector)
    {
        foreach (Transform child in sector.transform)
            if (child.gameObject.activeSelf) return child.gameObject;
        return null;
    }
}