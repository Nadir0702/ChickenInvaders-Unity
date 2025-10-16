using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    // Animation parameter hashes for performance
    private static readonly int sr_MovingLeft = Animator.StringToHash("MovingLeft");
    private static readonly int sr_MovingRight = Animator.StringToHash("MovingRight");
    private static readonly int sr_IsLightSpeed = Animator.StringToHash("IsLightSpeed");
    
    [Header("General")]
    [SerializeField] private Rigidbody2D m_Rigidbody2D;
    [SerializeField] private float m_MoveSpeed = 8f;
    [SerializeField] private Vector2 m_Padding = new Vector2(0.5f, 0.5f);
    [SerializeField] private Animator m_MovementAnimator;
    [SerializeField] private Animator m_JetAnimator;
    [SerializeField] private bool m_UseMouseControls = true;
    
    [Header("Mouse Controls")]
    [SerializeField] private float m_MouseSmoothTime = 0.1f;
    [SerializeField] private bool m_UseMouseSmoothing = true;
    
    [Header("Keyboard Controls")]
    [SerializeField] private float m_KeyboardAcceleration = 25f;
    [SerializeField] private float m_KeyboardDrag = 15f;
    [SerializeField] private bool m_UseKeyboardSmoothing = true;

    private Camera m_Camera;
    private Vector2 m_Input;
    private Vector3 m_TargetPosition;
    private Vector3 m_CurrentVelocity; // For SmoothDamp
    private Vector2 m_KeyboardVelocity; // Current keyboard velocity
    
    // Track current movement state to avoid unnecessary animator calls
    private bool m_IsMovingLeft = false;
    private bool m_IsMovingRight = false;

    private void Awake()
    {
        m_Camera = Camera.main;
    }
    
    private void Start()
    {
        // Initialize animator to Straight state
        if (m_MovementAnimator != null)
        {
            m_MovementAnimator.SetBool(sr_MovingLeft, false);
            m_MovementAnimator.SetBool(sr_MovingRight, false);
        }
        
        if (m_JetAnimator != null)
        {
            m_JetAnimator.SetBool(sr_IsLightSpeed, false);
        }
        
        // Initialize target position to current position
        m_TargetPosition = transform.position;
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != eGameState.Playing) return;
        
        if (m_UseMouseControls && m_Camera != null)
        {
            handleMouseInput();
        }
        else
        {
            handleKeyboardInput();
        }
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.GameState != eGameState.Playing) return;
        
        if (m_UseMouseControls && m_Camera != null)
        {
            handleMouseMovement();
        }
        else
        {
            handleKeyboardMovement();
        }

        // Update sprite based on movement direction
        updateSpriteDirection();
    }

    private void handleMouseInput()
    {
        // Mouse controls - ship follows mouse position directly
        Vector3 mouseWorldPos = m_Camera.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = transform.position.z; // Keep same Z position
        
        // Clamp mouse target position to screen bounds (same as keyboard)
        Vector3 min = m_Camera.ViewportToWorldPoint(new Vector3(0, 0.15f, 0));
        Vector3 max = m_Camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        mouseWorldPos.x = Mathf.Clamp(mouseWorldPos.x, min.x + m_Padding.x, max.x - m_Padding.x);
        mouseWorldPos.y = Mathf.Clamp(mouseWorldPos.y, min.y + m_Padding.y, max.y - m_Padding.y);
        
        m_TargetPosition = mouseWorldPos;
    }
    
    private void handleKeyboardInput()
    {
        // Keyboard controls (fallback) - use traditional input-based movement
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        m_Input = new Vector2(x, y).normalized;
    }
    
    private void handleMouseMovement()
    {
        // Mouse controls - move towards target position
        Vector3 currentPos = transform.position;
        Vector3 newPos;
        
        if (m_UseMouseSmoothing)
        {
            // Smooth movement towards mouse position
            newPos = Vector3.SmoothDamp(currentPos, m_TargetPosition, ref m_CurrentVelocity, m_MouseSmoothTime);
        }
        else
        {
            // Direct movement towards mouse position
            newPos = Vector3.MoveTowards(currentPos, m_TargetPosition, m_MoveSpeed * Time.fixedDeltaTime);
        }
        
        // Calculate movement direction for animation
        Vector3 movementDirection = (newPos - currentPos).normalized;
        m_Input = new Vector2(movementDirection.x, movementDirection.y);
        
        // Apply position directly (no rigidbody velocity for mouse control)
        transform.position = newPos;
    }
    
    private void handleKeyboardMovement()
    {
        // Keyboard controls - smooth acceleration and deceleration
        if (m_UseKeyboardSmoothing)
        {
            // Calculate target velocity
            Vector2 targetVelocity = m_Input * m_MoveSpeed;
            
            // Smooth acceleration/deceleration
            if (m_Input.magnitude > 0.1f)
            {
                // Accelerating towards target
                m_KeyboardVelocity = Vector2.MoveTowards(m_KeyboardVelocity, targetVelocity, 
                    m_KeyboardAcceleration * Time.fixedDeltaTime);
            }
            else
            {
                // Decelerating (drag)
                m_KeyboardVelocity = Vector2.MoveTowards(m_KeyboardVelocity, Vector2.zero, 
                    m_KeyboardDrag * Time.fixedDeltaTime);
            }
            
            // Apply smooth velocity
            m_Rigidbody2D.linearVelocity = m_KeyboardVelocity;
        }
        else
        {
            // Original instant keyboard movement
            Vector2 targetVel = m_Input * m_MoveSpeed;
            m_Rigidbody2D.linearVelocity = targetVel;
        }

        // Soft-clamp using camera bounds for keyboard movement
        clampToScreenBounds();
        
        // Update input for animation based on current velocity
        if (m_UseKeyboardSmoothing)
        {
            m_Input = m_KeyboardVelocity.normalized * Mathf.Min(m_KeyboardVelocity.magnitude / m_MoveSpeed, 1f);
        }
    }
    
    private void clampToScreenBounds()
    {
        Vector3 pos = transform.position;
        Vector3 min = m_Camera.ViewportToWorldPoint(new Vector3(0, 0.15f, 0));
        Vector3 max = m_Camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        pos.x = Mathf.Clamp(pos.x, min.x + m_Padding.x, max.x - m_Padding.x);
        pos.y = Mathf.Clamp(pos.y, min.y + m_Padding.y, max.y - m_Padding.y);
        transform.position = pos;
    }

    private void updateSpriteDirection()
    {
        if (m_MovementAnimator == null) return;
        
        // Determine current input state
        bool shouldMoveLeft = m_Input.x < -0.1f;
        bool shouldMoveRight = m_Input.x > 0.1f;
        
        // Only update animator if state actually changed to avoid interrupting transitions
        if (shouldMoveLeft && !m_IsMovingLeft)
        {
            // Start moving left
            m_MovementAnimator.SetBool(sr_MovingLeft, true);
            m_MovementAnimator.SetBool(sr_MovingRight, false);
            m_IsMovingLeft = true;
            m_IsMovingRight = false;
        }
        else if (shouldMoveRight && !m_IsMovingRight)
        {
            // Start moving right
            m_MovementAnimator.SetBool(sr_MovingLeft, false);
            m_MovementAnimator.SetBool(sr_MovingRight, true);
            m_IsMovingLeft = false;
            m_IsMovingRight = true;
        }
        else if (!shouldMoveLeft && !shouldMoveRight && (m_IsMovingLeft || m_IsMovingRight))
        {
            // Stop moving (return to straight)
            m_MovementAnimator.SetBool(sr_MovingLeft, false);
            m_MovementAnimator.SetBool(sr_MovingRight, false);
            m_IsMovingLeft = false;
            m_IsMovingRight = false;
        }
    }

    /// <summary>
    /// Reset input and stop movement (called during respawn)
    /// </summary>
    public void ResetMovement()
    {
        m_Input = Vector2.zero;
        if (m_Rigidbody2D) m_Rigidbody2D.linearVelocity = Vector2.zero;
        
        // Reset animator to straight position
        if (m_MovementAnimator != null)
        {
            m_MovementAnimator.SetBool(sr_MovingLeft, false);
            m_MovementAnimator.SetBool(sr_MovingRight, false);
        }
        
        // Reset movement state tracking
        m_IsMovingLeft = false;
        m_IsMovingRight = false;
        
        // Reset target position and smoothing velocity
        m_TargetPosition = transform.position;
        m_CurrentVelocity = Vector3.zero;
        m_KeyboardVelocity = Vector2.zero;
    }

    public void GoToLightSpeed()
    {
        Debug.Log("PlayerController: GoToLightSpeed() called");
        
        if (m_JetAnimator == null) 
        {
            Debug.LogWarning("PlayerController: JetAnimator is null!");
            return;
        }
        
        m_JetAnimator.SetBool(sr_IsLightSpeed, true);
        Debug.Log("PlayerController: Calling PlayLightSpeedSFX()");
        AudioManager.Instance?.PlayLightSpeedSFX(); // Play pausable SFX
        Debug.Log("PlayerController: Calling OnLightSpeedStart()");
        AudioManager.Instance?.OnLightSpeedStart(); // Start LightSpeed music
        ParallaxManager.Instance?.AccelerateBackground(); // Accelerate background
    }
    
    public void ExitLightSpeed()
    {
        if (m_JetAnimator == null) return;
        
        m_JetAnimator.SetBool(sr_IsLightSpeed, false);
        AudioManager.Instance?.StopSFX(); // Stop light speed SFX
        ParallaxManager.Instance?.DecelerateBackground(); // Decelerate background
    }

    public void ToggleControls()
    {
        m_UseMouseControls = !m_UseMouseControls;
    }
    
    /// <summary>
    /// Set control type (called by SettingsManager)
    /// </summary>
    public void SetUseMouseControls(bool i_UseMouseControls)
    {
        m_UseMouseControls = i_UseMouseControls;
    }
    
    /// <summary>
    /// Set mouse smoothing (called by SettingsManager)
    /// </summary>
    public void SetMouseSmoothing(bool i_UseMouseSmoothing)
    {
        m_UseMouseSmoothing = i_UseMouseSmoothing;
    }
    
    /// <summary>
    /// Set keyboard smoothing (called by SettingsManager)
    /// </summary>
    public void SetKeyboardSmoothing(bool i_UseKeyboardSmoothing)
    {
        m_UseKeyboardSmoothing = i_UseKeyboardSmoothing;
    }
}