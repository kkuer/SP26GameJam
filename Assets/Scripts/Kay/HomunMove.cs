using UnityEngine;

public class HomunMove : MonoBehaviour
{
    public Vector2 startDir;
    public Vector2 velocity;

    private Vector2 previousDir;
    
    [SerializeField]private float maxSpeed = 10;
    [SerializeField]private float minSpeed;
    public float CurrentSpeed = 0;

    public float torque;

    private float acceleration;
    private float deceleration;

    private Rigidbody2D rb;

    void Start()
    {
        rb=GetComponent<Rigidbody2D>();

        CurrentSpeed = maxSpeed;
        velocity = startDir * CurrentSpeed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
          Movement(CurrentSpeed);
    }

    void Movement(float speed)
    {
        velocity = velocity.normalized * speed;

        rb.linearVelocity = velocity;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        foreach (var contact in other.contacts) // getting surface normal of collision 
        {
            Debug.DrawRay(contact.point, contact.normal, Color.red, 2f);

            velocity = contact.normal;

            previousDir = velocity;
            rb.AddTorque(torque);
        }
    }
}
