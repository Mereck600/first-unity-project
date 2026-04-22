using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class CarMovement : MonoBehaviour
{
    [Header("Drive")]
    public float acceleration = 15f;
    public float brakeForce = 25f;
    public float maxSpeed = 20f;
    public float naturalDeceleration = 5f;
    public float reverseMaxSpeed = 8f;

    [Header("Steering")]
    public float maxSteerAngle = 30f;      // visual wheel angle in degrees
    public float turnSpeed = 120f;         // how fast wheels rotate toward target angle
    public float steeringStrength = 2.5f;  // how strongly car body turns
    public float minTurnSpeed = 0.5f;      // car must be moving a little before turning

    [Header("Wheel Visuals")]
    public Transform Wheel_FL;
    public Transform Wheel_FR;

    private InputAction pAcceleration;
    private InputAction pDeceleration;
    private InputAction pTurnLeft;
    private InputAction pTurnRight;

    private Rigidbody rb;

    // current visual wheel steering angle
    private float currentSteerAngle = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = 0f;

        pAcceleration = InputSystem.actions.FindAction("acceleration");
        pDeceleration = InputSystem.actions.FindAction("deceleration");
        pTurnLeft = InputSystem.actions.FindAction("turnLeft");
        pTurnRight = InputSystem.actions.FindAction("turnRight");

        if (pAcceleration != null) pAcceleration.Enable();
        if (pDeceleration != null) pDeceleration.Enable();
        if (pTurnLeft != null) pTurnLeft.Enable();
        if (pTurnRight != null) pTurnRight.Enable();
    }

    void FixedUpdate()
    {
        float rightTrigger = 0f;
        float leftTrigger = 0f;
        float leftInput = 0f;
        float rightInput = 0f;

        if (pAcceleration != null)
            rightTrigger = pAcceleration.ReadValue<float>();

        if (pDeceleration != null)
            leftTrigger = pDeceleration.ReadValue<float>();

        if (pTurnLeft != null)
            leftInput = pTurnLeft.ReadValue<float>();

        if (pTurnRight != null)
            rightInput = pTurnRight.ReadValue<float>();

        // Steering input:
        // turnLeft pushes negative, turnRight pushes positive
        float steerInput = Mathf.Clamp(rightInput - leftInput, -1f, 1f);

        Vector3 forward = transform.forward;
        float currentSpeed = Vector3.Dot(rb.linearVelocity, forward);

        HandleDrive(forward, currentSpeed, rightTrigger, leftTrigger);
        HandleSteering(steerInput, currentSpeed);
        Debug.Log($"Left: {leftInput}, Right: {rightInput}, Steer: {steerInput}");
    }

    private void HandleDrive(Vector3 forward, float currentSpeed, float rightTrigger, float leftTrigger)
    {
        if (rightTrigger > 0.1f)
        {
            if (currentSpeed < maxSpeed)
            {
                rb.AddForce(forward * rightTrigger * acceleration, ForceMode.Acceleration);
            }
        }

        if (leftTrigger > 0.1f)
        {
            if (currentSpeed > 0.1f)
            {
                rb.AddForce(-forward * leftTrigger * brakeForce, ForceMode.Acceleration);
            }
            else if (currentSpeed > -reverseMaxSpeed)
            {
                rb.AddForce(-forward * leftTrigger * acceleration, ForceMode.Acceleration);
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

    private void HandleSteering(float steerInput, float currentSpeed)
    {
        // Target angle for the front wheels
        float targetSteerAngle = steerInput * maxSteerAngle;

        // Smoothly move wheels toward the target angle
        currentSteerAngle = Mathf.MoveTowards(
            currentSteerAngle,
            targetSteerAngle,
            turnSpeed * Time.fixedDeltaTime
        );

        // Apply the visual wheel rotation
        if (Wheel_FL != null)
        {
            Wheel_FL.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
            //Debug.Log($"Steer Input: {steerInput}, Target Angle: {targetSteerAngle}, Current Angle: {currentSteerAngle}");

        }

        if (Wheel_FR != null)
        {
            Wheel_FR.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
            //Debug.Log($"Steer Input: {steerInput}, Target Angle: {targetSteerAngle}, Current Angle: {currentSteerAngle}");

        }
        

        // Turn the car body only if moving enough
        if (Mathf.Abs(currentSpeed) > minTurnSpeed)
        {
            // More speed = more turning response
            // Reverse flips steering direction naturally
            float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
            float turnAmount = currentSteerAngle * steeringStrength * speedFactor * Time.fixedDeltaTime;

            if (currentSpeed >= 0f)
            {
                rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turnAmount, 0f));
                Debug.Log($"Steer Input: {steerInput}, Target Angle: {targetSteerAngle}, Current Angle: {currentSteerAngle}");


            }
            else
                rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, -turnAmount, 0f));
        }
    }

    void OnDisable()
    {
        if (pAcceleration != null) pAcceleration.Disable();
        if (pDeceleration != null) pDeceleration.Disable();
        if (pTurnLeft != null) pTurnLeft.Disable();
        if (pTurnRight != null) pTurnRight.Disable();
    }
}