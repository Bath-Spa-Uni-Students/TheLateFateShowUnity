using FMOD.Studio;
using System.Collections;
using UnityEngine;


// Chests are Placed in the maze they Contain a weapons with 1-2 pre-attached perks
// When the player interacts hands the WeaponInstance to WeaponManager.

public class ChestObject : MonoBehaviour
{
    [Header("Contents")]
    public WeaponInstance contents;

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 1.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactPrompt; // Optional "Press E" UI

    [Header("Hum Settings")]
    [SerializeField] private float humIntervalMin = 5f;
    [SerializeField] private float humIntervalMax = 12f;
    [SerializeField] private float humMaxDistance = 15f;

    private bool playerInRange = false;
    private bool opened = false;
    private EventInstance humInstance;
    private Coroutine humCoroutine;
    private void Awake()
    { 
        // ChestSpawner assigns contents after Instantiate.
        contents = null;
    }
    private void Start()
    {
        humInstance = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.chestHum);
        humInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
        humCoroutine = StartCoroutine(HumLoop());
    }

    private void Update()
    {
        if (opened) return;

        // Keep FMOD position in sync
        humInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));

        if (playerInRange && Input.GetKeyDown(interactKey))
            Open();
    }
    private void OnDestroy()
    {
        // Called when ChestSpawner destroys stale chests on maze regen this ensures the hum no ghost 
        StopHum();
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (opened) return;
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            if (interactPrompt) interactPrompt.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            if (interactPrompt) interactPrompt.SetActive(false);
        }
    }

    private void Open()
    {
        if (contents == null)
        {
            Debug.LogWarning("[ChestObject] Chest has no contents assigned!");
            return;
        }

        opened = true;
        StopHum();

        if (interactPrompt) interactPrompt.SetActive(false);

        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.chestOpen, transform.position);

        Debug.Log($"[ChestObject] Opened chest: {contents.weaponType} with {contents.perks.Count} perk(s)");

        WeaponManager.Instance.ReceiveChestWeapon(contents);

        // Destroy after a short delay to allow open animation to finish.
        Destroy(gameObject, 0.5f);
    }

    private IEnumerator HumLoop()
    {
        while (!opened)
        {
            yield return new WaitForSeconds(Random.Range(humIntervalMin, humIntervalMax));
            if (opened) yield break;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) continue;

            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist > humMaxDistance) continue;

            humInstance.stop(STOP_MODE.ALLOWFADEOUT);
            humInstance.start();
        }
    }
    private void StopHum()
    {
        if (humCoroutine != null)
        {
            StopCoroutine(humCoroutine);
            humCoroutine = null;
        }

        humInstance.stop(STOP_MODE.IMMEDIATE);
        humInstance.release();
    }
    // Visualise interact radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}