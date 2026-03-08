using UnityEngine;
using UnityEngine.UIElements;

public class EnemyMoveSpot : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    private GameObject myMoveSpot;
    public Position moveSpotPosition;
   // private bool overlapping;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       // myMoveSpot = enemy.GetComponent<EnemyBehaviour>().moveSpot;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        /*if (myMoveSpot.GetComponent<BoxCollider2D>().IsTouchingLayers(11))
        {
            enemy.GetComponent<EnemyBehaviour>().overlappingCollider = true;
            enemy.GetComponent<EnemyBehaviour>().SetMoveSpot();
            //SetMoveSpot();
            Debug.Log("Overlapped");
        }*/
    }

    /*private void SetMoveSpot()
    {
        moveSpot.transform.position = new Vector2(UnityEngine.Random.Range
(minX.position.x, maxX.position.x), UnityEngine.Random.Range(minY.position.y, maxY.position.y));

        while (overlapping)
        {
            moveSpot.transform.position = new Vector2(UnityEngine.Random.Range
(minX.position.x, maxX.position.x), UnityEngine.Random.Range(minY.position.y, maxY.position.y));
            if (moveSpot.GetComponent<BoxCollider2D>().IsTouchingLayers(11))
            {
                overlapping = true;
                Debug.Log(overlapping);
            }
            else
            {
                overlapping = false;
                Debug.Log(overlapping);
                break;
            }
        }
    }*/
}
