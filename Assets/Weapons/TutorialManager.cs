using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("Pop-up Visuals (Arrow sprites etc.)")]
    public GameObject[] popUps;

    [Header("References")]
    [SerializeField] private TutorialEnemySpawner tutorialSpawner;
    [SerializeField] private GameObject playerObject;
    private PlayerMovement playerMovement;
    [SerializeField] private PerkSelectionUI perkSelectionUI;
    [SerializeField] private GameObject spawnManager;
    private SpawnManager spawnManagerScript;
    [SerializeField] private GameObject maze;
    private MazeChanger mazeChangerScript;

    private readonly string[] dialogueLines = new string[]
    {
        "Hey! Use W A S D to move around. Give it a try!",      // 0 - movement
        "Hold Shift to dash. Great for dodging attacks!",        // 1 - dash
        "Left-click to shoot. Take aim and fire!",               // 2 - shoot
        "Press R to reload. Don't get caught empty!",            // 3 - reload
        "Enemies incoming, take them all down!",                 // 4 - kill tutorial enemies
        "You levelled up! Let's see what you can do.",           // 5 - level up
        "Well done! Time to head into the maze!"                 // 6 - complete
    };

    private int popUpIndex = 0;
    private bool stepComplete = false;
    private bool enemiesDefeated = false;
    private Coroutine typeRoutine;

    private void Awake()
    {
        mazeChangerScript = maze.GetComponent<MazeChanger>();
        spawnManagerScript = spawnManager.GetComponent<SpawnManager>();
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        if (playerObject != null)
            playerMovement = playerObject.GetComponent<PlayerMovement>();
        else
            Debug.LogWarning("[TutorialManager] playerObject not assigned.");

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
                {
                    PlayApplause();
                    AdvanceStep();
                }
                break;

            case 1: // Dash
                if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                {
                    PlayApplause();
                    PlayCrowdNoise();
                    AdvanceStep();
                }
                break;

            case 2: // Shoot
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    PlayApplause();
                    PlayCrowdNoise();
                    AdvanceStep();
                }
                break;

            case 3: // Reload
                if (Input.GetKeyDown(KeyCode.R))
                {
                    PlayApplause();
                    AdvanceStep();
                }
                break;

            case 4: // Kill all tutorial enemies — handled via OnAllTutorialEnemiesDefeated()
                break;

            case 5: // Wait for perk selection — handled via PerkSelectionUI.OnPerkSelected AdvanceStep()
                break;

                // case 6 handled in WaitForHostThenUnlock on final step
        }
    }

    // Step Display

    private void ShowStep(int index)
    {
        for (int i = 0; i < popUps.Length; i++)
            popUps[i].SetActive(i == index);

        HostMood mood = (index == 4 || index == 6) ? HostMood.Ecstatic : HostMood.Talk;
        HostManager.Instance.SayAndHold(dialogueLines[index], mood);

        stepComplete = true;

        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(WaitForHostThenUnlock(index));

        // Spawn tutorial enemies as soon as step 4 is shown
        if (index == 4 && tutorialSpawner != null)
            tutorialSpawner.SpawnTutorialEnemies();
    }

    // Waits for HostManager to finish typing before unlocking player input
    private IEnumerator WaitForHostThenUnlock(int stepIndex)
    {
        yield return new WaitUntil(() => !HostManager.Instance.IsTalking);

        // Final step — kick off completion instead of unlocking input
        if (stepIndex == dialogueLines.Length - 1)
        {
            Debug.Log("[TutorialManager] Final step dialogue done — starting completion");
            StartCoroutine(CompleteAndTransition());
            yield break;
        }

        stepComplete = false;
        Debug.Log($"[TutorialManager] Step {stepIndex} unlocked — waiting for player input");
    }

    public void AdvanceStep()
    {
        Debug.Log($"[TutorialManager] AdvanceStep called — going from {popUpIndex} to {popUpIndex + 1}, stepComplete={stepComplete}");
        popUpIndex++;

        if (popUpIndex < dialogueLines.Length)
            ShowStep(popUpIndex);
        else if (popUpIndex == 4 && spawnManagerScript.currentEnemyCount == 0)
        {
            StartCoroutine(CompleteAndTransition());
        }
    }

    // Completion

    private IEnumerator CompleteAndTransition()
    {
        Debug.Log("[TutorialManager] CompleteAndTransition started");
        mazeChangerScript.enabled = true;

        // Crowd reacts to tutorial completion
        PlayCrowdNoise();

        // Wait for final dialogue to finish if still playing
        yield return new WaitUntil(() => !HostManager.Instance.IsTalking);

        yield return new WaitForSecondsRealtime(2f);

        HostManager.Instance.HideDialogue();

        for (int i = 0; i < popUps.Length; i++)
            popUps[i].SetActive(false);

        Debug.Log("[TutorialManager] Calling GameManager.OnTutorialComplete()");
        GameManager.Instance.OnTutorialComplete();
    }

    // Called by TutorialEnemySpawner when all enemies are dead
    public void OnAllTutorialEnemiesDefeated()
    {
        if (enemiesDefeated) return;

        Debug.Log("[TutorialManager] All enemies defeated");
        enemiesDefeated = true;
        stepComplete = true;
        popUpIndex = 5;

        // Crowd reacts to clearing all enemies
        PlayCrowdNoise();

        playerMovement.currentLevel = 1;
        perkSelectionUI.Show();
    }

    // Audio helpers

    // Ooh/ahh crowd reaction — bigger moments (enemies cleared, perk chosen, tutorial done)
    private void PlayCrowdNoise()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.crowdNoise, Vector3.zero);
    }

    // Clapping — smaller step completions (WASD, dash, shoot, reload)
    private void PlayApplause()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.crowdApplause, Vector3.zero);
    }
}