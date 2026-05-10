using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private KeySpawner spawner;
    private bool collected = false;

    // Called by KeySpawner immediately after instantiation
    public void Init(KeySpawner owner)
    {
        spawner = owner;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag("Player")) return;

        collected = true; // guard against double-firing
        spawner.OnKeyCollected(gameObject);
    }
}