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
    [SerializeField] private float switchInterval = 150f;
    [SerializeField] private bool debugMode = true;

    [Header("NavMesh")]
    [SerializeField] private NavMeshSurface navMeshSurface;

    [Header("Player")]
    [SerializeField] private Transform player;

    private GameObject[][] allSlots;
    private int[] currentActiveIndex;
    private Coroutine switchCoroutine;
    private bool isFirstGeneration = true;

    void OnEnable()
    {
        allSlots = new GameObject[][]
        {
            slot1, slot2, slot3, slot4,
            slot5, slot6, slot7, slot8, slot9
        };

        currentActiveIndex = new int[allSlots.Length];
        isFirstGeneration = true;
        GenerateAllSegments();
        switchCoroutine = StartCoroutine(RoomSwitchLoop());
    }

    private int GetPlayerSlotIndex()
    {
        if (player == null) return -1;

        for (int i = 0; i < allSlots.Length; i++)
        {
            GameObject[] slot = allSlots[i];
            if (slot == null) continue;

            foreach (GameObject room in slot)
            {
                // Only check the currently active room in this slot
                if (room == null || !room.activeInHierarchy) continue;

                Collider2D col = room.GetComponent<Collider2D>();
                if (col != null && col.OverlapPoint(player.position))
                    return i;
            }
        }

        return -1;
    }

    private void SelectSegment(GameObject[] segment, int slotIndex)
    {
        if (segment == null || segment.Length == 0) return;

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

        for (int i = 0; i < segment.Length; i++)
        {
            if (segment[i] != null)
                segment[i].SetActive(i == chosen);
        }

        currentActiveIndex[slotIndex] = chosen;
    }

    private void GenerateAllSegments()
    {
        int playerSlot = GetPlayerSlotIndex();

        ClearAllEnemies(playerSlot);

        for (int i = 0; i < allSlots.Length; i++)
        {
            if (i == playerSlot) continue;
            SelectSegment(allSlots[i], i);
        }

        if (SpawnManager.Instance != null)
            SpawnManager.Instance.ResetEnemyCount();

        RebakeNavMesh();
        ChestSpawner.NotifyMazeRegenerated(playerSlot);  // pass slot
        KeySpawner.NotifyMazeRegenerated(playerSlot);    // pass slot

        if (!isFirstGeneration)
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.mazeChange, Vector3.zero);

        isFirstGeneration = false;
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

    private void ClearAllEnemies(int playerSlot)
    {
        Collider2D playerRoomCollider = GetActiveRoomCollider(playerSlot);
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            // If we can't determine the player's slot, destroy all
            if (playerSlot < 0 || playerRoomCollider == null)
            {
                Destroy(enemy);
                continue;
            }

            // Keep enemies inside the player's current room
            if (!playerRoomCollider.OverlapPoint(enemy.transform.position))
                Destroy(enemy);
        }
    }

    private Collider2D GetActiveRoomCollider(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= allSlots.Length) return null;

        foreach (GameObject room in allSlots[slotIndex])
        {
            if (room != null && room.activeInHierarchy)
                return room.GetComponentInChildren<Collider2D>();
        }

        return null;
    }

    public void ForceSwitch()
    {
        if (switchCoroutine != null)
            StopCoroutine(switchCoroutine);

        GenerateAllSegments();
        switchCoroutine = StartCoroutine(RoomSwitchLoop());
    }

    public void StopSwitching()
    {
        if (switchCoroutine != null)
        {
            StopCoroutine(switchCoroutine);
            switchCoroutine = null;
        }
    }
}