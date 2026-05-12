using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ButtonImageChange : MonoBehaviour
{
    [SerializeField] private float changeDuration = 2f; // Duration to show the pressed image

    [SerializeField] private Button button;
    [SerializeField] private Sprite buttonImage;
    [SerializeField] private Sprite pressedImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeImage()
    {
        StartCoroutine(ImageChange());
    }

    IEnumerator ImageChange()
    {
        button.image.sprite = pressedImage;
        yield return new WaitForSeconds(changeDuration);
        button.image.sprite = buttonImage;
    }
}
