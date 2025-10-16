using UnityEngine;

public class BossController : MonoBehaviour, IDamageable
{
    private static readonly int sr_Explode = Animator.StringToHash("Explode");
    [Header("Boss Settings")]
    [SerializeField] private int m_BaseMaxHp = 50; // Boss base HP
    [SerializeField] private int m_ScoreReward = 5000; // Points awarded for killing boss
    [SerializeField] private int m_MinFoodDrops = 10; // Minimum food drops
    [SerializeField] private int m_MaxFoodDrops = 20; // Maximum food drops
    [SerializeField] private float m_ExplosionAnimationDuration = 1.5f; // Duration of explosion animation
    
    [Header("Components")]
    [SerializeField] private BossShooting m_BossShooting; // Boss shooting component
    [SerializeField] private Animator m_Animator;
    
    private int m_MaxHp = 50; // Actual max HP after scaling
    private int m_Hp = 50;
    private bool m_IsDead = false; // Prevent multiple death calls
    private bool m_IsActive = false; // Track if boss is currently active
    private Coroutine m_ExplosionCoroutine; // Track explosion animation coroutine
    
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
        AudioManager.Instance?.Play(eSFXId.BossHit, 0.3f, 0.8f); // Reduced volume for boss hits
        
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
        
        // Disable shooting immediately
        if (m_BossShooting != null)
        {
            m_BossShooting.enabled = false;
        }
        
        // Disable collider to prevent collision with player during death animation
        var collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        
        // Stop movement by disabling BossMover component
        var bossMover = GetComponent<BossMover>();
        if (bossMover != null)
        {
            bossMover.enabled = false;
        }
        
        // Start explosion sequence - this will handle all the effects synchronously
        m_ExplosionCoroutine = StartCoroutine(explosionSequence());
    }
    
    /// <summary>
    /// Handle the complete explosion sequence with proper timing
    /// </summary>
    private System.Collections.IEnumerator explosionSequence()
    {
        // Force immediate animation start by updating animator immediately
        m_Animator.SetTrigger(sr_Explode);
        m_Animator.Update(0f); // Force animator to process the trigger immediately
        
        // Wait one frame to ensure animation has started
        yield return null;
        
        // All effects happen at the exact moment the explosion animation starts
        AudioManager.Instance?.Play(eSFXId.Explosion, 0.4f, 0.5f); // Reduced from 0.7f
        AudioManager.Instance?.Play(eSFXId.BossHit, 0.5f, 0.6f);   // Reduced from 0.8f
        GameManager.Instance?.AddScore(m_ScoreReward);
        dropFood();
        
        // Notify systems
        AudioManager.Instance?.OnBossDefeated();
        WaveDirector.Instance?.OnBossKilled();
        
        // Show boss defeated congratulations message after 0.5 seconds
        StartCoroutine(ShowBossDefeatedMessageDelayed());
        
        Debug.Log($"Boss defeated! Awarded {m_ScoreReward} points and dropped food.");
        
        // Wait for explosion animation to complete (minus the frame we already waited)
        yield return new WaitForSeconds(m_ExplosionAnimationDuration - Time.deltaTime);
        
        // Clean up
        m_ExplosionCoroutine = null;
        Destroy(gameObject);
        Debug.Log("Boss explosion animation completed, boss destroyed.");
    }
    
    private System.Collections.IEnumerator ShowBossDefeatedMessageDelayed()
    {
        yield return new WaitForSeconds(0.5f);
        InterWaveMessageManager.Instance?.ShowBossDefeatedMessage();
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
        
        // Stop explosion coroutine if it's running (for game resets)
        if (m_ExplosionCoroutine != null)
        {
            StopCoroutine(m_ExplosionCoroutine);
            m_ExplosionCoroutine = null;
        }
        
        // Destroy boss object
        Destroy(gameObject);
    }
}
