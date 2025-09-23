using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject[] m_DropPrefabs;
    [SerializeField, Range(0,1)] private float m_DropChance = 0.25f;
    [SerializeField] private int m_MaxHp = 3;
    private int m_Hp;
    
    private void onEnable() => m_Hp = m_MaxHp;
    
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
        // For now: Destroy. (We'll pool enemies later.)
        GameManager.Instance?.AddScore(100);
        
        // Use new pickup system
        PickupManager.Instance?.OnEnemyKilled(transform.position);
        
        Destroy(gameObject);
    }
}
