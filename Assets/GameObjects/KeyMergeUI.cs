using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeyMergeUI : MonoBehaviour
{
    [Header("Fragment Slots")]
    [SerializeField] private Image[] fragmentSlots;

    [Header("Completed Key")]
    [SerializeField] private Image completedKeyImage;

    [Header("Timings")]
    [SerializeField] private float fragmentFadeIn = 0.35f;
    [SerializeField] private float flyDuration = 0.4f;
    [SerializeField] private float holdDuration = 1.8f;
    [SerializeField] private float fadeDuration = 0.6f;

    [Header("Scale Punch")]
    [SerializeField] private float punchScale = 1.25f;
    [SerializeField] private float punchDuration = 0.18f;

    private Vector2[] slotOrigins;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        Debug.Log($"[KeyMergeUI] Awake — GameObject: {gameObject.name}, active: {gameObject.activeSelf}");

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Debug.Log("[KeyMergeUI] No CanvasGroup found — added one at runtime.");
        }
        else
        {
            Debug.Log($"[KeyMergeUI] Found existing CanvasGroup. alpha={canvasGroup.alpha}, interactable={canvasGroup.interactable}, blocksRaycasts={canvasGroup.blocksRaycasts}");
        }

        if (fragmentSlots == null || fragmentSlots.Length == 0)
        {
            Debug.LogError("[KeyMergeUI] fragmentSlots array is empty or null — assign Image references in the Inspector!");
            return;
        }

        slotOrigins = new Vector2[fragmentSlots.Length];

        for (int i = 0; i < fragmentSlots.Length; i++)
        {
            if (fragmentSlots[i] == null)
            {
                Debug.LogError($"[KeyMergeUI] fragmentSlots[{i}] is null — drag the Image into the Inspector slot!");
                continue;
            }

            slotOrigins[i] = fragmentSlots[i].rectTransform.anchoredPosition;

            // Log state BEFORE we touch alpha
            Debug.Log($"[KeyMergeUI] Slot {i} BEFORE SetAlpha: colour={fragmentSlots[i].color}, gameObject active={fragmentSlots[i].gameObject.activeSelf}, enabled={fragmentSlots[i].enabled}");

            SetAlpha(fragmentSlots[i], 0f);

            // Log state AFTER
            Debug.Log($"[KeyMergeUI] Slot {i} AFTER  SetAlpha: colour={fragmentSlots[i].color}");
        }
        if (completedKeyImage == null)
        {
            Debug.LogWarning("[KeyMergeUI] completedKeyImage is not assigned.");
        }
        else
        {
            SetAlpha(completedKeyImage, 0f);
            completedKeyImage.rectTransform.localScale = Vector3.one;
            Debug.Log($"[KeyMergeUI] completedKeyImage initialised. colour={completedKeyImage.color}");
        }
    }

    public void OnFragmentCollected(int keyIndex)
    {
        Debug.Log($"[KeyMergeUI] OnFragmentCollected called with keyIndex={keyIndex}");

        if (fragmentSlots == null || fragmentSlots.Length == 0)
        {
            Debug.LogError("[KeyMergeUI] fragmentSlots is empty — cannot fade in slot.");
            return;
        }

        if (keyIndex < 0 || keyIndex >= fragmentSlots.Length)
        {
            Debug.LogError($"[KeyMergeUI] keyIndex {keyIndex} out of range (fragmentSlots.Length={fragmentSlots.Length})");
            return;
        }

        if (fragmentSlots[keyIndex] == null)
        {
            Debug.LogError($"[KeyMergeUI] fragmentSlots[{keyIndex}] is null!");
            return;
        }

        Debug.Log($"[KeyMergeUI] Starting FadeInSlot coroutine for slot {keyIndex}. Current alpha={fragmentSlots[keyIndex].color.a}, gameObject active={gameObject.activeSelf}");
        StartCoroutine(FadeInSlot(fragmentSlots[keyIndex], keyIndex));
    }

    public void PlayMergeSequence()
    {
        Debug.Log("[KeyMergeUI] PlayMergeSequence called.");
        StartCoroutine(MergeSequence());
    }
    private IEnumerator FadeInSlot(Image slot, int debugIndex)
    {
        Debug.Log($"[KeyMergeUI] FadeInSlot [{debugIndex}] started. timeScale={Time.timeScale}");

        float elapsed = 0f;
        int frameCount = 0;

        while (elapsed < fragmentFadeIn)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsed / fragmentFadeIn);
            SetAlpha(slot, alpha);

            // Log every 10 frames so we can see if the loop is running
            if (frameCount % 10 == 0)
                Debug.Log($"[KeyMergeUI] FadeInSlot [{debugIndex}] frame={frameCount} elapsed={elapsed:F3} alpha={alpha:F3} slot.colour={slot.color}");

            frameCount++;
            yield return null;
        }

        SetAlpha(slot, 1f);
        Debug.Log($"[KeyMergeUI] FadeInSlot [{debugIndex}] complete. Final colour={slot.color}");
    }

    private IEnumerator MergeSequence()
    {
        Debug.Log("[KeyMergeUI] MergeSequence started — waiting for last fade to finish.");
        yield return new WaitForSecondsRealtime(fragmentFadeIn + 0.1f);

        Debug.Log("[KeyMergeUI] MergeSequence — beginning fly-to-centre.");
        Vector2 centre = Vector2.zero;
        float elapsed = 0f;

        while (elapsed < flyDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / flyDuration);

            for (int i = 0; i < fragmentSlots.Length; i++)
            {
                if (fragmentSlots[i] == null) continue;
                fragmentSlots[i].rectTransform.anchoredPosition = Vector2.Lerp(slotOrigins[i], centre, t);
                SetAlpha(fragmentSlots[i], Mathf.Lerp(1f, 0f, t));
            }

            yield return null;
        }

        for (int i = 0; i < fragmentSlots.Length; i++)
        {
            if (fragmentSlots[i] == null) continue;
            fragmentSlots[i].rectTransform.anchoredPosition = centre;
            SetAlpha(fragmentSlots[i], 0f);
        }

        Debug.Log("[KeyMergeUI] MergeSequence — fly complete, popping completed key.");

        if (completedKeyImage != null)
        {
            SetAlpha(completedKeyImage, 1f);
            Debug.Log($"[KeyMergeUI] completedKeyImage shown. colour={completedKeyImage.color}");
            yield return StartCoroutine(ScalePunch(completedKeyImage.rectTransform));
        }
        else
        {
            Debug.LogWarning("[KeyMergeUI] completedKeyImage is null — skipping pop.");
        }

        Debug.Log($"[KeyMergeUI] Holding for {holdDuration}s.");
        yield return new WaitForSecondsRealtime(holdDuration);
    }

    private IEnumerator ScalePunch(RectTransform rt)
    {
        float half = punchDuration * 0.5f;
        float elapsed = 0f;

        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            rt.localScale = Vector3.Lerp(Vector3.one, Vector3.one * punchScale, elapsed / half);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            rt.localScale = Vector3.Lerp(Vector3.one * punchScale, Vector3.one, elapsed / half);
            yield return null;
        }

        rt.localScale = Vector3.one;
    }

    private void SetAlpha(Image image, float alpha)
    {
        if (image == null) return;
        Color c = image.color;
        c.a = alpha;
        image.color = c;
        Debug.Log($"[KeyMergeUI] SetAlpha {image.gameObject.name} = {alpha:F3} | confirmed: {image.color.a:F3}");
    }
}