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
    public float maxSteerAngle = 30f;
    public float turnSpeed = 120f;
    public float steeringStrength = 2.5f;
    public float minTurnSpeed = 0.5f;

    [Header("VR Steering Wheel")]
    public Transform SteeringWheel;
    public float steeringWheelMaxRotation = 180f;
    public bool invertSteeringWheel = false;

    [Header("Wheel Visuals")]
    public Transform Wheel_FL;
    public Transform Wheel_FR;

    private InputAction pAcceleration;
    private InputAction pDeceleration;

    private Rigidbody rb;
    private float currentSteerAngle = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearDamping = 0f;

        pAcceleration = InputSystem.actions.FindAction("acceleration");
        pDeceleration = InputSystem.actions.FindAction("deceleration");

        if (pAcceleration != null) pAcceleration.Enable();
        if (pDeceleration != null) pDeceleration.Enable();
    }

    void FixedUpdate()
    {
        float rightTrigger = 0f;
        float leftTrigger = 0f;

        if (pAcceleration != null)
            rightTrigger = pAcceleration.ReadValue<float>();

        if (pDeceleration != null)
            leftTrigger = pDeceleration.ReadValue<float>();

        float steerInput = GetSteeringWheelInput();

        Vector3 forward = transform.forward;
        float currentSpeed = Vector3.Dot(rb.linearVelocity, forward);

        HandleDrive(forward, currentSpeed, rightTrigger, leftTrigger);
        HandleSteering(steerInput, currentSpeed);

        Debug.Log($"Wheel Steer Input: {steerInput}");
    }

    private float GetSteeringWheelInput()
    {
        if (SteeringWheel == null)
            return 0f;

        float wheelAngle = SteeringWheel.localEulerAngles.z;

        if (wheelAngle > 180f)
            wheelAngle -= 360f;

        float steerInput = Mathf.Clamp(
            wheelAngle / steeringWheelMaxRotation,
            -1f,
            1f
        );

        if (invertSteeringWheel)
            steerInput *= -1f;

        return steerInput;
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
        float targetSteerAngle = steerInput * maxSteerAngle;

        currentSteerAngle = Mathf.MoveTowards(
            currentSteerAngle,
            targetSteerAngle,
            turnSpeed * Time.fixedDeltaTime
        );

        if (Wheel_FL != null)
        {
            Wheel_FL.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
        }

        if (Wheel_FR != null)
        {
            Wheel_FR.localRotation = Quaternion.Euler(0f, currentSteerAngle, 0f);
        }

        if (Mathf.Abs(currentSpeed) > minTurnSpeed)
        {
            float speedFactor = Mathf.Clamp01(Mathf.Abs(currentSpeed) / maxSpeed);
            float turnAmount = currentSteerAngle * steeringStrength * speedFactor * Time.fixedDeltaTime;

            if (currentSpeed >= 0f)
            {
                rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, turnAmount, 0f));
            }
            else
            {
                rb.MoveRotation(rb.rotation * Quaternion.Euler(0f, -turnAmount, 0f));
            }
        }
    }

    void OnDisable()
    {
        if (pAcceleration != null) pAcceleration.Disable();
        if (pDeceleration != null) pDeceleration.Disable();
    }
}