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
    [SerializeField] private KeyMergeUI keyMergeUI;


    private int totalKeys = 3;
    private int keysRemaining = 3;

    private Dictionary<int, GameObject> activeKeys = new Dictionary<int, GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        OnMazeRegenerated(-1);
    }
    public void Initialise(int keysRequired)
    {
        totalKeys = keysRequired;
        keysRemaining = keysRequired;
    }

    public static void NotifyMazeRegenerated(int skipSectorIndex = -1)
    {
        if (Instance != null)
            Instance.OnMazeRegenerated(skipSectorIndex);
        else
            Debug.LogWarning("[KeySpawner] No instance found!");
    }


    private void OnMazeRegenerated(int skipSectorIndex)
    {
        ClearActiveKeys(skipSectorIndex);
        if (keysRemaining <= 0) return;
        PlaceKeys(skipSectorIndex);
    }

    private void PlaceKeys(int skipSectorIndex)
    {
        List<Transform> allPoints = GatherAllKeySpawnPoints(skipSectorIndex);

        if (allPoints.Count == 0)
        {
            Debug.LogWarning("[KeySpawner] No KeySpawnPoints found in any active room!");
            return;
        }

        for (int i = allPoints.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (allPoints[i], allPoints[j]) = (allPoints[j], allPoints[i]);
        }

        int alreadyCollected = totalKeys - keysRemaining;
        int pointIdx = 0;

        for (int keyIdx = alreadyCollected; keyIdx < totalKeys; keyIdx++)
        {
            if (activeKeys.ContainsKey(keyIdx)) continue;  // already exists in player's room

            if (pointIdx >= allPoints.Count)
            {
                Debug.LogWarning("[KeySpawner] Ran out of spawn points!");
                break;
            }

            GameObject keyObj = Instantiate(keyPrefab, allPoints[pointIdx].position, Quaternion.identity);
            pointIdx++;

            KeyPickup pickup = keyObj.GetComponent<KeyPickup>();
            if (pickup != null)
                pickup.Init(this, keyIdx);

            activeKeys[keyIdx] = keyObj;
        }
    }

    public void OnKeyCollected(GameObject keyObj, int keyIndex)
    {
        if (activeKeys.ContainsKey(keyIndex))
            activeKeys.Remove(keyIndex);

        Destroy(keyObj);
        keysRemaining--;

        Debug.Log($"[KeySpawner] Key {keyIndex} collected. Remaining: {keysRemaining}/{totalKeys}");

        if (keyMergeUI != null)
            keyMergeUI.OnFragmentCollected(keyIndex);

        if (keysRemaining <= 0)
        {
            AudioManager.Instance.PlayOneShot(
                FMODEvents.Instance.keyMerge,
                Camera.main != null ? Camera.main.transform.position : Vector3.zero);

            if (keyMergeUI != null)
                keyMergeUI.PlayMergeSequence();
        }

        GameManager.Instance.OnKeyCollected();
    }

    private void ClearActiveKeys(int skipSectorIndex)
    {
        List<int> toRemove = new List<int>();

        foreach (var kvp in activeKeys)
        {
            if (kvp.Value == null) { toRemove.Add(kvp.Key); continue; }

            bool inPlayerSector = skipSectorIndex >= 0
                && skipSectorIndex < sectors.Length
                && IsInsideSector(kvp.Value, sectors[skipSectorIndex]);

            if (!inPlayerSector)
            {
                Destroy(kvp.Value);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (int k in toRemove)
            activeKeys.Remove(k);
    }

    private List<Transform> GatherAllKeySpawnPoints(int skipSectorIndex)
    {
        List<Transform> points = new List<Transform>();

        for (int i = 0; i < sectors.Length; i++)
        {
            if (i == skipSectorIndex) continue;  // skip player's sector

            GameObject sector = sectors[i];
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

    private bool IsInsideSector(GameObject obj, GameObject sector)
    {
        GameObject activeRoom = GetActiveRoom(sector);
        if (activeRoom == null) return false;

        Collider2D col = activeRoom.GetComponentInChildren<Collider2D>();
        if (col == null) return false;

        return col.OverlapPoint(obj.transform.position);
    }
}