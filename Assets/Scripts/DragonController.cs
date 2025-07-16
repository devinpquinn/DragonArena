using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private Animator targetAnimator;
    [SerializeField] private float sensitivity = 1.0f;
    [SerializeField] private float smoothSpeed = 5.0f;
    
    private Vector2 screenCenter;
    private Vector2 currentInput;
    private Vector2 smoothedInput;
    
    // Start is called before the first frame update
    void Start()
    {
        // If no animator is assigned, try to get one from this GameObject
        if (targetAnimator == null)
            targetAnimator = GetComponent<Animator>();
            
        // Calculate screen center
        screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
        HandleMouseInput();
        UpdateAnimationParameters();
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
}
