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
    public void Initialise(int keysRequired)
    {
        totalKeys = keysRequired;
        keysRemaining = keysRequired;
    }

    public static void NotifyMazeRegenerated()
    {
        if (Instance != null)
            Instance.OnMazeRegenerated();
        else
            Debug.LogWarning("[KeySpawner] No instance found!");
    }


    private void OnMazeRegenerated()
    {
        ClearActiveKeys();
        if (keysRemaining <= 0) return;
        PlaceKeys();
    }


    private void PlaceKeys()
    {
        List<Transform> allPoints = GatherAllKeySpawnPoints();

        if (allPoints.Count == 0)
        {
            Debug.LogWarning("[KeySpawner] No KeySpawnPoints found in any active room!");
            return;
        }

        // Shuffle list
        for (int i = allPoints.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (allPoints[i], allPoints[j]) = (allPoints[j], allPoints[i]);
        }

        int alreadyCollected = totalKeys - keysRemaining;
        int pointIdx = 0;

        for (int keyIdx = alreadyCollected; keyIdx < totalKeys; keyIdx++)
        {
            if (pointIdx >= allPoints.Count)
            {
                Debug.LogWarning("[KeySpawner] Ran out of spawn points!");
                break;
            }

            GameObject keyObj = Instantiate(keyPrefab, allPoints[pointIdx].position, Quaternion.identity);
            pointIdx++;

            KeyPickup pickup = keyObj.GetComponent<KeyPickup>();
            if (pickup != null)
                pickup.Init(this, keyIdx);          // <-- pass the stable index
            else
                Debug.LogWarning("[KeySpawner] Key prefab is missing a KeyPickup component!");

            activeKeys[keyIdx] = keyObj;
        }

        Debug.Log($"[KeySpawner] Placed {activeKeys.Count} key(s). Keys remaining: {keysRemaining}/{totalKeys}");
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

    private void ClearActiveKeys()
    {
        foreach (var kvp in activeKeys)
            if (kvp.Value != null) Destroy(kvp.Value);
        activeKeys.Clear();
    }

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