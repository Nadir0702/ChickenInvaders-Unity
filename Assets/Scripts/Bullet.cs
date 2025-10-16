using UnityEngine;

public class Bullet : MonoBehaviour, IPoolable
{
    [SerializeField] private float m_Speed = 18f;
    [SerializeField] private float m_LifeTime = 2f; // seconds
    [SerializeField] private int m_Damage = 1;

    private float m_DeathTime;
    private Vector2 m_Direction;
    private bool m_Active;

    public void Fire(Vector2 i_Direction)
    {
        m_Direction = i_Direction.normalized;
        m_DeathTime = Time.time + m_LifeTime;
        m_Active = true;
    }
    
    public void OnPoolGet()
    {
        // Reset state when retrieved from pool
        m_Active = false;
        m_Direction = Vector2.zero;
        m_DeathTime = 0f;
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
        
        transform.Translate(Time.deltaTime * m_Speed * m_Direction , Space.World);
        if(Time.time >= m_DeathTime)
        {
            deactivate();
        }
    }

    void OnTriggerEnter2D(Collider2D i_Other)
    {
        // Only hit enemies
        if (i_Other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            if(i_Other.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(m_Damage);
            }
            
            deactivate();
        }
    }

    private void deactivate()
    {
        m_Active = false;
        PoolManager.Instance?.ReturnBullet(this);
    }
}

