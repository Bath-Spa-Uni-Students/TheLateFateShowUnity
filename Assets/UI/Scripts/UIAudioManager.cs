using UnityEngine;
using FMODUnity;

public class UIAudioManager : MonoBehaviour
{
    [Header("UI Sound Events")]
    public EventReference pauseEvent;
    public EventReference confirmEvent;
    public EventReference hoverEvent;
    public EventReference errorEvent;
    public EventReference backEvent;

    public static UIAudioManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void PlayPause() => RuntimeManager.PlayOneShot(pauseEvent);
    public void PlayConfirm() => RuntimeManager.PlayOneShot(confirmEvent);
    public void PlayHover() => RuntimeManager.PlayOneShot(hoverEvent);
    public void PlayError() => RuntimeManager.PlayOneShot(errorEvent);
    public void PlayBack() => RuntimeManager.PlayOneShot(backEvent);
}