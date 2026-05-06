using System;
using UnityEngine;

public class TutorialEnemy : MonoBehaviour
{
    public event Action OnDeath;

    public void NotifyDeath()
    {
        OnDeath?.Invoke();
    }
}
