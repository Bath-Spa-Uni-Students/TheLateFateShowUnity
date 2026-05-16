using System.Collections;
using UnityEngine;
using FMOD.Studio;

public class KeyPickup : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private Sprite[] keySprites;          // assign all 3 in prefab

    private KeySpawner spawner;
    private bool collected = false;
    private SpriteRenderer spriteRenderer;
    private Coroutine humCoroutine;

    public void Init(KeySpawner owner)
    {
        spawner = owner;
    }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Pick a random fragment sprite
        if (keySprites != null && keySprites.Length > 0 && spriteRenderer != null)
            spriteRenderer.sprite = keySprites[Random.Range(0, keySprites.Length)];
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

        spawner.OnKeyCollected(gameObject);
    }

    private IEnumerator HumLoop()
    {
        while (!collected)
        {
            // Play hum SFX at this position
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.keyHum, transform.position);
            yield return new WaitForSeconds(1f); // Adjust the interval as needed
        }
    }
}