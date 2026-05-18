using System.Collections;
using TMPro;
using UnityEngine;

public enum HostMood { Talk, Ecstatic, Idle, AngryTransition, AngryTalk }

public class HostManager : MonoBehaviour
{
    public static HostManager Instance { get; private set; }

    [Header("Host Visuals")]
    public Animator hostAnimator;
    public TMP_Text dialogueText;
    public GameObject dialogueBox;
    public GameObject hostObject;

    [Header("Typewriter Settings")]
    public float typeSpeed = 0.04f;

    private static readonly int AnimTalk = Animator.StringToHash("Talk");
    private static readonly int AnimIdle = Animator.StringToHash("Idle");
    private static readonly int AnimEcstatic = Animator.StringToHash("Ecstatic");
    private static readonly int AnimAngryTransition = Animator.StringToHash("AngryTransition");
    private static readonly int AnimAngryTalk = Animator.StringToHash("AngryTalk");

    private Coroutine typeRoutine;
    public bool IsTalking { get; private set; }

    // Persistent instance for tv static so we can stop it cleanly
    private FMOD.Studio.EventInstance tvStaticInstance;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        hostObject.SetActive(false);
        dialogueBox.SetActive(false);
    }

    // Show a line then hide automatically
    public void Say(string line, HostMood mood = HostMood.Talk, System.Action onComplete = null)
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypewriterRoutine(line, mood, onComplete));
    }

    // Show a line and keep the box open until HideDialogue() is called
    public void SayAndHold(string line, HostMood mood = HostMood.Talk)
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        typeRoutine = StartCoroutine(TypewriterRoutine(line, mood, null, hold: true));
    }

    // Immediately hide dialogue and stop any ongoing typewriter effect
    public void HideDialogue()
    {
        if (typeRoutine != null) StopCoroutine(typeRoutine);
        dialogueBox.SetActive(false);
        hostObject.SetActive(false);
        hostAnimator.CrossFade(AnimIdle, 0.2f);
        IsTalking = false;
    }

    // Called by GameManager when the transition canvas appears
    public void PlayTVBeep()
    {
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.tvBeep, Vector3.zero);
    }

    public void StartTVStatic()
    {
        tvStaticInstance = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.tvStatic);
        tvStaticInstance.start();
    }


    public void StopTVStatic()
    {
        tvStaticInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        tvStaticInstance.release();
    }

    // Core typewriter logic
    private IEnumerator TypewriterRoutine(string line, HostMood mood, System.Action onComplete, bool hold = false)
    {
        IsTalking = true;

        // Host appear sound plays once as the host becomes visible
        hostObject.SetActive(true);
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.hostAppear, Vector3.zero);

        dialogueBox.SetActive(true);
        dialogueText.text = "";
        hostAnimator.CrossFade(MoodToAnim(mood), 0.2f);

        foreach (char c in line)
        {
            dialogueText.text += c;
            // Click on every character skip spaces to avoid cluttered noise
            if (c != ' ')
                AudioManager.Instance.PlayOneShot(FMODEvents.Instance.typeWriterClick, Vector3.zero);
            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        // Ding when the line finishes
        AudioManager.Instance.PlayOneShot(FMODEvents.Instance.typeWriterDing, Vector3.zero);

        if (!hold)
        {
            yield return new WaitForSecondsRealtime(2f);
            hostAnimator.CrossFade(AnimIdle, 0.2f);
            HideDialogue();
        }
        else
        {
            hostAnimator.CrossFade(AnimIdle, 0.2f);
        }

        IsTalking = false;
        onComplete?.Invoke();
    }

    private int MoodToAnim(HostMood mood) => mood switch
    {
        HostMood.Talk => AnimTalk,
        HostMood.Ecstatic => AnimEcstatic,
        HostMood.Idle => AnimIdle,
        HostMood.AngryTransition when IsTalking => AnimAngryTransition,
        HostMood.AngryTalk => AnimAngryTalk,
        _ => AnimTalk
    };
}