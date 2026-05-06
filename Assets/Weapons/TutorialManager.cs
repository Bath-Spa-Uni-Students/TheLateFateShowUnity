using UnityEngine;
using TMPro;
using System.Collections;
using UnityEditor.IMGUI.Controls;
public class TutorialManager : MonoBehaviour
{
    public GameObject[] popUps;
    private int popUpIndex = 0;
    private bool stepCompleted = false;

    [Header("Host")]
    public Animator hostAnimator;
    public TMP_Text dialougeText;
    public GameObject dialougeBox;

    public float typeSpeeed = 0.05f;

    public PlayerMovement playerStats;
    public SpawnManager spawnManager;

    private Coroutine typeRoutine;

    private static readonly string[] dialougelines = new string[]
  {
        "Use WASD to move around.",
        "Hold Shift to sprint.",
        "Left Click to shoot.",
        "Press R to reload.",
        "You've leveled up! Choose a perk to enhance your weapon.",
        "Defeat all enemies to progress.",
        "Good job! Now, let's move on to the maze.",
  };

    //Animations
    private static readonly int animTalk = Animator.StringToHash("Talk");
    private static readonly int animIdle = Animator.StringToHash("Idle");
    private static readonly int animEcstatic = Animator.StringToHash("Ecstatic");

    private void Start()
    {
        ShowStep(0);
    }

    private void Update()
    {

        if (stepCompleted) return;

        switch (popUpIndex)
        {
            case 0://Movement tutorial
                if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                {
                    AdvanceStep();
                }
                break;
            case 1://Sprint tutorial
                if (Input.GetKey(KeyCode.LeftShift))
                {
                    AdvanceStep();
                }
                break;
            case 2://Shooting tutorial
                if (Input.GetMouseButtonDown(0))
                {
                    AdvanceStep();
                }
                break;
            case 3://Reload tutorial
                if (Input.GetKeyDown(KeyCode.R))
                {
                    AdvanceStep();
                }
                break;
            case 4://Perk selection tutorial
                //if (//reference perk slots)
                {
                    AdvanceStep();
                }
                break;
            case 5://Enemy defeat tutorial
                // if(spawnManager.AllTutorialEnemiesDefeated())
                {
                    AdvanceStep();
                }
                break;
            case 6:
                TeleportPlayerToMaze();
                break;
        }
    }
    private void AdvanceStep()
    {
       popUpIndex++;
        if(popUpIndex < popUps.Length)
        {
            ShowStep(popUpIndex);
        }
        else
        {
            //end tutorial
             dialougeBox.SetActive(false);
             foreach (var popUp in popUps)
                 popUp.SetActive(false);
        }
      
    }

    private void ShowStep(int index)
    {
        for (int i = 0; i < popUps.Length; i++)
            popUps[i].SetActive(i == index);

        if (typeRoutine != null)
            StopCoroutine(typeRoutine);

        string line = index < dialougelines.Length ? dialougelines[index] : "No more instructions.";

        typeRoutine = StartCoroutine(TypeWriterRoutine(line, index));


    }

    //Anims and Text for host character during dialouge
    private IEnumerator TypeWriterRoutine(string line,int stepIndex)
    {
        stepCompleted = true;
        dialougeBox.SetActive(true);
        dialougeText.text = "";

        PlayHostAnimation(stepIndex);

        foreach (char c in line)
        {
            dialougeText.text += c;
            yield return new WaitForSeconds(typeSpeeed);
        }

        hostAnimator.CrossFade(animIdle, 0.2f);
        stepCompleted = false;
    }

    private void PlayHostAnimation(int stepIndex)
    {
      int anim =(stepIndex == 4 || stepIndex == 6) ? animEcstatic : animTalk;
        hostAnimator.CrossFade(anim, 0.2f);
    }
    private void TeleportPlayerToMaze()
    {
        // Teleport player to maze entrance
        playerStats.transform.position = new Vector3(50, 0, 0); // Example position
        dialougeBox.SetActive(false);
    }
}
