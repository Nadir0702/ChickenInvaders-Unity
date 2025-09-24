using UnityEngine;

public class Food : MonoBehaviour, IPoolable
{
    [SerializeField ]private Rigidbody2D m_Rigidbody;
    
    [Header("Physics")]
    [SerializeField] private float m_EjectionForceMin = 150f;
    [SerializeField] private float m_EjectionForceMax = 300f;
    [SerializeField] private float m_BounceDamping = 0.6f;
    [SerializeField] private float m_MinVelocityThreshold = 0.5f;
    [SerializeField] private float m_FrictionForce = 5f;
    
    [Header("Collection")]
    [SerializeField] private float m_FallSpeed = 2f;
    [SerializeField] private float m_SettleTime = 1f;
    
    private bool m_IsPhysicsActive = true;
    private bool m_IsGrounded;
    private float m_SettleTimer;
    private Camera m_Camera;
    private float m_GroundY;
    
    private void Awake()
    {
        m_Camera = Camera.main;
        
        // Configure rigidbody for realistic physics
        m_Rigidbody.gravityScale = 1f; // Use Unity's gravity
        m_Rigidbody.linearDamping = 0.5f; // Air resistance
        m_Rigidbody.angularDamping = 0.8f; // Rotation resistance
        m_Rigidbody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }
    
    private void Start()
    {
        // Calculate ground level (bottom of screen)
        m_GroundY = m_Camera.ViewportToWorldPoint(new Vector3(0, 0.1f, 0)).y;
        
        // Initialize with random ejection force
        initializePhysics();
    }
    
    public void OnPoolGet()
    {
        // Reset physics state when retrieved from pool
        m_IsPhysicsActive = true;
        m_IsGrounded = false;
        m_SettleTimer = 0f;
        
        // Reset rigidbody
        if (m_Rigidbody)
        {
            m_Rigidbody.linearVelocity = Vector2.zero;
            m_Rigidbody.angularVelocity = 0f;
            m_Rigidbody.gravityScale = 1f;
        }
        
        // Calculate ground level (in case screen size changed)
        if (m_Camera) m_GroundY = m_Camera.ViewportToWorldPoint(new Vector3(0, 0.1f, 0)).y;
        
        // Initialize physics for new spawn
        initializePhysics();
    }
    
    public void OnPoolReturn()
    {
        // Clean up state before returning to pool
        m_IsPhysicsActive = false;
        if (m_Rigidbody)
        {
            m_Rigidbody.linearVelocity = Vector2.zero;
            m_Rigidbody.angularVelocity = 0f;
        }
    }
    
    private void initializePhysics()
    {
        if (!m_Rigidbody) return;
        
        // Random ejection direction (slightly upward and outward)
        float angle = Random.Range(-60f, 60f); // Angle in degrees from straight up
        float force = Random.Range(m_EjectionForceMin, m_EjectionForceMax);
        
        Vector2 direction = new Vector2(
            Mathf.Sin(angle * Mathf.Deg2Rad),
            Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad)) // Always upward component
        );
        
        // Apply initial ejection force
        m_Rigidbody.AddForce(direction * force, ForceMode2D.Impulse);
        
        // Add some random spin for visual appeal
        float randomTorque = Random.Range(-800f, 800f);
        m_Rigidbody.AddTorque(randomTorque);
    }
    
    private void FixedUpdate()
    {
        if (m_IsPhysicsActive)
        {
            updatePhysics();
        }
        else
        {
            // Simple downward movement like other pickups when physics are done
            // Use MovePosition for physics consistency
            m_Rigidbody.MovePosition(m_Rigidbody.position +  m_FallSpeed * Time.fixedDeltaTime * Vector2.down);
        }
    }
    
    private void Update()
    {
        // Handle off-screen check in Update (doesn't need to be physics-based)
        handleOffScreen();
    }
    
    private void updatePhysics()
    {
        if (!m_Rigidbody) return;
        
        // Apply horizontal friction if grounded and moving slowly horizontally
        if (m_IsGrounded && Mathf.Abs(m_Rigidbody.linearVelocity.x) > 0.1f)
        {
            Vector2 frictionForce =  m_FrictionForce * Time.fixedDeltaTime * -m_Rigidbody.linearVelocity.normalized;
            frictionForce.y = 0; // Only apply friction horizontally
            m_Rigidbody.AddForce(frictionForce, ForceMode2D.Force);
        }
        
        // Check ground collision
        if (transform.position.y <= m_GroundY && m_Rigidbody.linearVelocity.y <= 0)
        {
            if (!m_IsGrounded)
            {
                // First time hitting ground - apply bounce
                Vector2 currentVelocity = m_Rigidbody.linearVelocity;
                currentVelocity.y = -currentVelocity.y * m_BounceDamping;
                m_Rigidbody.linearVelocity = currentVelocity;
                m_IsGrounded = true;
            }
            
            // Clamp position to ground
            Vector3 pos = transform.position;
            pos.y = m_GroundY;
            transform.position = pos;
            
            // If velocity is low enough, start settling
            if (m_Rigidbody.linearVelocity.magnitude < m_MinVelocityThreshold)
            {
                m_SettleTimer += Time.fixedDeltaTime;
                if (m_SettleTimer >= m_SettleTime)
                {
                    m_IsPhysicsActive = false;
                    // Disable gravity for settled food
                    m_Rigidbody.gravityScale = 0f;
                    m_Rigidbody.linearVelocity = Vector2.zero;
                }
            }
        }
        else
        {
            m_IsGrounded = false;
        }
        
        // Handle screen boundaries
        handleScreenBounds();
    }
    
    private void handleScreenBounds()
    {
        if (!m_Rigidbody) return;
        
        Vector3 viewportPos = m_Camera.WorldToViewportPoint(transform.position);
        
        // Bounce off left and right walls
        if (viewportPos.x <= 0.05f && m_Rigidbody.linearVelocity.x < 0)
        {
            Vector2 velocity = m_Rigidbody.linearVelocity;
            velocity.x = -velocity.x * m_BounceDamping;
            m_Rigidbody.linearVelocity = velocity;
            
            // Clamp position to boundary
            Vector3 pos = transform.position;
            pos.x = m_Camera.ViewportToWorldPoint(new Vector3(0.05f, 0, 0)).x;
            transform.position = pos;
        }
        else if (viewportPos.x >= 0.95f && m_Rigidbody.linearVelocity.x > 0)
        {
            Vector2 velocity = m_Rigidbody.linearVelocity;
            velocity.x = -velocity.x * m_BounceDamping;
            m_Rigidbody.linearVelocity = velocity;
            
            // Clamp position to boundary
            Vector3 pos = transform.position;
            pos.x = m_Camera.ViewportToWorldPoint(new Vector3(0.95f, 0, 0)).x;
            transform.position = pos;
        }
    }
    
    private void handleOffScreen()
    {
        Vector3 viewPortPosition = m_Camera.WorldToViewportPoint(transform.position);
        if (viewPortPosition.y < -0.15f || viewPortPosition.x < -0.15f || viewPortPosition.x > 1.15f)
        {
            // Return to pool instead of destroying
            PoolManager.Instance?.ReturnFood(this);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D i_Other)
    {
        // Grant 50 points for food collection
        GameManager.Instance?.AddScore(50);
        
        AudioManager.Instance?.Play(eSFXId.Eat, 0.5f);
        
        // Return to pool instead of destroying
        PoolManager.Instance?.ReturnFood(this);
    }
}
