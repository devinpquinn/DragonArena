using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    private Animator animator;
    
    [Header("Flight Controls")]
    [Tooltip("Speed at which the flight parameters change")]
    public float inputSmoothSpeed = 5f;
    
    private float currentFlyUp = 0f;
    private float currentFlyRight = 0f;
    
    // Start is called before the first frame update
    void Start()
    {
        // Get the Animator component
        animator = GetComponent<Animator>();
        
        if (animator == null)
        {
            Debug.LogError("DragonController: No Animator component found on " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleFlightInput();
        UpdateAnimatorParameters();
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
