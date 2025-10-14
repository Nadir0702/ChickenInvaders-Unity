using UnityEngine;

public class BossController : MonoBehaviour, IDamageable
{
    [Header("Boss Settings")]
    [SerializeField] private int m_BaseMaxHp = 50; // Boss base HP
    [SerializeField] private int m_ScoreReward = 5000; // Points awarded for killing boss
    [SerializeField] private int m_MinFoodDrops = 10; // Minimum food drops
    [SerializeField] private int m_MaxFoodDrops = 20; // Maximum food drops
    
    [Header("Components")]
    [SerializeField] private BossShooting m_BossShooting; // Boss shooting component
    
    private int m_MaxHp = 50; // Actual max HP after scaling
    private int m_Hp = 50;
    private bool m_IsDead = false; // Prevent multiple death calls
    private bool m_IsActive = false; // Track if boss is currently active
    
    // Debug properties for inspector visibility
    public int CurrentHP => m_Hp;
    public int MaxHP => m_MaxHp;
    public bool IsActive => m_IsActive;
    
    private void OnEnable()
    {
        initializeBoss();
    }
    
    /// <summary>
    /// Initialize boss when spawned
    /// </summary>
    public void InitializeBoss()
    {
        initializeBoss();
    }
    
    private void initializeBoss()
    {
        // Calculate scaled HP based on current difficulty (bosses scale more than regular enemies)
        int difficultyBonus = (GameManager.Instance.CurrentDifficultyTier - 1) * 25; // 25 HP per tier
        m_MaxHp = m_BaseMaxHp + difficultyBonus;
        m_Hp = m_MaxHp;
        m_IsDead = false;
        m_IsActive = true;
        
        // Enable shooting if component exists
        if (m_BossShooting != null)
        {
            m_BossShooting.enabled = true;
        }
        
        Debug.Log($"Boss initialized with {m_MaxHp} HP (Difficulty Tier: {GameManager.Instance.CurrentDifficultyTier})");
    }
    
    public void TakeDamage(int i_Amount)
    {
        // Don't take damage if already dead or not active
        if (m_IsDead || !m_IsActive) return;
        
        m_Hp -= i_Amount;
        AudioManager.Instance?.Play(eSFXId.BossHit, 0.7f, 0.8f); // Slightly different pitch for boss
        
        // Optional: Add boss hit visual effects here
        
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
        m_IsActive = false;
        
        // Disable shooting
        if (m_BossShooting != null)
        {
            m_BossShooting.enabled = false;
        }
        
        // Award score
        GameManager.Instance?.AddScore(m_ScoreReward);
        
        // Drop multiple food items
        dropFood();
        
        // Play boss death sound (using existing enemy hit sound for now)
        AudioManager.Instance?.Play(eSFXId.BossHit, 1.0f, 0.6f);
        
        // Fade out boss music
        AudioManager.Instance?.OnBossDefeated();
        
        // Notify wave director that boss is dead
        WaveDirector.Instance?.OnBossKilled();
        
        // Destroy boss (since it's not pooled like regular enemies)
        Destroy(gameObject, 0.1f); // Small delay to allow sound to play
        
        Debug.Log($"Boss defeated! Awarded {m_ScoreReward} points and dropped food.");
    }
    
    private void dropFood()
    {
        // Drop random amount of food between min and max
        int foodCount = Random.Range(m_MinFoodDrops, m_MaxFoodDrops + 1);
        
        for (int i = 0; i < foodCount; i++)
        {
            // Spread food drops around boss position
            Vector3 dropPosition = transform.position + new Vector3(
                Random.Range(-2f, 2f), 
                Random.Range(-1f, 1f), 
                0f
            );
            
            // Use pickup manager to spawn food
            PickupManager.Instance?.SpawnFood(dropPosition);
        }
    }
    
    /// <summary>
    /// Deactivate boss (called when boss moves off screen or game resets)
    /// </summary>
    public void DeactivateBoss()
    {
        m_IsActive = false;
        
        // Disable shooting
        if (m_BossShooting != null)
        {
            m_BossShooting.enabled = false;
        }
        
        // Stop boss music if it's playing (for game resets)
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }
        
        // Destroy boss object
        Destroy(gameObject);
    }
}
