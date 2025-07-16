using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;
    
    [Header("Movement")]
    [Tooltip("Constant forward movement speed")]
    public float forwardSpeed = 10f;
    
    [Header("Flight Controls")]
    [Tooltip("Speed at which the flight parameters change")]
    public float inputSmoothSpeed = 5f;
    
    [Header("Rotation Controls")]
    [Tooltip("Speed at which the dragon rotates left and right")]
    public float rotationSpeed = 90f; // degrees per second
    
    private float currentFlyUp = 0f;
    private float currentFlyRight = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        // Get the Animator component
        animator = GetComponent<Animator>();
        
        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        
        if (animator == null)
        {
            Debug.LogError("DragonController: No Animator component found on " + gameObject.name);
        }
        
        if (rb == null)
        {
            Debug.LogError("DragonController: No Rigidbody component found on " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleFlightInput();
        HandleRotation();
        UpdateAnimatorParameters();
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
        
        // Smoothly interpolate to target values
        currentFlyUp = Mathf.Lerp(currentFlyUp, verticalInput, inputSmoothSpeed * Time.deltaTime);
        currentFlyRight = Mathf.Lerp(currentFlyRight, horizontalInput, inputSmoothSpeed * Time.deltaTime);
    }
    
    void HandleForwardMovement()
    {
        if (rb != null)
        {
            // Move forward in the direction the dragon is facing
            Vector3 forwardMovement = transform.forward * forwardSpeed;
            
            // Apply the movement while preserving current Y velocity (for gravity/vertical movement)
            rb.velocity = new Vector3(forwardMovement.x, rb.velocity.y, forwardMovement.z);
        }
    }
    
    void HandleRotation()
    {
        float rotationInput = 0f;
        
        // A/D for rotation (A = left, D = right)
        if (Input.GetKey(KeyCode.A))
            rotationInput = -1f; // Rotate left
        else if (Input.GetKey(KeyCode.D))
            rotationInput = 1f;  // Rotate right
        
        // Apply rotation around Y-axis
        if (rotationInput != 0f)
        {
            float rotationAmount = rotationInput * rotationSpeed * Time.deltaTime;
            transform.Rotate(0f, rotationAmount, 0f, Space.Self);
        }
    }
    
    void UpdateAnimatorParameters()
    {
        if (animator != null)
        {
            // Set the animator parameters
            animator.SetFloat("FlyUp", currentFlyUp);
            animator.SetFloat("FlyRight", currentFlyRight);
        }
    }
}
