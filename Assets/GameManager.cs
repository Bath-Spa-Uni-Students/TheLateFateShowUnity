using System.Collections;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using FMOD.Studio;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using TMPro;

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
    [SerializeField] private GameObject gameManagerCanvas;
    [SerializeField] private GameObject bossHealthBar;
    private PlayerMovement playerMovement;
    private PlayerInput playerInput;

    public TextMeshProUGUI ppText;
    private TextMeshPro ppPoints;
    public float playerFame;

    [SerializeField] private GameObject congratsCanvas;

    private EventInstance explorationTheme;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        SetPPText(playerFame);

        if (playerObject != null)
            playerMovement = playerObject.GetComponent<PlayerMovement>();

        if (bossTeleportButtonUI != null)
            bossTeleportButtonUI.SetActive(false);

        explorationTheme = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.explorationTheme);

        if (SceneManager.GetActiveScene().name == "Tutorial")
        {
            EnterTutorial();
            Debug.Log("[GameManager] Starting in tutorial scene.");
        }
        else if (SceneManager.GetActiveScene().name == "Map 1")
        {
            StartCoroutine(TransitionToMaze());
        }
            else
                Debug.LogWarning("[GameManager] Unrecognized scene � no game state entered.");
    }

    // Tutorial

    private void EnterTutorial()
    {
        if (SceneManager.GetActiveScene().name != "Tutorial")
        {
            Debug.LogError("[GameManager] Attempted to enter tutorial state while not in tutorial scene!");
            return;
        }
        else
        {
            CurrentState = GameState.Tutorial;
            tutorialRoom.SetActive(true);
            mazeArea.SetActive(false);

            if (bossRoom != null) bossRoom.SetActive(false);
            if (mazeSpawner != null) mazeSpawner.enabled = false;
            if (tutorialSpawner != null) tutorialSpawner.enabled = true;

            Debug.Log("[GameManager] Tutorial started.");
        }
    }

    public void OnTutorialComplete()
    {
        if (CurrentState != GameState.Tutorial) return;
        SceneManager.LoadScene("Map 1");
    }

    private IEnumerator TransitionToMaze()
    {
        Time.timeScale = 0f;
        gameManagerCanvas.SetActive(false);
        tutorialCanvas.SetActive(false);

        transitionCanvas.SetActive(true);

        // Beep first, then static kicks in as the canvas appears
        HostManager.Instance.PlayTVBeep();
        yield return new WaitForSecondsRealtime(0.3f);

        
        HostManager.Instance.StartTVStatic();

        // Teleport while canvas covers the screen
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

        yield return new WaitForSecondsRealtime(transitionDelay);
        Time.timeScale = 1f;

        // Static stops as the canvas comes down
        HostManager.Instance.StopTVStatic();
        transitionCanvas.SetActive(false);

        gameManagerCanvas.SetActive(true);
        Debug.Log("[GameManager] Transitioned to maze.");
        explorationTheme.start();
    }

    // Key System

    public void OnKeyCollected()
    {
        if (CurrentState != GameState.Game) return;

        keysCollected++;
        Debug.Log($"[GameManager] Key collected: {keysCollected}/{keysRequired}");

        if (keysCollected >= keysRequired)
            OnAllKeysCollected();
    }

    private void OnAllKeysCollected()
    {
        Debug.Log("[GameManager] All keys collected � player can now teleport to boss");

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
            Debug.Log("[GameManager] Max level reached without all keys � host scolds player");

            HostManager.Instance.Say(
                "Really? You reached max level without finding all the keys? Fine... to the boss you go.",
                HostMood.Talk,
                onComplete: () => StartCoroutine(TransitionToBoss())
            );
        }
        else
        {
            Debug.Log("[GameManager] Max level reached with all keys � forcing boss transition");

            HostManager.Instance.Say(
                "Max level and all the keys � time to face the boss!",
                HostMood.Ecstatic,
                onComplete: () => StartCoroutine(TransitionToBoss())
            );
        }
    }

    private IEnumerator TransitionToBoss()
    {
        explorationTheme.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        transitionCanvas.SetActive(true);

        // Beep then static as the canvas appears
        HostManager.Instance.PlayTVBeep();
        yield return new WaitForSecondsRealtime(0.3f);

        
        HostManager.Instance.StartTVStatic();

        gameManagerCanvas.SetActive(false);
        tutorialCanvas.SetActive(false);

        CurrentState = GameState.Boss;
        mazeArea.SetActive(false);
        bossRoom.SetActive(true);

        if (beetleBoss != null) beetleBoss.SetActive(true);
        if (mazeChanger != null) mazeChanger.StopSwitching();

        if (playerMovement != null && bossTeleportPoint != null)
            playerMovement.transform.position = bossTeleportPoint.position;

        if (mazeSpawner != null) mazeSpawner.enabled = false;

        Time.timeScale = 0f; // Pause the game during transition

        yield return new WaitForSecondsRealtime(transitionDelay);

        // Static stops as the canvas comes down
        HostManager.Instance.StopTVStatic();
        bossHealthBar.SetActive(true);
        transitionCanvas.SetActive(false);
        gameManagerCanvas.SetActive(true);

        Debug.Log("[GameManager] Transitioned to boss room.");

        Time.timeScale = 1f;

    }

    public void OnGameWin()
    {
        //if (CurrentState != GameState.Boss) return;
        CurrentState = GameState.Win;
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.victoryStinger, Vector3.zero);
        HostManager.Instance.Say("You did it! The beetle boss is defeated!", HostMood.Ecstatic);


        if (mazeSpawner != null) mazeSpawner.enabled = false;

        HostManager.Instance.Say("You did it! The beetle boss is defeated!", HostMood.Ecstatic);

        Debug.Log("[GameManager] Player won!");

        playerFame = playerMovement.fame;


        StartCoroutine(LoadCongratsSceneAfterDelay(3f));
        //SceneManager.LoadScene("Congrats");

        // Show win screen here
    }

    IEnumerator LoadCongratsSceneAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Time.timeScale = 0f;
        ppText.text = $"+{playerFame.ToString("F0")}";
        congratsCanvas.SetActive(true);
    }
    public void OnGameLose()
    {
        CurrentState = GameState.Lose;
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.defeatStinger, Vector3.zero);
        HostManager.Instance.Say("Oh dear... better luck next time.", HostMood.Talk);

        if (mazeSpawner != null) mazeSpawner.enabled = false;
        if (tutorialSpawner != null) tutorialSpawner.enabled = false;

        HostManager.Instance.Say("Oh dear... better luck next time.", HostMood.Talk);

        Debug.Log("[GameManager] Player lost.");
    }

    private void SetPPText(float ppText)
    {
        if (ppPoints != null)
        {
            ppPoints.text = playerFame.ToString("F0");
        }
    }
}