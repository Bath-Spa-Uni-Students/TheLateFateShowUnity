using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    private float speed;
    [SerializeField] private EnemyBehaviour enemyBehaviour;

    public Transform moveSpot;

    private float waitTime;
    [SerializeField] private float startWaitTime;

    [SerializeField] private float minX;
    [SerializeField] private float maxX;
    [SerializeField] private float minY;
    [SerializeField] private float maxY;
    
    void Start()
    {
        //speed = enemyBehaviour.speed;
        waitTime = startWaitTime;
        //Sets a random spot from given x,y values
        moveSpot.position = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
    }

    void Update()
    {

    }

    public void Patrol()
    {
        //Moves to the random spot (delta time is used so it is not frames based
        transform.position = Vector2.MoveTowards(transform.position, moveSpot.position, speed * Time.deltaTime);

        //Checks if close to the spot - This is done to prevent exact checks
        if (Vector2.Distance(transform.position, moveSpot.position) < 0.2f)
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
