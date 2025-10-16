using UnityEngine;

public enum eBossMovePhase
{
    Entrance,    // Dramatic entrance from above
    Combat       // Random movement around screen
}

public class BossMover : MonoBehaviour
{
    [Header("Entrance Phase")]
    [SerializeField] private float m_EntranceSpeed = 2f; // Speed during entrance
    
    [Header("Combat Phase")]
    [SerializeField] private float m_CombatSpeed = 1.5f; // Speed during combat movement
    [SerializeField] private float m_DirectionChangeInterval = 2f; // How often to change direction
    [SerializeField] private float m_ScreenPadding = 1f; // Padding from screen edges
    
    private Camera m_Camera;
    private eBossMovePhase m_CurrentPhase = eBossMovePhase.Entrance;
    private Vector3 m_CurrentDirection = Vector3.down;
    private float m_NextDirectionChangeTime;
    private Vector3 m_ScreenBounds;
    private Vector3 m_EntranceTarget;
    private bool m_IsActive = true;
    
    private void Awake()
    {
        m_Camera = Camera.main;
        calculateScreenBounds();
    }
    
    private void Start()
    {
        initializeEntrance();
    }
    
    private void Update()
    {
        if (!m_IsActive || GameManager.Instance?.GameState != eGameState.Playing)
            return;
            
        switch (m_CurrentPhase)
        {
            case eBossMovePhase.Entrance:
                handleEntranceMovement();
                break;
            case eBossMovePhase.Combat:
                handleCombatMovement();
                break;
        }
    }
    
    private void initializeEntrance()
    {
        // Start boss above screen
        Vector3 startPosition = m_Camera.ViewportToWorldPoint(new Vector3(0.5f, 1.2f, 0));
        startPosition.z = 0f;
        transform.position = startPosition;
        
        // Calculate entrance target position
        m_EntranceTarget = m_Camera.ViewportToWorldPoint(new Vector3(0.5f, 0.7f, 0));
        m_EntranceTarget.z = 0f;
        
        m_CurrentPhase = eBossMovePhase.Entrance;
        m_CurrentDirection = Vector3.down;
    }
    
    private void handleEntranceMovement()
    {
        // Move downward until reaching target position
        transform.Translate(m_EntranceSpeed * Time.deltaTime * m_CurrentDirection, Space.World);
        
        // Check if reached entrance target
        if (transform.position.y <= m_EntranceTarget.y)
        {
            // Clamp position to target
            Vector3 pos = transform.position;
            pos.y = m_EntranceTarget.y;
            transform.position = pos;
            
            // Switch to combat phase
            switchToCombatPhase();
        }
    }
    
    private void switchToCombatPhase()
    {
        m_CurrentPhase = eBossMovePhase.Combat;
        chooseRandomDirection();
        scheduleNextDirectionChange();
    }
    
    private void handleCombatMovement()
    {
        // Move in current direction
        Vector3 newPosition = transform.position + m_CombatSpeed * Time.deltaTime * m_CurrentDirection;
        
        // Check screen boundaries and bounce if necessary
        bool hitBoundary = false;
        
        // Check horizontal boundaries
        if (newPosition.x <= -m_ScreenBounds.x + m_ScreenPadding)
        {
            newPosition.x = -m_ScreenBounds.x + m_ScreenPadding;
            m_CurrentDirection.x = Mathf.Abs(m_CurrentDirection.x); // Force rightward
            hitBoundary = true;
        }
        else if (newPosition.x >= m_ScreenBounds.x - m_ScreenPadding)
        {
            newPosition.x = m_ScreenBounds.x - m_ScreenPadding;
            m_CurrentDirection.x = -Mathf.Abs(m_CurrentDirection.x); // Force leftward
            hitBoundary = true;
        }
        
        // Check vertical boundaries (keep boss in upper portion of screen)
        float minY = m_Camera.ViewportToWorldPoint(new Vector3(0, 0.3f, 0)).y; // Don't go below 30% of screen
        float maxY = m_Camera.ViewportToWorldPoint(new Vector3(0, 0.9f, 0)).y; // Don't go above 90% of screen
        
        if (newPosition.y <= minY)
        {
            newPosition.y = minY;
            m_CurrentDirection.y = Mathf.Abs(m_CurrentDirection.y); // Force upward
            hitBoundary = true;
        }
        else if (newPosition.y >= maxY)
        {
            newPosition.y = maxY;
            m_CurrentDirection.y = -Mathf.Abs(m_CurrentDirection.y); // Force downward
            hitBoundary = true;
        }
        
        // Apply new position
        transform.position = newPosition;
        
        // Change direction if hit boundary or time elapsed
        if (hitBoundary || Time.time >= m_NextDirectionChangeTime)
        {
            chooseRandomDirection();
            scheduleNextDirectionChange();
        }
    }
    
    private void chooseRandomDirection()
    {
        // Choose random direction with preference for interesting movement
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        m_CurrentDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f).normalized;
        
        // Ensure direction is not too vertical (more interesting horizontal movement)
        if (Mathf.Abs(m_CurrentDirection.y) > 0.7f)
        {
            m_CurrentDirection.y *= 0.5f;
            m_CurrentDirection = m_CurrentDirection.normalized;
        }
    }
    
    private void scheduleNextDirectionChange()
    {
        // Add some randomness to direction change timing
        float randomVariation = Random.Range(-0.5f, 0.5f);
        m_NextDirectionChangeTime = Time.time + m_DirectionChangeInterval + randomVariation;
    }
    
    private void calculateScreenBounds()
    {
        // Calculate screen bounds in world coordinates
        Vector3 screenBounds = m_Camera.ViewportToWorldPoint(new Vector3(1f, 1f, 0));
        m_ScreenBounds = new Vector3(screenBounds.x, screenBounds.y, 0f);
    }
}
