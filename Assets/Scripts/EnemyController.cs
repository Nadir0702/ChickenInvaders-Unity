using UnityEngine;

public class EnemyController : MonoBehaviour, IDamageable
{
    [SerializeField] private int m_MaxHp = 3;
    private int m_Hp;
    
    void onEnable() => m_Hp = m_MaxHp;
    
    public void TakeDamage(int i_Amount)
    {
        m_Hp -= i_Amount;
        if (m_Hp <= 0)
        {
            die();
            // (Optional) flash/hit VFX hook here
        }
    }

    private void die()
    {
        // For now: Destroy. (We’ll pool enemies later.)
        Destroy(gameObject);
        UIManager.Instance.AddScore(10);
    }
}
