using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class KeyMergeUI : MonoBehaviour
{

    [Header("Fragment Slots")]
    [SerializeField] private Image[] fragmentSlots;

    [Header("Completed Key")]
    [SerializeField] private Image completedKeyImage;

    [Header("Fragment Fade-In")]
    [Tooltip("How long a single fragment takes to fade in when collected.")]
    [SerializeField] private float fragmentFadeIn = 0.35f;

    private Vector2[] slotOrigins;
    private CanvasGroup canvasGroup;


    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Record starting positions and hide all fragments.
        slotOrigins = new Vector2[fragmentSlots.Length];
        for (int i = 0; i < fragmentSlots.Length; i++)
        {
            slotOrigins[i] = fragmentSlots[i].rectTransform.anchoredPosition;
            SetAlpha(fragmentSlots[i], 0f);
        }

        if (completedKeyImage != null)
        {
            SetAlpha(completedKeyImage, 0f);
            completedKeyImage.rectTransform.localScale = Vector3.one;
        }
    }

    public void OnFragmentCollected(int collectedCount, int totalCount)
    {
        int slotIndex = collectedCount - 1;

        if (slotIndex >= 0 && slotIndex < fragmentSlots.Length)
            StartCoroutine(FadeInFragment(fragmentSlots[slotIndex]));

        if (collectedCount >= totalCount) ;
            //StartCoroutine(PlayMergeSequence());
    }


    private IEnumerator FadeInFragment(Image slot)
    {
        float elapsed = 0f;
        while (elapsed < fragmentFadeIn)
        {
            elapsed += Time.deltaTime;
            SetAlpha(slot, Mathf.Clamp01(elapsed / fragmentFadeIn));
            yield return null;
        }
        SetAlpha(slot, 1f);
    }

    //private IEnumerator PlayMergeSequence()
   

    //private IEnumerator ScalePunch(RectTransform rt)
 
    private void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;
        Color c = graphic.color;
        c.a = alpha;
        graphic.color = c;
    }
}