using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    private Vector3 target;
    NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        target = GameObject.FindGameObjectWithTag("Player").transform.position;
    }


    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    void SetTargetPosition()
    {
        target = GameObject.FindGameObjectWithTag("Player").transform.position;
    }
}
