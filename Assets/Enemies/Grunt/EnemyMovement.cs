using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private Transform target;
    [SerializeField] private float stoppingDistance = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Sets target as player by making sure the target has player tag
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        //Checks if enemy is too close to player
        if (Vector2.Distance(transform.position, target.position) > stoppingDistance)
        {
            // Moves Enemy Character From Their Position to Target Position at set speed
            // Delta Time was chosen so the enemy speed isn't faster or slower depending on FPS
            transform.position = Vector2.MoveTowards(transform.position, target.position, speed * Time.deltaTime);
        }
    }
}
