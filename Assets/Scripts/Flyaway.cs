using UnityEngine;

public class Flyaway : MonoBehaviour
{
    public Vector3 Velocity;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Velocity;
            //rb.linearVelocity = new Vector3(rb.linearVelocity.x, Velocity.y, rb.linearVelocity.z);
        }
        else {
            gameObject.transform.position = new Vector3(transform.position.x+(Time.deltaTime*Velocity.x),
                transform.position.y + (Time.deltaTime * Velocity.y)
                , (Time.deltaTime*transform.position.z));
        }
            

        //transform.position += Vector3.up * yVelocity * Time.deltaTime;


    }
}
