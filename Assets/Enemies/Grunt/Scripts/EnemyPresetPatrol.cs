using UnityEngine;

public class EnemyPresetPatrol : MonoBehaviour
{
    [SerializeField] public float speed;

    public Transform[] moveSpots;
    private int randomSpot;

    private float waitTime;
    [SerializeField] private float startWaitTime;
    
    void Start()
    {
        //Sets a random spot from the array to go to
        randomSpot = Random.Range(0, moveSpots.Length);
    }

    void Update()
    {
        //Moves to the random spot (delta time is used so it is not frames based
        transform.position = Vector2.MoveTowards(transform.position, moveSpots[randomSpot].position, speed * Time.deltaTime);
        
        //Checks if close to the spot - This is done to prevent exact checks
        if (Vector2.Distance(transform.position, moveSpots[randomSpot].position) < 0.2f)
        {
            //Timer to make enemy wait before moving to new spot
            if (waitTime <= 0)
            {
                //sets random spot and resets the timer
                randomSpot = Random.Range(0, moveSpots.Length);
                waitTime = startWaitTime;
            }
            else
            {
                waitTime -= Time.deltaTime;
            }
        }
    }
}
