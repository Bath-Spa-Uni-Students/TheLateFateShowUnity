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

    [Header("Hum Settings")]
    [SerializeField] private float humIntervalMin = 4f;
    [SerializeField] private float humIntervalMax = 9f;
    [SerializeField] private float humMaxDistance = 20f;

    public void Init(KeySpawner owner, int index)
    {
        spawner = owner;
        keyIndex = index;

        // Apply the sprite that corresponds to this slot
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null && keySprites != null && index < keySprites.Length)
            sr.sprite = keySprites[index];
        else if (keySprites == null || keySprites.Length == 0)
            Debug.LogWarning("[KeyPickup] No keySprites assigned on the prefab!");
        else if (index >= keySprites.Length)
            Debug.LogWarning($"[KeyPickup] keyIndex {index} is out of range — only {keySprites.Length} sprite(s) assigned.");
    }

    private void Start()
    {
        humCoroutine = StartCoroutine(HumLoop());
    }

    private void OnDestroy()
    {
        if (humCoroutine != null)
            StopCoroutine(humCoroutine);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true;

        // Stop the hum so it doesn't fire again during destroy
        if (humCoroutine != null)
        {
            StopCoroutine(humCoroutine);
            humCoroutine = null;
        }

        // Play pickup SFX at this position
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.keyPickup, transform.position);

        // Pass keyIndex so the spawner knows exactly which UI slot to light up
        spawner.OnKeyCollected(gameObject, keyIndex);
    }

    private IEnumerator HumLoop()
    {
        while (!collected)
        {
            float wait = Random.Range(humIntervalMin, humIntervalMax);
            yield return new WaitForSeconds(wait);

            if (collected) yield break;

            // Only pulse if a player exists and is within range
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist <= humMaxDistance)
                    AudioManager.Instance.PlayOneShot(FMODEvents.Instance.keyHum, transform.position);
            }
        }
    }
}