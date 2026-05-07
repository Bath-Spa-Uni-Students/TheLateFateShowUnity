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

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
    // Core typewriter logic, with optional hold parameter to keep dialogue open for lengthier passages
    private IEnumerator TypewriterRoutine(string line, HostMood mood, System.Action onComplete, bool hold = false)
    {
        IsTalking = true;
        hostObject.SetActive(true);
        dialogueBox.SetActive(true);
        dialogueText.text = "";

        hostAnimator.CrossFade(MoodToAnim(mood), 0.2f);

        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSecondsRealtime(typeSpeed);
        }

        if (!hold)
        {
            yield return new WaitForSecondsRealtime(2f);
            HideDialogue();
        }

        IsTalking = false;
        onComplete?.Invoke();
    }
    // Map moods to animator states, with special handling for AngryTransition if currently talking
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

