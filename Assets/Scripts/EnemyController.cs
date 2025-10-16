using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private GameObject[] m_DropPrefabs;
    [SerializeField] private int m_BaseMaxHp = 1; // Base HP before scaling
    [SerializeField] private EnemyShootingSimple m_EnemyShooting; // Shooting component reference
    private int m_MaxHp = 1; // Actual max HP after scaling  
    private int m_Hp = 1;
    private bool m_IsDead = false; // Prevent multiple death calls

    private void OnEnable() => m_Hp = m_MaxHp;
    
    public void OnPoolGet()
    {
        // Calculate scaled HP based on current difficulty
        m_MaxHp = m_BaseMaxHp + (GameManager.Instance.CurrentDifficultyTier - 1);
        m_Hp = m_MaxHp; // Set current HP to max HP
        m_IsDead = false; // Reset death state
    }
    
    public void OnPoolReturn()
    {
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
