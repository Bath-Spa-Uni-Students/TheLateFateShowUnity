using System;
using UnityEngine;

public class TutorialEnemy : MonoBehaviour
{
    public event Action OnDeath;

    public void NotifyDeath()
    {
        OnDeath?.Invoke();// Notify any subscribers (like the spawner) that this enemy has died
        Debug.Log("TutorialEnemy: Enemy has died and notified subscribers.");
    }
}
