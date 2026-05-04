using UnityEngine;
using UnityEngine.UI;

public class FameDisplayUpdater : MonoBehaviour
{
    public static FameDisplayUpdater Instance;

    public Image digit1; // Hundreds
    public Image digit2; // Tens
    public Image digit3; // Ones

    public Sprite[] numberSprites; // 0–9

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void UpdateFame(int score)
    {
        score = Mathf.Clamp(score, 0, 999);

        int hundreds = score / 100;
        int tens = (score / 10) % 10;
        int ones = score % 10;

        digit1.sprite = numberSprites[hundreds];
        digit2.sprite = numberSprites[tens];
        digit3.sprite = numberSprites[ones];
    }
    //testing method to add fame on key press
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            PlayerMovement player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
            player.AddFame(150);
            Debug.Log("Added 150 fame. Current fame: " + player.fame);
        }
    }

}