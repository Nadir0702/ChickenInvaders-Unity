using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable, IPoolable
{
    [SerializeField] private GameObject[] m_DropPrefabs;
    // [SerializeField, Range(0,1)] private float m_DropChance = 0.25f;
    [SerializeField] private int m_MaxHp = 5;
    private int m_Hp = 1;
    
    private void OnEnable() => m_Hp = m_MaxHp;
    
    public void OnPoolGet()
    {
        m_Hp = 1;
        // Reset any other state if needed
    }
    
    public void OnPoolReturn()
    {
        // Clean up any state before returning to pool
    }
    
    public void TakeDamage(int i_Amount)
    {
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
        GameManager.Instance?.AddScore(100);
        PickupManager.Instance?.OnEnemyKilled(transform.position);
        PoolManager.Instance?.ReturnEnemy(this);
    }
}
