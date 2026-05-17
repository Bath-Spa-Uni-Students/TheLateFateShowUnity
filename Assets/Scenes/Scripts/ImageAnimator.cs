using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class ImageAnimator : MonoBehaviour
{
    public Sprite[] frames; // Array of sprites for animation
    [SerializeField] private float spritePerFrame = 0;
    public bool loop = true;
    public bool destroyOnEnd = false;

    [SerializeField] private bool isSpriteRenderer = false;
    [SerializeField] private bool isImage = false;

    private int index = 0;
    private Image image;
    private SpriteRenderer spriteRenderer;
    private int frame = 0;
    private float timer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();

        if (spriteRenderer != null)
        {
            isSpriteRenderer = true;

        }
        else if (image != null)
        {
            isImage = true;
        }
    }

    private void Start()
    {
        if (isSpriteRenderer)
        {
            StartCoroutine(SpriteAnimation());
        }
        else if (isImage)
        {
            StartCoroutine(ImageAnimation());
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {

    }

    IEnumerator ImageAnimation()
    {
        float frameDuration = 1f / spritePerFrame; // Duration of each frame in seconds
        while (loop || index < frames.Length)
        {
            image.sprite = frames[index];
            index++;
            if (index >= frames.Length)
            {
                if (loop)
                {
                    index = 0;
                }
                else
                {
                    if (destroyOnEnd)
                    {
                        Destroy(gameObject);
                    }
                    break;
                }
            }

            yield return new WaitForSecondsRealtime(frameDuration);

        }
    }

    IEnumerator SpriteAnimation()
    {
        float frameDuration = 1f / spritePerFrame; // Duration of each frame in seconds
        while (loop || index < frames.Length)
        {
            spriteRenderer.sprite = frames[index];
            index++;
            if (index >= frames.Length)
            {
                if (loop)
                {
                    index = 0;
                }
                else
                {
                    if (destroyOnEnd)
                    {
                        Destroy(gameObject);
                    }
                    break;
                }
            }

            yield return new WaitForSecondsRealtime(frameDuration);
        }
    }
}
