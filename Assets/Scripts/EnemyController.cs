using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private GameObject[] m_DropPrefabs;
    [SerializeField] private int m_BaseMaxHp = 1; // Base HP before scaling
    private int m_MaxHp = 1; // Actual max HP after scaling  
    private int m_Hp = 1;
    private bool m_IsDead = false; // Prevent multiple death calls
    
    // Debug property for inspector visibility
    public int CurrentHP => m_Hp;
    public int MaxHP => m_MaxHp;
    
    private void OnEnable() => m_Hp = m_MaxHp;
    
    public void OnPoolGet()
    {
        // Calculate scaled HP based on current difficulty
        m_MaxHp = m_BaseMaxHp + (GameManager.Instance.CurrentDifficultyTier - 1);
        m_Hp = m_MaxHp; // Set current HP to max HP
        m_IsDead = false; // Reset death state
        
        // Debug: Log HP scaling for first few enemies to verify system works
        if (GameManager.Instance.CurrentDifficultyTier > 1 && Random.value < 0.1f) // 10% chance to log
        {
            Debug.Log($"Enemy spawned with {m_Hp} HP (Base: {m_BaseMaxHp}, Tier: {GameManager.Instance.CurrentDifficultyTier})");
        }
        // Reset any other state if needed
    }
    
    public void OnPoolReturn()
    {
        // Clean up any state before returning to pool
    }
    
    public void TakeDamage(int i_Amount)
    {
        // Don't take damage if already dead
        if (m_IsDead) return;
        
        m_Hp -= i_Amount;
        AudioManager.Instance?.Play(eSFXId.EnemyHit, 0.5f);
        if (m_Hp <= 0)
        {
            die();
            // (Optional) flash/hit VFX hook here
        }
    }

    private void die()
    {
        // Prevent multiple death calls
        if (m_IsDead) return;
        m_IsDead = true;
        
        GameManager.Instance?.AddScore(100);
        PickupManager.Instance?.OnEnemyKilled(transform.position);
        PoolManager.Instance?.ReturnEnemy(this);
    }
}
