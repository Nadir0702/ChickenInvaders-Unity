using UnityEngine;

public class Food : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float m_EjectionForceMin = 2f;
    [SerializeField] private float m_EjectionForceMax = 4f;
    [SerializeField] private float m_Gravity = -9.8f;
    [SerializeField] private float m_BounceDamping = 0.6f;
    [SerializeField] private float m_FrictionDeceleration = 2f;
    [SerializeField] private float m_MinVelocityThreshold = 0.5f;
    
    [Header("Collection")]
    [SerializeField] private float m_FallSpeed = 2f;
    [SerializeField] private float m_SettleTime = 1f;
    
    private Vector2 m_Velocity;
    private bool m_IsPhysicsActive = true;
    private bool m_IsGrounded;
    private float m_SettleTimer;
    private Camera m_Camera;
    private float m_GroundY;
    
    private void Awake()
    {
        m_Camera = Camera.main;
    }
    
    private void Start()
    {
        // Calculate ground level (bottom of screen)
        m_GroundY = m_Camera.ViewportToWorldPoint(new Vector3(0, 0.1f, 0)).y;
        
        // Initialize with random ejection velocity
        initializePhysics();
    }
    
    private void initializePhysics()
    {
        // Random ejection direction (slightly upward and outward)
        float angle = Random.Range(-60f, 60f); // Angle in degrees from straight up
        float force = Random.Range(m_EjectionForceMin, m_EjectionForceMax);
        
        Vector2 direction = new Vector2(
            Mathf.Sin(angle * Mathf.Deg2Rad),
            Mathf.Abs(Mathf.Cos(angle * Mathf.Deg2Rad)) // Always upward component
        );
        
        m_Velocity = direction * force;
    }
    
    private void Update()
    {
        if (m_IsPhysicsActive)
        {
            updatePhysics();
        }
        else
        {
            // Simple downward movement like other pickups when physics are done
            transform.Translate(m_FallSpeed * Time.deltaTime * Vector2.down, Space.World);
            handleOffScreen();
        }
    }
    
    private void updatePhysics()
    {
        // Apply gravity
        m_Velocity.y += m_Gravity * Time.deltaTime;
        
        // Apply horizontal friction if grounded
        if (m_IsGrounded && Mathf.Abs(m_Velocity.x) > 0.1f)
        {
            float frictionForce = m_FrictionDeceleration * Time.deltaTime;
            if (m_Velocity.x > 0)
                m_Velocity.x = Mathf.Max(0, m_Velocity.x - frictionForce);
            else
                m_Velocity.x = Mathf.Min(0, m_Velocity.x + frictionForce);
        }
        
        // Move based on velocity
        Vector2 movement = m_Velocity * Time.deltaTime;
        transform.Translate(movement, Space.World);
        
        // Check ground collision
        if (transform.position.y <= m_GroundY && m_Velocity.y <= 0)
        {
            // Bounce
            m_Velocity.y = -m_Velocity.y * m_BounceDamping;
            m_IsGrounded = true;
            
            // Clamp position to ground
            Vector3 pos = transform.position;
            pos.y = m_GroundY;
            transform.position = pos;
            
            // If velocity is low enough, start settling
            if (m_Velocity.magnitude < m_MinVelocityThreshold)
            {
                m_SettleTimer += Time.deltaTime;
                if (m_SettleTimer >= m_SettleTime)
                {
                    m_IsPhysicsActive = false;
                    m_Velocity = Vector2.zero;
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
        Vector3 viewportPos = m_Camera.WorldToViewportPoint(transform.position);
        
        // Bounce off left and right walls
        if (viewportPos.x <= 0.05f && m_Velocity.x < 0)
        {
            m_Velocity.x = -m_Velocity.x * m_BounceDamping;
            Vector3 pos = transform.position;
            pos.x = m_Camera.ViewportToWorldPoint(new Vector3(0.05f, 0, 0)).x;
            transform.position = pos;
        }
        else if (viewportPos.x >= 0.95f && m_Velocity.x > 0)
        {
            m_Velocity.x = -m_Velocity.x * m_BounceDamping;
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
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D i_Other)
    {
        // Grant 50 points for food collection
        GameManager.Instance?.AddScore(50);
        
        AudioManager.Instance?.Play(eSFXId.Eat, 0.5f);
        Destroy(gameObject);
    }
}
