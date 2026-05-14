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

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!loop && index >= frames.Length) return;
        frame ++;
        if (frame < spritePerFrame) return;
        image.sprite = frames [index];
        frame = 0;
        index++;
        if (index >= frames.Length)
        {
            if (loop) index = 0;
            if(destroyOnEnd) Destroy(gameObject);
        }
    }
}
