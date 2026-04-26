using UnityEngine;

public class DebugControls : MonoBehaviour
{
    [SerializeField] private MazeChanger mazeChanger;

    void Update()
    {
        // Press F1 to force a room switch
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("Debug: Forcing room switch");
            mazeChanger.ForceSwitch();
        }
    }
}