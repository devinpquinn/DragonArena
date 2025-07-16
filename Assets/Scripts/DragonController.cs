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
    [SerializeField] private float horizontalRange = 10.0f;
    [SerializeField] private float verticalRange = 5.0f;
    [SerializeField] private float rotationSpeed = 50.0f;
    [SerializeField] private float bankAngle = 30.0f; // How much the dragon banks when turning
    [SerializeField] private float pitchAngle = 20.0f; // How much the dragon pitches when climbing/diving
    
    private Vector2 screenCenter;
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
            
        // Calculate screen center
        screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        
        // Store starting position and rotation as reference points
        startPosition = transform.position;
        baseRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();
        UpdateAnimationParameters();
        UpdateMovement();
        UpdateRotation();
    }
    
    private void HandleMouseInput()
    {
        // Get mouse position relative to screen center
        Vector2 mousePos = Input.mousePosition;
        Vector2 mouseOffset = mousePos - screenCenter;
        
        // Normalize to -1 to 1 range based on screen size
        currentInput.x = (mouseOffset.x / screenCenter.x) * sensitivity; // FlyRight
        currentInput.y = (mouseOffset.y / screenCenter.y) * sensitivity; // FlyUp
        
        // Clamp values to -1 to 1 range
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
        // Move forward in the direction the dragon is currently facing
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);
        
        // Optional: Apply additional positional offset based on mouse input for more responsive feel
        Vector3 lateralMovement = new Vector3(
            smoothedInput.x * horizontalRange * Time.deltaTime,
            smoothedInput.y * verticalRange * Time.deltaTime,
            0
        );
        
        // Apply lateral movement in world space for more natural feel
        transform.Translate(lateralMovement, Space.World);
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
