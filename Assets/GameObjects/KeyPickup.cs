using System.Collections;
using UnityEngine;
using FMOD.Studio;

public class KeyPickup : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Sprite[] keySprites;

    private KeySpawner spawner;
    private int keyIndex = 0; 
    private bool collected = false;
    private Coroutine humCoroutine;
    private EventInstance humInstance;

    [Header("Hum Settings")]
    [SerializeField] private float humIntervalMin = 4f;
    [SerializeField] private float humIntervalMax = 9f;
    [SerializeField] private float humMaxDistance = 20f;

    public void Init(KeySpawner owner, int index)
    {
        spawner = owner;
        keyIndex = index;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && keySprites != null && index < keySprites.Length)
            sr.sprite = keySprites[index];
        else if (keySprites == null || keySprites.Length == 0)
            Debug.LogWarning("[KeyPickup] No keySprites assigned on the prefab!");
        else if (index >= keySprites.Length)
            Debug.LogWarning($"[KeyPickup] keyIndex {index} out of range — only {keySprites.Length} sprite(s) assigned.");
    }

    private void Start()
    {
        humInstance = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.keyHum);
        humInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));

        humCoroutine = StartCoroutine(HumLoop());
    }
    private void Update()
    {
        //Fmod keep track of the key when maze changes
        if (!collected)
            humInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(transform.position));
    }

    private void OnDestroy()
    {
        StopHum();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        StopHum();
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.keyPickup, transform.position);
        spawner.OnKeyCollected(gameObject, keyIndex);
    }

    private IEnumerator HumLoop()
    {
        while (!collected)
        {
            yield return new WaitForSeconds(Random.Range(humIntervalMin, humIntervalMax));
            if (collected) yield break;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null) continue;

            float dist = Vector2.Distance(transform.position, player.transform.position);
            if (dist > humMaxDistance) continue;

            // Retrigger the instance
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

        // Stop and release the FMOD instance so it doesn't linger in memory.
        humInstance.stop(STOP_MODE.IMMEDIATE);
        humInstance.release();
    }
}