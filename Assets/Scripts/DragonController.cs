using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private float sensitivity = 1.0f;
    [SerializeField] private float smoothSpeed = 5.0f;
    
    [Header("Movement Settings")]
    [SerializeField] private float forwardSpeed = 10.0f;
    [SerializeField] private float horizontalRange = 5.0f;
    [SerializeField] private float verticalRange = 3.0f;
    [SerializeField] private float rotationSpeed = 80.0f; // Increased for more responsive turning
    [SerializeField] private float bankAngle = 30.0f; // How much the dragon banks when turning
    [SerializeField] private float pitchAngle = 20.0f; // How much the dragon pitches when climbing/diving
    
    private Vector2 currentInput;
    private Vector2 smoothedInput;
    private Vector3 startPosition;
    private Quaternion baseRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        // If no animator is assigned, try to get one from this GameObject
        if (targetAnimator == null)
            targetAnimator = GetComponent<Animator>();
            
        // Store starting position and rotation as reference points
        startPosition = transform.position;
        baseRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        HandleKeyboardInput();
        UpdateAnimationParameters();
        UpdateRotation();
        UpdateMovement();
    }
    
    private void HandleKeyboardInput()
    {
        // Get WASD input
        float horizontal = Input.GetAxis("Horizontal"); // A/D keys
        float vertical = Input.GetAxis("Vertical");     // W/S keys
        
        // Create input vector and normalize it to prevent faster diagonal movement
        Vector2 rawInput = new Vector2(horizontal, vertical);
        if (rawInput.magnitude > 1f)
            rawInput = rawInput.normalized;
        
        // Apply sensitivity
        currentInput.x = rawInput.x * sensitivity; // FlyRight
        currentInput.y = rawInput.y * sensitivity; // FlyUp
        
        // Clamp values to -1 to 1 range (should already be normalized, but just in case)
        currentInput.x = Mathf.Clamp(currentInput.x, -1f, 1f);
        currentInput.y = Mathf.Clamp(currentInput.y, -1f, 1f);
        
        // Smooth the input for more natural movement
        smoothedInput = Vector2.Lerp(smoothedInput, currentInput, smoothSpeed * Time.deltaTime);
    }
    
    private void UpdateAnimationParameters()
    {
        if (targetAnimator != null)
        {
            targetAnimator.SetFloat("FlyRight", smoothedInput.x);
            targetAnimator.SetFloat("FlyUp", smoothedInput.y);
        }
    }
    
    private void UpdateMovement()
    {
        // Simply move forward in whatever direction the dragon is currently facing
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);
    }
    
    private void UpdateRotation()
    {
        // Calculate banking (roll) based on horizontal movement
        float bankRotation = -smoothedInput.x * bankAngle;
        
        // Calculate pitching based on vertical movement
        float pitchRotation = -smoothedInput.y * pitchAngle;
        
        // Calculate yaw (turning) based on horizontal input for actual direction change
        float yawRotation = smoothedInput.x * rotationSpeed * Time.deltaTime;
        
        // Apply yaw rotation first to change flight direction
        transform.Rotate(0, yawRotation, 0, Space.World);
        
        // Get current rotation and clamp the roll to prevent going upside down
        Vector3 currentEuler = transform.eulerAngles;
        
        // Convert angles to -180 to 180 range for easier clamping
        float currentRoll = currentEuler.z;
        if (currentRoll > 180) currentRoll -= 360;
        
        float currentPitch = currentEuler.x;
        if (currentPitch > 180) currentPitch -= 360;
        
        // Calculate target roll with banking, but clamp it
        float targetRoll = bankRotation;
        targetRoll = Mathf.Clamp(targetRoll, -bankAngle, bankAngle);
        
        // Calculate target pitch, but clamp it too
        float targetPitch = currentPitch + pitchRotation;
        targetPitch = Mathf.Clamp(targetPitch, -pitchAngle, pitchAngle);
        
        // Apply the clamped rotations
        Vector3 targetEuler = new Vector3(
            targetPitch,
            currentEuler.y,
            targetRoll
        );
        
        // Smooth the banking and pitching rotations
        transform.rotation = Quaternion.Lerp(
            transform.rotation, 
            Quaternion.Euler(targetEuler), 
            smoothSpeed * Time.deltaTime
        );
    }
}
