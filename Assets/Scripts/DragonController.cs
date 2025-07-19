using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [Header("Flight Controls")]
    public float forwardSpeed = 10f;
    public float turnSmoothSpeed = 5f;
    public float pitchSmoothSpeed = 5f;
    public float bankSmoothSpeed = 5f;
    public float rotationSpeed = 90f; // degrees per second
    public float pitchSpeed = 60f; // degrees per second for pitching
    public float maxPitchAngle = 45f; // maximum pitch angle in degrees
    public float bankAngle = 20f; // degrees for banking effect
    public float upwardMovementMult = 0.5f; // Multiplier for upward movement speed
    public float downwardMovementMult = 1.5f; // Multiplier for downward movement speed

    private float currentFlyUp = 0f;
    private float currentFlyRight = 0f;
    private float currentBankAngle = 0f;
    private float currentPitchAngle = 0f;

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float cameraBankCounterRotation = 1f;

    [Header("Simulation Controls")]
    public bool simulateInput = false;
    public float simulatedFlyUp = 0f;
    public float simulatedFlyRight = -1f;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Animator component
        animator = GetComponent<Animator>();

        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleFlightInput();
    }

    void FixedUpdate()
    {
        HandleForwardMovement();
    }

    void HandleFlightInput()
    {
        // Get WASD input
        float verticalInput = 0f;
        float horizontalInput = 0f;

        // W/S for up/down (FlyUp parameter)
        if (Input.GetKey(KeyCode.W))
            verticalInput = 1f;  // Fly up
        else if (Input.GetKey(KeyCode.S))
            verticalInput = -1f; // Fly down

        // A/D for left/right (FlyRight parameter)
        if (Input.GetKey(KeyCode.D))
            horizontalInput = 1f;  // Fly right
        else if (Input.GetKey(KeyCode.A))
            horizontalInput = -1f; // Fly left

        // Simulate input if enabled
        if (simulateInput)
        {
            verticalInput = simulatedFlyUp;
            horizontalInput = simulatedFlyRight;
        }

        // Smoothly interpolate to target values
        currentFlyUp = Mathf.Lerp(currentFlyUp, verticalInput, pitchSmoothSpeed * Time.deltaTime);
        currentFlyRight = Mathf.Lerp(currentFlyRight, horizontalInput, turnSmoothSpeed * Time.deltaTime);

        // Set the animator parameters
        animator.SetFloat("FlyUp", currentFlyUp);
        animator.SetFloat("FlyRight", currentFlyRight);

        // Handle pitching (up/down rotation) with angle limits
        float pitchAmount = -currentFlyUp * pitchSpeed * Time.deltaTime; // Negative for intuitive controls
        float newPitchAngle = currentPitchAngle + pitchAmount;
        
        // Clamp the pitch angle to the maximum limits
        newPitchAngle = Mathf.Clamp(newPitchAngle, -maxPitchAngle, maxPitchAngle);
        
        // Only apply the rotation if it's within limits
        float actualPitchAmount = newPitchAngle - currentPitchAngle;
        if (Mathf.Abs(actualPitchAmount) > 0.001f) // Small threshold to avoid floating point errors
        {
            transform.Rotate(actualPitchAmount, 0f, 0f, Space.Self);
            currentPitchAngle = newPitchAngle;
        }

        // Handle turning
        float turnAmount = currentFlyRight * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, turnAmount, 0f, Space.World);

        // Handle banking
        float targetBankAngle = -currentFlyRight * bankAngle;
        currentBankAngle = Mathf.LerpAngle(currentBankAngle, targetBankAngle, bankSmoothSpeed * Time.deltaTime);
        transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, transform.localEulerAngles.y, currentBankAngle);

        // Rotate the camera based on banking
        float cameraRotationZ = currentBankAngle * -cameraBankCounterRotation;
        cameraTransform.localEulerAngles = new Vector3(cameraTransform.localEulerAngles.x, cameraTransform.localEulerAngles.y, cameraRotationZ);
    }

    void HandleForwardMovement()
    {
        if (rb != null)
        {
            // Calculate speed multiplier based on dragon's pitch angle proportional to max angle
            float speedMultiplier = 1f;
            
            if (currentPitchAngle < 0) // Pitched upward (negative angle due to negative pitchAmount)
            {
                // Interpolate between 1.0 and upwardMovementMult based on pitch angle
                float pitchRatio = Mathf.Abs(currentPitchAngle) / maxPitchAngle;
                speedMultiplier = Mathf.Lerp(1f, upwardMovementMult, pitchRatio);
            }
            else if (currentPitchAngle > 0) // Pitched downward (positive angle)
            {
                // Interpolate between 1.0 and downwardMovementMult based on pitch angle
                float pitchRatio = currentPitchAngle / maxPitchAngle;
                speedMultiplier = Mathf.Lerp(1f, downwardMovementMult, pitchRatio);
            }

            // Move forward in the direction the dragon is facing with adjusted speed
            Vector3 forwardMovement = transform.forward * forwardSpeed * speedMultiplier;

            // Apply the movement
            rb.velocity = forwardMovement;
        }
    }
}
