using UnityEngine;


// Cehsts are Placed in the maze they Contain a weapons with 1-2 pre-attached perks
// When the player interacts hands the WeaponInstance to WeaponManager.

public class ChestObject : MonoBehaviour
{
    [Header("Contents")]
    public WeaponInstance contents;

    [Header("Interaction")]
    [SerializeField] private float interactRadius = 1.5f;
    [SerializeField] private KeyCode interactKey = KeyCode.E;
    [SerializeField] private GameObject interactPrompt; // Optional "Press E" UI

    private bool playerInRange = false;
    private bool opened = false;

    private void Update()
    {
        if (opened) return;
        if (playerInRange && Input.GetKeyDown(interactKey))
            Open();
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
        if (interactPrompt) interactPrompt.SetActive(false);

        Debug.Log($"[ChestObject] Opened chest containing: {contents.weaponType} with {contents.perks.Count} perk(s)");

        WeaponManager.Instance.ReceiveChestWeapon(contents);

        // Chest animation here
        // AudioManager.Instance.PlayOneShot(chestOpening)

        // Destroy after a short delay to allow animations to finish
        Destroy(gameObject, 0.1f);
    }

    // Visualise interact radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactRadius);
    }
}