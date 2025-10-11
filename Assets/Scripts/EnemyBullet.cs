using UnityEngine;

public class EnemyBullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float m_Speed = 2f;
    [SerializeField] private float m_LifeTime = 5f; // seconds - longer than player bullets since they travel further
    
    private float m_DeathTime;
    private Vector2 m_Direction;
    private bool m_Active;
    private Camera m_Camera;

    private void Awake()
    {
        m_Camera = Camera.main;
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
    }

    void Update()
    {
        if (!m_Active) return;
        
        transform.Translate(Time.deltaTime * m_Speed * m_Direction, Space.World);
        
        // Check if lifetime expired
        if (Time.time >= m_DeathTime)
        {
            deactivate();
            return;
        }
        
        // Check if off screen (below camera view)
        Vector3 viewPortPosition = m_Camera.WorldToViewportPoint(transform.position);
        if (viewPortPosition.y < -0.15f)
        {
            deactivate();
        }
    }

    void OnTriggerEnter2D(Collider2D i_Other)
    {
        // Only hit player
        if (i_Other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            
            deactivate();
        }
    }

    private void deactivate()
    {
        m_Active = false;
        PoolManager.Instance?.ReturnEnemyBullet(this);
    }
}
