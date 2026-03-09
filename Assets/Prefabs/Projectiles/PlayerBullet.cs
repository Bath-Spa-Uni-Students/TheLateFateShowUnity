using UnityEngine;
using UnityEngine.InputSystem;

public class Bullet : MonoBehaviour
{
    public float lifeTime = 3f;
    public float speed = 5f;


    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {

    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        Destroy(gameObject);
    }
}
