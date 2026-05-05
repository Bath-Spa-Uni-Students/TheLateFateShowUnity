using UnityEngine;
using System.Collections;
using UnityEngine.UI;

// Attach this to a trigger collider placed BEHIND the boss.
// When the player enters it they are teleported back in front of the boss
// and the host provides some excuse as for why

public class BossTeleportBarrier : MonoBehaviour
{

    [Header("Boss Reference")]
    [SerializeField] private BossBehaviour bossBehaviour;

    [Header("Quip UI")]
    [SerializeField] private GameObject quipPanel;// A UI panel that contains the quip text, which can be enabled/disabled
    [SerializeField] private Text quipText;// UI elements to display the quip
    [SerializeField] private float quipDuration = 3f;// How long the quip is shown before disappearing

    // A pool of lines to randomly pick from
    private static readonly string[] quips = new string[]
    {
        "\"Due to budget cuts, I regret to inform the boss can't turn around…\"",
    };

    // Runtime
    private string playerTag = "Player";
    private Coroutine hideCoroutine;
    private Canvas fallbackCanvas;   // created at runtime if no quipPanel is wired up

    private void Awake()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        //teleport player back in front of the boss
    }

    private void TeleportPlayerToFront(Transform playerTransform)
    {

    }

    //ui stuff here, show a random quip from the pool for a few seconds when the player hits the barrier


}
