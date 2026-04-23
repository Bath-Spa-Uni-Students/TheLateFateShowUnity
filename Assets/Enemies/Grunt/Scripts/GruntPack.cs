using System.Collections.Generic;
using UnityEngine;

public class GruntPack : MonoBehaviour
{
    [Header("Pack Setup")]
    public EnemyBehaviour leader;
    public List<EnemyBehaviour> followers = new List<EnemyBehaviour>();
    public float followDistance = 1.5f;

    void Start()
    {
        if (leader == null)
        {
            Debug.LogWarning("GruntPack has no leader assigned!");
            return;
        }

        leader.isLeader = true;

        foreach (var follower in followers)
        {
            if (follower == null) continue;

            follower.isLeader = false;
            follower.leader = leader.transform;
            follower.followOffset = Random.insideUnitCircle * followDistance;
        }
    }

    public void LeaderDied()
    {
        foreach (var follower in followers)
        {
            if (follower != null)
                follower.LeaderDied();
        }
    }
}