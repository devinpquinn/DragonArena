using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    private Animator animator;
    private Rigidbody rb;

    [Header("Flight Controls")]
    public float forwardSpeed = 10f;
    public float inputSmoothSpeed = 5f;
    public float rotationSpeed = 90f; // degrees per second

    private float currentFlyUp = 0f;
    private float currentFlyRight = 0f;

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
        currentFlyUp = Mathf.Lerp(currentFlyUp, verticalInput, inputSmoothSpeed * Time.deltaTime);
        currentFlyRight = Mathf.Lerp(currentFlyRight, horizontalInput, inputSmoothSpeed * Time.deltaTime);

        // Set the animator parameters
        animator.SetFloat("FlyUp", currentFlyUp);
        animator.SetFloat("FlyRight", currentFlyRight);

        // Handle turning
        float turnAmount = currentFlyRight * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, turnAmount, 0f, Space.World);
    }

    void HandleForwardMovement()
    {
        if (rb != null)
        {
            // Move forward in the direction the dragon is facing
            Vector3 forwardMovement = transform.forward * forwardSpeed;

            // Apply the movement
            rb.velocity = forwardMovement;
        }
    }
}
