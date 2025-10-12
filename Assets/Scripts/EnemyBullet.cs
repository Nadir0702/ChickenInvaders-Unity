using UnityEngine;

public class EnemyBullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float m_Speed = 2f;
    [SerializeField] private float m_LifeTime = 5f; // seconds - longer than player bullets since they travel further
    [SerializeField] private SpriteRenderer m_SpriteRenderer;
    [SerializeField] private Sprite m_NormalSprite;
    [SerializeField] private Sprite m_BrokenSprite;
    
    private float m_DeathTime;
    private Vector2 m_Direction;
    private bool m_Active;
    private bool m_IsBroken;
    private float m_GroundY;
    private Camera m_Camera;

    private void Awake()
    {
        m_Camera = Camera.main;
        
        // Calculate ground level (same as food bouncing Y position)
        if (m_Camera != null)
        {
            m_GroundY = m_Camera.ViewportToWorldPoint(new Vector3(0, 0.1f, 0)).y;
        }
    }

    public void Fire(Vector2 i_Direction)
    {
        m_Direction = i_Direction.normalized;
        m_DeathTime = Time.time + m_LifeTime;
        m_Active = true;
        // Don't set active here - pool will handle it
    }
    
    public void OnPoolGet()
    {
        // Reset state when retrieved from pool
        m_Active = false;
        m_Direction = Vector2.zero;
        m_DeathTime = 0f;
        m_IsBroken = false;
        
        // Reset to normal sprite
        if (m_SpriteRenderer && m_NormalSprite)
        {
            m_SpriteRenderer.sprite = m_NormalSprite;
        }
        
        randomizeSpeed();
    }

    private void randomizeSpeed()
    {
        // Optionally randomize speed slightly for variety
        m_Speed = Random.Range(2f, 4f);
    }

    public void OnPoolReturn()
    {
        // Clean up state before returning to pool
        m_Active = false;
        m_Direction = Vector2.zero;
        m_IsBroken = false;
    }

    void Update()
    {
        if (!m_Active) return;
        
        // Only move if not broken
        if (!m_IsBroken)
        {
            transform.Translate(Time.deltaTime * m_Speed * m_Direction, Space.World);
            
            // Check if egg reached the ground level (same as food bouncing)
            if (transform.position.y <= m_GroundY)
            {
                AudioManager.Instance?.Play(eSFXId.EggCrack, 0.8f);
                breakEgg();
                return;
            }
        }
        
        // Check if lifetime expired
        if (Time.time >= m_DeathTime)
        {
            deactivate();
            return;
        }
        
        // Check if off screen (below camera view) - only for non-broken eggs
        if (!m_IsBroken)
        {
            Vector3 viewPortPosition = m_Camera.WorldToViewportPoint(transform.position);
            if (viewPortPosition.y < -0.15f)
            {
                deactivate();
            }
        }
    }

    void OnTriggerEnter2D(Collider2D i_Other)
    {
        // Only hit player if egg is not broken
        if (!m_IsBroken && i_Other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            deactivate();
        }
    }

    private void breakEgg()
    {
        m_IsBroken = true;
        
        // Clamp position to ground level
        Vector3 pos = transform.position;
        pos.y = m_GroundY;
        transform.position = pos;
        
        // Change to broken sprite
        if (m_SpriteRenderer && m_BrokenSprite)
        {
            m_SpriteRenderer.sprite = m_BrokenSprite;
        }
        
        // Stop movement
        m_Direction = Vector2.zero;
    }

    private void deactivate()
    {
        m_Active = false;
        PoolManager.Instance?.ReturnEnemyBullet(this);
    }
}
