using DG.Tweening.Core.Easing;
using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Pop-up Visuals (Arrow sprites etc.)")]
    public GameObject[] popUps;

    [Header("Host")]
    public Animator hostAnimator;    // Animator on your host character
    public TMP_Text dialogueText;    // TMP_Text inside the dialogue box
    public GameObject dialogueBox;     // Parent dialogue box panel

    [Header("Typewriter Settings")]
    public float typeSpeed = 0.04f;

    [Header("References")]
    [SerializeField] private TutorialEnemySpawner tutorialSpawner;
    [SerializeField] private GameObject playerObject;
    private PlayerMovement playerMovement;
    [SerializeField] private PerkSelectionUI perkSelectionUI;

    private readonly string[] dialogueLines = new string[]
    {
        "Hey! Use W A S D to move around. Give it a try!",      // 0 – movement
        "Hold Shift to dash. Great for dodging attacks!",        // 1 – dash
        "Left-click to shoot. Take aim and fire!",               // 2 – shoot
        "Press R to reload. Don't get caught empty!",            // 3 – reload
          "Enemies incoming take them all down!",        // 4 – kill tutorial enemies  
         "You levelled up! Let's see what you can do.",   // 5 – level up (currentLevel >= 1, started at 0)
        "Well done! Head through the portal into the maze!"      // 6 – complete
    };

    private static readonly int AnimTalk = Animator.StringToHash("Talk");
    private static readonly int AnimIdle = Animator.StringToHash("Idle");
    private static readonly int AnimExcite = Animator.StringToHash("Excited");

    private int popUpIndex = 0;
    private bool stepComplete = false;
    private bool enemiesDefeated = false;   // Set by callback from TutorialEnemySpawner
    private Coroutine typeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (playerObject != null)
            playerMovement = playerObject.GetComponent<PlayerMovement>();
        else
            Debug.LogWarning("TutorialManager: playerObject not assigned.");

        ShowStep(0);
    }

    private void Update()
    {
        if (stepComplete) return;

        switch (popUpIndex)
        {
            case 0: // WASD
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) ||
                    Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                    AdvanceStep();
                break;

            case 1: // Dash
                if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                    AdvanceStep();
                break;

            case 2: // Shoot
                if (Input.GetKeyDown(KeyCode.Mouse0))
                    AdvanceStep();
                break;

            case 3: // Reload
                if (Input.GetKeyDown(KeyCode.R))
                    AdvanceStep();
                break;
            case 4:
                if (enemiesDefeated && !stepComplete)
                {
                    stepComplete = true;   // prevent re-entry
                    playerMovement.currentLevel = 1;
                    perkSelectionUI.Show();
                }
                break;
            case 5:
                Debug.Log($"[TutorialManager] Update case 5 — currentLevel={playerMovement.currentLevel}, stepComplete={stepComplete}");
                if (playerMovement != null && playerMovement.currentLevel >= 1)
                    AdvanceStep();
                break;
        }
    }

    //Step display 

    private void ShowStep(int index)
    {
        for (int i = 0; i < popUps.Length; i++)
            popUps[i].SetActive(i == index);

        HostMood mood = (index == 4 || index == 6) ? HostMood.Ecstatic : HostMood.Talk;
        HostManager.Instance.SayAndHold(dialogueLines[index], mood);

        if (index == 4 && tutorialSpawner != null)
            tutorialSpawner.SpawnTutorialEnemies();
    }

    public void AdvanceStep()
    {
    Debug.Log($"[TutorialManager] AdvanceStep called — going from {popUpIndex} to {popUpIndex + 1}, stepComplete={stepComplete}");
        popUpIndex++;

        if (popUpIndex < dialogueLines.Length)
            ShowStep(popUpIndex);
        else
            OnTutorialSequenceComplete();
    }
    public void OnAllTutorialEnemiesDefeated()
    {
        Debug.Log($"[TutorialManager] OnAllTutorialEnemiesDefeated called. stepComplete={stepComplete}, popUpIndex={popUpIndex}, enemiesDefeated={enemiesDefeated}");

        if (enemiesDefeated)
        {
            Debug.LogWarning("[TutorialManager] OnAllTutorialEnemiesDefeated called MORE THAN ONCE — returning early");
            return;
        }

        enemiesDefeated = true;
        stepComplete = true;
        popUpIndex = 5;

        Debug.Log($"[TutorialManager] After setting flags — stepComplete={stepComplete}, popUpIndex={popUpIndex}");

        playerMovement.currentLevel = 1;
        Debug.Log($"[TutorialManager] Set currentLevel to {playerMovement.currentLevel}, calling perkSelectionUI.Show()");

        perkSelectionUI.Show();
    }

    private void PlayHostAnimation(int stepIndex)
    {
        int anim = (stepIndex == 4 || stepIndex == 6) ? AnimExcite : AnimTalk;
    }

    //Tutorial end

    private void OnTutorialSequenceComplete()
    {
        StartCoroutine(CompleteAndTransition());
    }
    private IEnumerator CompleteAndTransition()
    {
        Debug.Log("[TutorialManager] CompleteAndTransition started");

        // Let the player read the final line
        yield return new WaitForSecondsRealtime(2f);

        dialogueBox.SetActive(false);
        for (int i = 0; i < popUps.Length; i++)
            popUps[i].SetActive(false);

        Debug.Log("[TutorialManager] Calling GameManager.OnTutorialComplete()");
        GameManager.Instance.OnTutorialComplete();
    }


}