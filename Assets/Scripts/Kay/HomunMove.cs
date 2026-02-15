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

            //// need to randomize direction if its too similar to previous one.

            //if (contact.normal.magnitude + 0.02f < -previousDir.normalized.magnitude && contact.normal.magnitude - 0.02f > -previousDir.normalized.magnitude)

            //if (contact.normal == -previousDir)
            //{
            //    Debug.Log("direction is like the exact same as last time.");
            //    Vector2 newRandomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));

            //    for(int i = 0; i < 10 && newRandomDir == Vector2.zero; i++) // make sure new random dir is not 0
            //    {
            //        newRandomDir = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            //        if (newRandomDir != Vector2.zero) break;
            //    }

            //    float newVelocityComparedToNormal = Vector2.Dot(newRandomDir.normalized, contact.normal);

            //    if (newVelocityComparedToNormal > 0.0f)
            //    {
            //        newRandomDir = -newRandomDir;
            //    }
            //    velocity = newRandomDir;
            //}
            //else velocity = contact.normal;

            //previousDir = velocity;

            velocity = contact.normal;
            rb.AddTorque(torque);
        }
    }
}
