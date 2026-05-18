using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;
using Image = UnityEngine.UI.Image;

public class ImageAnimator : MonoBehaviour
{
    public Sprite[] frames; // Array of sprites for animation
    public int spritePerFrame = 0;
    public bool loop = true;
    public bool destroyOnEnd = false;

    private int index = 0;
    private Image image;
    private int frame = 0;
    private float timer = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!loop && index >= frames.Length) return;

        timer += Time.unscaledDeltaTime; // Use unscaledDeltaTime to ignore time scale changes

        float frameDuration = 1f / spritePerFrame; // Duration of each frame in seconds

        while (timer >= frameDuration)
        {
            timer -= frameDuration;

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
        }
    }
}
