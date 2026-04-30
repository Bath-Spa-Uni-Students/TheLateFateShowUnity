using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] popUps;
    private int popUpIndex = 0;

    private void Update()
    {
        for (int i = 0; i < popUps.Length; i++)
        {
            if (i == popUpIndex)
            {
                popUps[i].SetActive(true);
            }
            else
            {
                popUps[i].SetActive(false);
            }
        }
        if (popUpIndex == 0)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.D))
                popUpIndex++;
        }
        else if (popUpIndex == 1)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift))
                popUpIndex++;

        }
        else if (popUpIndex == 2)
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
                popUpIndex++;
        }
        else if (popUpIndex == 3)
        {
            if (Input.GetKeyDown(KeyCode.R))
                popUpIndex++;
        }
        else if (popUpIndex == 4)
        {
            //if (player level ==1){
            popUpIndex++;
        }
        else if (popUpIndex == 5)
        {
            //if (all enemies defeated){
            popUpIndex++;
        }
        else if (popUpIndex == 6)
        {
            //teleport player to maze
        }
    }
}
