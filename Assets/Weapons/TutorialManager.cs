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

    private readonly string[] dialogueLines = new string[]
    {
        "Hey! Use W A S D to move around. Give it a try!",      // 0 – movement
        "Hold Shift to dash. Great for dodging attacks!",        // 1 – dash
        "Left-click to shoot. Take aim and fire!",               // 2 – shoot
        "Press R to reload. Don't get caught empty!",            // 3 – reload
        "You levelled up! Let's see what you can do.",         // 4 – kill tutorial enemies  
        "Enemies incoming take them all down!",                // 5 – level up (currentLevel >= 1, started at 0)
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
            case 4: // Kill all tutorial enemies
                if (enemiesDefeated)
                    AdvanceStep();
                break;
            case 5: // Level up — player starts at 0, so level 1 = first level-up
                if (playerMovement != null && playerMovement.currentLevel >= 1)
                    AdvanceStep();
                break;
                // case 6 is handled in ShowStep via OnTutorialSequenceComplete
        }
    }

    //Step display 

    private void ShowStep(int index)
    {
        for (int i = 0; i < popUps.Length; i++)
            popUps[i].SetActive(i == index);

        if (typeRoutine != null) StopCoroutine(typeRoutine);

        string line = index < dialogueLines.Length ? dialogueLines[index] : "";
        typeRoutine = StartCoroutine(TypewriterRoutine(line, index));

        // Spawn the tutorial enemies as soon as step 5 is shown
        if (index == 5 && tutorialSpawner != null)
            tutorialSpawner.SpawnTutorialEnemies();
    }

    private void AdvanceStep()
    {
        popUpIndex++;

        if (popUpIndex < dialogueLines.Length)
            ShowStep(popUpIndex);
        else
            OnTutorialSequenceComplete();
    }

    //Typewriter 

    private IEnumerator TypewriterRoutine(string line, int stepIndex)
    {
        stepComplete = true;
        dialogueBox.SetActive(true);
        dialogueText.text = "";

        PlayHostAnimation(stepIndex);

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typeSpeed);
        }

        hostAnimator.CrossFade(AnimIdle, 0.2f);
        stepComplete = false;
    }

    private void PlayHostAnimation(int stepIndex)
    {
        int anim = (stepIndex == 4 || stepIndex == 6) ? AnimExcite : AnimTalk;
        hostAnimator.CrossFade(anim, 0.1f);
    }

    // External callbacks


    // Called by TutorialEnemySpawner when all 3 grunts are dead.
    public void OnAllTutorialEnemiesDefeated()
    {
        enemiesDefeated = true;     // Update() will pick this up next frame
    }

    //Tutorial end

    private void OnTutorialSequenceComplete()
    {
        dialogueBox.SetActive(false);

        for (int i = 0; i < popUps.Length; i++)
            popUps[i].SetActive(false);

        hostAnimator.CrossFade(AnimIdle, 0.2f);

        // Hand off to GameManager to do the room swap and teleport
        GameManager.Instance.OnTutorialComplete();
    }
}