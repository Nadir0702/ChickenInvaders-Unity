using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    // Animation parameter hashes for performance
    private static readonly int sr_MovingLeft = Animator.StringToHash("MovingLeft");
    private static readonly int sr_MovingRight = Animator.StringToHash("MovingRight");
    
    [SerializeField] private Rigidbody2D m_Rigidbody2D;
    [SerializeField] private float m_MoveSpeed = 8f;
    [SerializeField] private Vector2 m_Padding = new Vector2(0.5f, 0.5f);
    [SerializeField] private Animator m_Animator;

    private Camera m_Camera;
    private Vector2 m_Input;
    
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
        if (m_Animator != null)
        {
            m_Animator.SetBool(sr_MovingLeft, false);
            m_Animator.SetBool(sr_MovingRight, false);
        }
    }

    private void Update()
    {
        if (GameManager.Instance.GameState != eGameState.Playing) return;
        // Old Input Manager
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        m_Input = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        if (GameManager.Instance.GameState != eGameState.Playing) return;
        
        Vector2 targetVel = m_Input * m_MoveSpeed;
        m_Rigidbody2D.linearVelocity = targetVel;

        // Update sprite based on input direction
        updateSpriteDirection();

        // Soft-clamp using camera bounds
        Vector3 pos = transform.position;
        Vector3 min = m_Camera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 max = m_Camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        pos.x = Mathf.Clamp(pos.x, min.x + m_Padding.x, max.x - m_Padding.x);
        pos.y = Mathf.Clamp(pos.y, min.y + m_Padding.y, max.y - m_Padding.y);
        transform.position = pos;
    }

    private void updateSpriteDirection()
    {
        if (m_Animator == null) return;
        
        // Determine current input state
        bool shouldMoveLeft = m_Input.x < -0.1f;
        bool shouldMoveRight = m_Input.x > 0.1f;
        
        // Only update animator if state actually changed to avoid interrupting transitions
        if (shouldMoveLeft && !m_IsMovingLeft)
        {
            // Start moving left
            m_Animator.SetBool(sr_MovingLeft, true);
            m_Animator.SetBool(sr_MovingRight, false);
            m_IsMovingLeft = true;
            m_IsMovingRight = false;
        }
        else if (shouldMoveRight && !m_IsMovingRight)
        {
            // Start moving right
            m_Animator.SetBool(sr_MovingLeft, false);
            m_Animator.SetBool(sr_MovingRight, true);
            m_IsMovingLeft = false;
            m_IsMovingRight = true;
        }
        else if (!shouldMoveLeft && !shouldMoveRight && (m_IsMovingLeft || m_IsMovingRight))
        {
            // Stop moving (return to straight)
            m_Animator.SetBool(sr_MovingLeft, false);
            m_Animator.SetBool(sr_MovingRight, false);
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
        if (m_Animator != null)
        {
            m_Animator.SetBool(sr_MovingLeft, false);
            m_Animator.SetBool(sr_MovingRight, false);
        }
        
        // Reset movement state tracking
        m_IsMovingLeft = false;
        m_IsMovingRight = false;
    }
}