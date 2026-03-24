using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private Vector3 playerTarget;
    private Vector3 randomTarget;
    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform.position;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    void SetPlayerTargetPosition()
    {
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform.position;
    }

    void SetRandomTargetPosition()
    {
        
    }

    void SetAgentPosition(Vector3 target)
    {
        agent.SetDestination(new Vector3(target.x, target.y, transform.position.z));
    }
}
