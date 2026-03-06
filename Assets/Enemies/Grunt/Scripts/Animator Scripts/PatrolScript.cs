using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class PatrolScript : StateMachineBehaviour
{
    [SerializeField] public float speed;

    private UnityEngine.Transform gruntTransform;
    public UnityEngine.Transform moveSpot;

    private float waitTime;
    [SerializeField] private float startWaitTime;

    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        {
            gruntTransform = animator.gameObject.transform;
            //Moves to the random spot (delta time is used so it is not frames based
            gruntTransform.position = Vector2.MoveTowards(gruntTransform.position, moveSpot.position, speed * Time.deltaTime);

            //Checks if close to the spot - This is done to prevent exact checks
            if (Vector2.Distance(gruntTransform.position, moveSpot.position) < 0.2f)
            {
                //Timer to make enemy wait before moving to new spot
                if (waitTime <= 0)
                {
                    //sets random spot and resets the timer
                    moveSpot.position = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
                    waitTime = startWaitTime;
                }
                else
                {
                    waitTime -= Time.deltaTime;
                }
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
