using System.Collections;
using UnityEngine;
using FMOD.Studio;

public enum GameState { Tutorial, Game, Boss, Win, Lose }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.Tutorial;

    [Header("Tutorial")]
    [SerializeField] private GameObject tutorialRoom;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private TutorialEnemySpawner tutorialSpawner;
    [SerializeField] private GameObject tutorialCanvas;

    [Header("Main Game")]
    [SerializeField] private GameObject mazeArea;
    [SerializeField] private EnemySpawner mazeSpawner;
    [SerializeField] private Transform mazeTeleportPoint;
    [SerializeField] private MazeChanger mazeChanger;

    [Header("Boss Room")]
    [SerializeField] private GameObject bossRoom;
    [SerializeField] private Transform bossTeleportPoint;
    [SerializeField] private GameObject beetleBoss;

    [Header("Key System")]
    [SerializeField] private int keysRequired = 3;
    [SerializeField] private int forcedBossLevel = 10;
    [SerializeField] private GameObject bossTeleportButtonUI;
    private int keysCollected = 0;
    public int KeysCollected => keysCollected;

    [Header("Transition")]
    [SerializeField] private GameObject transitionCanvas;
    [SerializeField] private float transitionDelay = 10f;

    [Header("Player")]
    [SerializeField] private GameObject playerObject;
    private PlayerMovement playerMovement;

    private EventInstance explorationTheme;
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Update()
    {
        if (CurrentState != GameState.Game) return;
        if (keysCollected < keysRequired) return;

        if (Input.GetKeyDown(KeyCode.T))
            OnPlayerRequestBossTeleport();
    }

    private void Start()
    {
        if (playerObject != null)
            playerMovement = playerObject.GetComponent<PlayerMovement>();

        if (bossTeleportButtonUI != null)
            bossTeleportButtonUI.SetActive(false);

        explorationTheme = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.levelAmbience);
        EnterTutorial();
    }

    // Tutorial 

    private void EnterTutorial()
    {
        CurrentState = GameState.Tutorial;
        tutorialRoom.SetActive(true);
        mazeArea.SetActive(false);

        if (bossRoom != null) bossRoom.SetActive(false);
        if (mazeSpawner != null) mazeSpawner.enabled = false;
        if (tutorialSpawner != null) tutorialSpawner.enabled = true;

        Debug.Log("[GameManager] Tutorial started.");
    }

    public void OnTutorialComplete()
    {
        if (CurrentState != GameState.Tutorial) return;
        StartCoroutine(TransitionToMaze());
    }

    private IEnumerator TransitionToMaze()
    {
        transitionCanvas.SetActive(true);
        tutorialCanvas.SetActive(false);

        // Teleport immediately while canvas covers the screen
        CurrentState = GameState.Game;
        tutorialRoom.SetActive(false);
        mazeArea.SetActive(true);

        if (playerMovement != null && mazeTeleportPoint != null)
            playerMovement.transform.position = mazeTeleportPoint.position;

        if (tutorialSpawner != null) tutorialSpawner.enabled = false;
        if (mazeSpawner != null) mazeSpawner.enabled = true;

        SpawnManager.Instance.ResetEnemyCount();
        KeySpawner.Instance.Initialise(keysRequired);
        KeySpawner.NotifyMazeRegenerated();
        // Keep canvas up for transition animation to play out
        yield return new WaitForSecondsRealtime(transitionDelay);

        transitionCanvas.SetActive(false);
        Debug.Log("[GameManager] Transitioned to maze.");
        explorationTheme.start();

    }

    // Key System 

    public void OnKeyCollected()
    {
        if (CurrentState != GameState.Game) return;

        keysCollected++;
        Debug.Log($"[GameManager] Key collected: {keysCollected}/{keysRequired}");

        // key UI here
        // UIManager.Instance.UpdateKeyDisplay(keysCollected, keysRequired);

        if (keysCollected >= keysRequired)
            OnAllKeysCollected();
    }

    private void OnAllKeysCollected()
    {
        Debug.Log("[GameManager] All keys collected — player can now teleport to boss");

        if (bossTeleportButtonUI != null)
            bossTeleportButtonUI.SetActive(true);

        HostManager.Instance.Say(
            "You've found all the keys. Press T whenever you're ready for the boss!",
            HostMood.Ecstatic
        );  
    }

    public void OnPlayerRequestBossTeleport()
    {
        if (CurrentState != GameState.Game) return;
        if (keysCollected < keysRequired) return;

        if (bossTeleportButtonUI != null)
            bossTeleportButtonUI.SetActive(false);

        StartCoroutine(TransitionToBoss());
    }

    public void OnPlayerReachedMaxLevel()
    {
        if (CurrentState != GameState.Game) return;

        if (bossTeleportButtonUI != null)
            bossTeleportButtonUI.SetActive(false);

        if (keysCollected < keysRequired)
        {
            Debug.Log("[GameManager] Max level reached without all keys — host scolds player");

            HostManager.Instance.Say(
                "Really? You reached max level without finding all the keys? Fine... to the boss you go.",
                HostMood.Talk,
                onComplete: () => StartCoroutine(TransitionToBoss())
            );
        }
        else
        {
            Debug.Log("[GameManager] Max level reached with all keys — forcing boss transition");

            HostManager.Instance.Say(
                "Max level and all the keys — time to face the boss!",
                HostMood.Ecstatic,
                onComplete: () => StartCoroutine(TransitionToBoss())
            );
        }
    }

    private IEnumerator TransitionToBoss()
    {
        explorationTheme.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        transitionCanvas.SetActive(true);

        CurrentState = GameState.Boss;
        mazeArea.SetActive(false);
        bossRoom.SetActive(true);

        if (beetleBoss != null) beetleBoss.SetActive(true);

        if (mazeChanger != null) mazeChanger.StopSwitching();

        if (playerMovement != null && bossTeleportPoint != null)
            playerMovement.transform.position = bossTeleportPoint.position;

        if (mazeSpawner != null) mazeSpawner.enabled = false;

        yield return new WaitForSecondsRealtime(transitionDelay);

        transitionCanvas.SetActive(false);
        Debug.Log("[GameManager] Transitioned to boss room.");
    }

    public void OnGameWin()
    {
        if (CurrentState != GameState.Boss) return;
        CurrentState = GameState.Win;

        if (mazeSpawner != null) mazeSpawner.enabled = false;

       HostManager.Instance.Say("You did it! The beetle boss is defeated!", HostMood.Ecstatic);

        Debug.Log("[GameManager] Player won!");
        // Show win screen here
    }
    public void OnGameLose()
    {
        CurrentState = GameState.Lose;

        if (mazeSpawner != null) mazeSpawner.enabled = false;
        if (tutorialSpawner != null) tutorialSpawner.enabled = false;

       HostManager.Instance.Say("Oh dear... better luck next time.", HostMood.Talk);

        Debug.Log("[GameManager] Player lost.");
        // Show game over screen here
    }
}