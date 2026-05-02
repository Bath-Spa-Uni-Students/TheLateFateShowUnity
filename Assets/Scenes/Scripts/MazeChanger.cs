using NavMeshPlus.Components;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class MazeChanger : MonoBehaviour
{
    [Header("Room Slots")]
    [SerializeField] private GameObject[] slot1;
    [SerializeField] private GameObject[] slot2;
    [SerializeField] private GameObject[] slot3;
    [SerializeField] private GameObject[] slot4;
    [SerializeField] private GameObject[] slot5;
    [SerializeField] private GameObject[] slot6;
    [SerializeField] private GameObject[] slot7;
    [SerializeField] private GameObject[] slot8;
    [SerializeField] private GameObject[] slot9;


    [Header("Settings")]
    [SerializeField] private float switchInterval = 150f;//adjust this value for balance currently set quite quick for testing
    [SerializeField] private bool debugMode = true; 

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    private GameObject[][] allSlots;
    private int[] currentActiveIndex; // tracks which room variation is active
    private Coroutine switchCoroutine;

    void Start()
    {
        allSlots = new GameObject[][]
        {
            slot1, slot2, slot3, slot4,
            slot5, slot6, slot7, slot8, slot9
        };

        currentActiveIndex = new int[allSlots.Length];

        GenerateAllSegments();
        switchCoroutine = StartCoroutine(RoomSwitchLoop());
    }

    private void SelectSegment(GameObject[] segment, int slotIndex)
    {
        if (segment == null || segment.Length == 0) return;

        // Pick a random variant that isn't the current one
        int chosen;
        if (segment.Length > 1)
        {
            do
            {
                chosen = Random.Range(0, segment.Length);
            }
            while (chosen == currentActiveIndex[slotIndex]);
        }
        else
        {
            chosen = 0;
        }

        // Swap active state
        for (int i = 0; i < segment.Length; i++)
        {
            if (segment[i] != null)
                segment[i].SetActive(i == chosen);
        }

        currentActiveIndex[slotIndex] = chosen;
    }

    private void GenerateAllSegments()
    {
        ClearAllEnemies();

        for (int i = 0; i < allSlots.Length; i++)
            SelectSegment(allSlots[i], i);

        // Reset count since all enemies were just destroyed
        if (SpawnManager.Instance != null)
            SpawnManager.Instance.ResetEnemyCount();

        RebakeNavMesh();
        ChestSpawner.NotifyMazeRegenerated();
    }

    private void RebakeNavMesh()
    {
        if (navMeshSurface != null)
            navMeshSurface.BuildNavMesh();
        else
            Debug.LogWarning("No NavMeshSurface assigned!");
    }

    IEnumerator RoomSwitchLoop()
    {
        float interval = debugMode ? 60f : switchInterval;

        while (true)
        {
            yield return new WaitForSeconds(interval);
            GenerateAllSegments();
            Debug.Log("Rooms switched at: " + Time.time);
        }
    }

    // Debug
    public void ForceSwitch()
    {
        if (switchCoroutine != null)
            StopCoroutine(switchCoroutine);

        GenerateAllSegments();
        switchCoroutine = StartCoroutine(RoomSwitchLoop());
    }

    // calls which segment is active 
    public int GetActiveVariant(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= currentActiveIndex.Length) return -1;
        return currentActiveIndex[slotIndex];
    }

    private void ClearAllEnemies()
    {
        // Find all enemies and destroy them before switching
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
            Destroy(enemy);
    }
}