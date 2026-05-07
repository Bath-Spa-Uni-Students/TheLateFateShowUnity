using System.Collections;
using UnityEngine;

public enum GameState { Tutorial, Game, Win, Lose }

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

    [Header("Player")]
    [SerializeField] private GameObject playerObject;
    private PlayerMovement playerMovement;

    [Header("Transition")]
    [SerializeField] private GameObject transitionCanvas;
    [SerializeField] private Animation transitionAnimation;
    [SerializeField] private float transitionDelay = 15f; // Delay for any transition animations not sure whether to use static animation or we will be right back

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (playerObject != null)
            playerMovement = playerObject.GetComponent<PlayerMovement>();

        EnterTutorial();
    }

    private void EnterTutorial()
    {
        CurrentState = GameState.Tutorial;

        tutorialRoom.SetActive(true);
        mazeArea.SetActive(false);

        // Only the tutorial spawner runs during the tutorial
        if (mazeSpawner != null) mazeSpawner.enabled = false;
        if (tutorialSpawner != null) tutorialSpawner.enabled = true;

        Debug.Log("GameManager: Tutorial started.");
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
        transitionAnimation.Play();

        // Teleport immediately while canvas covers the screen
        CurrentState = GameState.Game;
        tutorialRoom.SetActive(false);
        mazeArea.SetActive(true);

        if (playerMovement != null && mazeTeleportPoint != null)
            playerMovement.transform.position = mazeTeleportPoint.position;

        if (tutorialSpawner != null) tutorialSpawner.enabled = false;
        if (mazeSpawner != null) mazeSpawner.enabled = true;
        SpawnManager.Instance.ResetEnemyCount();

        // Keep canvas up for animation to play out
        yield return new WaitForSecondsRealtime(transitionDelay);

        transitionCanvas.SetActive(false);
        Debug.Log("GameManager: Transitioned to maze.");
    }
    //call this in beetle boss death
    public void OnGameWin()
    {
        if (CurrentState != GameState.Game) return;
        CurrentState = GameState.Win;

        if (mazeSpawner != null) mazeSpawner.enabled = false;
        Debug.Log("GameManager: Player won!");
        //Show win screen here
    }


    // Call this from PlayerMovement.PlayerDie() when the player dies.
    public void OnGameLose()
    {
        CurrentState = GameState.Lose;

        if (mazeSpawner != null) mazeSpawner.enabled = false;
        if (tutorialSpawner != null) tutorialSpawner.enabled = false;

        Debug.Log("GameManager: Player lost.");
        //  Show game over screen here
    }
}