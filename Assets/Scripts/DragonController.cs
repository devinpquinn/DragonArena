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
        // Calculate target position based on mouse input
        Vector3 targetOffset = new Vector3(
            smoothedInput.x * horizontalRange,
            smoothedInput.y * verticalRange,
            0
        );
        
        Vector3 targetPosition = startPosition + targetOffset;
        
        // Move forward continuously
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.Self);
        
        // Smoothly move towards the target horizontal and vertical position
        Vector3 currentPos = transform.position;
        Vector3 desiredPos = new Vector3(targetPosition.x, targetPosition.y, currentPos.z);
        transform.position = Vector3.Lerp(currentPos, desiredPos, smoothSpeed * Time.deltaTime);
    }
    
    private void UpdateRotation()
    {
        // Calculate banking (roll) based on horizontal movement
        float bankRotation = -smoothedInput.x * bankAngle;
        
        // Calculate pitching based on vertical movement
        float pitchRotation = -smoothedInput.y * pitchAngle;
        
        // Calculate yaw (turning) based on horizontal input
        float yawRotation = smoothedInput.x * rotationSpeed * Time.deltaTime;
        
        // Apply banking and pitching relative to base rotation
        Quaternion targetRotation = baseRotation * Quaternion.Euler(pitchRotation, 0, bankRotation);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        
        // Apply continuous yaw rotation
        transform.Rotate(0, yawRotation, 0, Space.World);
    }
}
