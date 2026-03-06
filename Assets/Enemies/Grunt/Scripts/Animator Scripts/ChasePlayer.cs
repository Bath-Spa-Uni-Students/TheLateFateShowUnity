using UnityEngine;

public class ChasePlayer : StateMachineBehaviour
{
    // Info Of Player
    private Transform player;

    // Enemy Stats
    [SerializeField] private float gruntSpeed;
    [SerializeField] private Transform target;
    [SerializeField] private float stoppingDistance = 0.5f;

    // Enemy Weapon Stats
    [SerializeField] private float fireRate;
    [SerializeField] private GameObject projectile;
    private float fireTimer;

    // Radius around Enemy to find Player
    [SerializeField] private float detectionRadius;
    [SerializeField] private GameObject detectionCircle;

    // Finding Collision when moving
    [SerializeField] private float obstacleRadiusCheck;
    [SerializeField] private float obstacleCheckDistance;
    private UnityEngine.Transform gruntTransform;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {

        detectionRadius = animator.gameObject.GetComponent<CircleCollider2D>().radius;
        detectionCircle.transform.localScale = new Vector3(detectionRadius * 2, detectionRadius * 2, detectionRadius * 2);
        // Sets target as player by making sure the target has player tag
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();

        fireTimer = fireRate;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        gruntTransform = animator.gameObject.transform;
        //Checks if player is in detection radius
        if (Vector2.Distance(gruntTransform.position, target.position) < detectionRadius)
        {
            //Checks if enemy is too close to player
            if (Vector2.Distance(gruntTransform.position, target.position) > stoppingDistance)
            {
                // Moves Enemy Character From Their Position to Target Position at set speed
                // Delta Time was chosen so the enemy speed isn't faster or slower depending on FPS
                gruntTransform.position = Vector2.MoveTowards(gruntTransform.position, target.position, gruntSpeed * Time.deltaTime);
            }

            //Shooting Player Code
            if (fireTimer <= 0)
            {
                //spawns bullet and does firerate timer
                Instantiate(projectile, gruntTransform.position, Quaternion.identity);
                fireTimer = fireRate;
            }
            else
            {
                fireTimer -= Time.deltaTime;
            }
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
