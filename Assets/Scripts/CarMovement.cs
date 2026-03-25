using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CarMovement : MonoBehaviour
{
    public float acceleration = 15f;
    public float brakeForce = 25f;
    public float maxSpeed = 20f;
    public float naturalDeceleration = 5f;
    public float reverseMaxSpeed = 8f;

    private InputAction pAcceleration;
    private InputAction pDeceleration;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = 0f;

        pAcceleration = InputSystem.actions.FindAction("acceleration");
        Debug.Log("Found acceleration");

        pDeceleration = InputSystem.actions.FindAction("deceleration");
        Debug.Log("Found deceleration");


        if (pAcceleration != null) pAcceleration.Enable();
        if (pDeceleration != null) pDeceleration.Enable();
    }

    void FixedUpdate()
    {
        float rightTrigger = 0f;
        float leftTrigger = 0f;

        if (pAcceleration != null)
        {
            rightTrigger = pAcceleration.ReadValue<float>();
            Debug.Log("Accelerating detected... ");

        }



        if (pDeceleration != null)
            leftTrigger = pDeceleration.ReadValue<float>();

        Vector3 forward = transform.forward;
        float currentSpeed = Vector3.Dot(rb.linearVelocity, forward);

        if (rightTrigger > 0.1f)
        {
            if (currentSpeed < maxSpeed)
            {
                rb.AddForce(forward * rightTrigger * acceleration, ForceMode.Acceleration);
                Debug.Log("Applying acceleration force...");
            }
        }

        if (leftTrigger > 0.1f)
        {
            if (currentSpeed > 0.1f)
            {
                rb.AddForce(-forward * leftTrigger * brakeForce, ForceMode.Acceleration);
                Debug.Log("Applying brake force...");
            }
            else if (currentSpeed > -reverseMaxSpeed)
            {
                rb.AddForce(-forward * leftTrigger * acceleration, ForceMode.Acceleration);
                Debug.Log("Applying reverse acceleration...");
            }
        }

        if (rightTrigger < 0.1f && leftTrigger < 0.1f)
        {
            if (Mathf.Abs(currentSpeed) > 0.1f)
            {
                Vector3 horizontalVelocity = Vector3.Project(rb.linearVelocity, forward);
                Vector3 decel = -horizontalVelocity.normalized * naturalDeceleration;
                rb.AddForce(decel, ForceMode.Acceleration);
            }
            else
            {
                rb.linearVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
            }
        }
    }

    void OnDisable()
    {
        if (pAcceleration != null) pAcceleration.Disable();
        if (pDeceleration != null) pDeceleration.Disable();
    }
}