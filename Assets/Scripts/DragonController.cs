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
    public float animatorPitchEffortSpeed = 8f; // Speed for animator pitch effort easing
    public float cameraPitchEffortSpeed = 8f; // Speed for camera pitch effort easing

    private float currentFlyUp = 0f;
    private float currentFlyRight = 0f;
    private float currentBankAngle = 0f;
    private float currentPitchAngle = 0f;
    private float currentAnimatorPitchEffort = 0f; // Represents dragon's effort for animation
    private float currentCameraPitchEffort = 0f; // Represents dragon's effort for camera

    [Header("Camera Settings")]
    public Transform cameraTransform;
    public float cameraBankCounterRotation = 1f;
    public float cameraPitchMultiplier = 0.3f; // How much the camera pitches with dragon movement

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

        // Handle pitch effort (for animation and camera) - separate from actual rotation
        // Calculate how much effort the dragon should show based on input and current pitch limits
        float targetPitchEffort = verticalInput;
        
        // Reduce effort as we approach pitch angle limits
        if (Mathf.Abs(currentPitchAngle) > maxPitchAngle * 0.25f) // Start reducing effort at 25% of max angle
        {
            float effortPitchRatio = Mathf.Abs(currentPitchAngle) / maxPitchAngle;
            float effortReduction = 1f - ((effortPitchRatio - 0.25f) / 0.75f); // Linear reduction from 25% to 100%
            effortReduction = Mathf.Clamp01(effortReduction);
            
            // Only reduce effort when moving toward the limit
            if (Mathf.Sign(verticalInput) == Mathf.Sign(-currentPitchAngle)) // Note: currentPitchAngle is inverted
            {
                targetPitchEffort *= effortReduction;
            }
        }
        
        currentAnimatorPitchEffort = Mathf.Lerp(currentAnimatorPitchEffort, targetPitchEffort, animatorPitchEffortSpeed * Time.deltaTime);
        currentCameraPitchEffort = Mathf.Lerp(currentCameraPitchEffort, targetPitchEffort, cameraPitchEffortSpeed * Time.deltaTime);

        // Set the animator parameters using animator pitch effort
        animator.SetFloat("FlyUp", currentAnimatorPitchEffort);
        animator.SetFloat("FlyRight", currentFlyRight);

        // Handle pitching (up/down rotation) with eased angle limits
        float basePitchAmount = -currentFlyUp * pitchSpeed * Time.deltaTime; // Negative for intuitive controls
        
        // Calculate target pitch angle
        float targetPitchAngle = currentPitchAngle + basePitchAmount;
        
        // Calculate how close we are to the pitch limits (0 = at center, 1 = at limit)
        float pitchRatio = Mathf.Abs(targetPitchAngle) / maxPitchAngle;
        
        // Apply easing factor when approaching limits
        float easingFactor = 1f;
        if (pitchRatio > 0.7f) // Start easing when 70% of the way to the limit
        {
            // Use a smooth curve that goes from 1.0 to 0.0 as we approach the limit
            float easingProgress = (pitchRatio - 0.7f) / 0.3f; // Normalize to 0-1 range
            easingFactor = 1f - (easingProgress * easingProgress); // Quadratic easing
            
            // Only apply easing when moving toward the limit
            if (Mathf.Sign(basePitchAmount) == Mathf.Sign(targetPitchAngle))
            {
                basePitchAmount *= easingFactor;
                targetPitchAngle = currentPitchAngle + basePitchAmount;
            }
        }
        
        // Clamp the target pitch angle
        targetPitchAngle = Mathf.Clamp(targetPitchAngle, -maxPitchAngle, maxPitchAngle);
        
        // Update current pitch angle
        currentPitchAngle = targetPitchAngle;

        // Handle turning
        float turnAmount = currentFlyRight * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, turnAmount, 0f, Space.World);

        // Handle banking
        float targetBankAngle = -currentFlyRight * bankAngle;
        currentBankAngle = Mathf.LerpAngle(currentBankAngle, targetBankAngle, bankSmoothSpeed * Time.deltaTime);
        
        // Apply pitch and bank rotations using absolute Euler angles
        // Keep the current Y rotation (yaw) and apply the new pitch and bank
        Vector3 currentEulers = transform.eulerAngles;
        // Normalize the Y angle to prevent accumulation issues
        float currentYaw = currentEulers.y;
        transform.eulerAngles = new Vector3(currentPitchAngle, currentYaw, currentBankAngle);

        // Rotate the camera based on banking and camera pitch effort (not actual pitch angle)
        float cameraRotationZ = currentBankAngle * -cameraBankCounterRotation;
        float cameraRotationX = -currentCameraPitchEffort * maxPitchAngle * cameraPitchMultiplier; // Negative to fix inversion
        cameraTransform.localEulerAngles = new Vector3(cameraRotationX, cameraTransform.localEulerAngles.y, cameraRotationZ);
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
