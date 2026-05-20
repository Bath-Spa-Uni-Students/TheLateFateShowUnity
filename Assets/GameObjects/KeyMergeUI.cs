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
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        if (fragmentSlots == null || fragmentSlots.Length == 0)
        {
            return;
        }

        slotOrigins = new Vector2[fragmentSlots.Length];

        for (int i = 0; i < fragmentSlots.Length; i++)
        {
            if (fragmentSlots[i] == null)
            {
                continue;
            }

            slotOrigins[i] = fragmentSlots[i].rectTransform.anchoredPosition;

           
            SetAlpha(fragmentSlots[i], 0f);
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

        if (fragmentSlots == null || fragmentSlots.Length == 0)
        {
            return;
        }
        if (keyIndex < 0 || keyIndex >= fragmentSlots.Length)
        {
            return;
        }

        if (fragmentSlots[keyIndex] == null)
        {
            return;
        }

      StartCoroutine(FadeInSlot(fragmentSlots[keyIndex], keyIndex));
    }

    public void PlayMergeSequence()
    {

        StartCoroutine(MergeSequence());
    }
    private IEnumerator FadeInSlot(Image slot, int debugIndex)
    {

        float elapsed = 0f;
        int frameCount = 0;

        while (elapsed < fragmentFadeIn)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsed / fragmentFadeIn);
            SetAlpha(slot, alpha);

            // Log every 10 frames so we can see if the loop is running
            if (frameCount % 10 == 0)
               
            frameCount++;
            yield return null;
        }

        SetAlpha(slot, 1f);
          }

    private IEnumerator MergeSequence()
    {
         yield return new WaitForSecondsRealtime(fragmentFadeIn + 0.1f);

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

       
        if (completedKeyImage != null)
        {
            AudioManager.Instance.PlayOneShot(FMODEvents.Instance.keyMerge, Vector3.zero);
            SetAlpha(completedKeyImage, 1f);
            yield return StartCoroutine(ScalePunch(completedKeyImage.rectTransform));
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
    }
}