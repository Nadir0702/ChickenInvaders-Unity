using UnityEngine;

public class BossShooting : MonoBehaviour
{
    [Header("Boss Shooting Settings")]
    [SerializeField] private float m_MinShootInterval = 1.5f; // Much faster than regular enemies (was 5f)
    [SerializeField] private float m_MaxShootInterval = 3.5f; // Much faster than regular enemies (was 12f)
    [SerializeField] private int m_BurstCount = 1; // Number of eggs to shoot in a burst
    [SerializeField] private float m_BurstDelay = 0.2f; // Delay between eggs in a burst
    
    private float m_NextShootTime;
    private bool m_IsActive = true;
    
    private void OnEnable()
    {
        m_IsActive = true;
        // Schedule first shot
        scheduleNextShot();
    }
    
    private void OnDisable()
    {
        m_IsActive = false;
    }
    
    private void Update()
    {
        // Only shoot if active, game is playing, and we can shoot
        if (!m_IsActive || GameManager.Instance?.GameState != eGameState.Playing)
            return;
        
        // Check if it's time to shoot
        if (Time.time >= m_NextShootTime)
        {
            StartCoroutine(shootBurst());
            
            // Schedule next shot
            scheduleNextShot();
        }
    }
    
    private System.Collections.IEnumerator shootBurst()
    {
        for (int i = 0; i < m_BurstCount; i++)
        {
            shootEgg();
            AudioManager.Instance?.Play(eSFXId.LayEgg, 0.7f, Random.Range(0.8f, 1.2f)); // Vary pitch slightly
            
            // Wait between shots in burst (except for last shot)
            if (i < m_BurstCount - 1)
            {
                yield return new WaitForSeconds(m_BurstDelay);
            }
        }
    }
    
    private void shootEgg()
    {
        // Get egg from pool and fire it downward
        var egg = PoolManager.Instance?.GetEnemyBullet(transform.position);
        if (egg != null)
        {
            // Boss eggs could have slight random spread for more challenge
            Vector2 direction = Vector2.down;
            
            // Add slight random spread (optional - can be removed if too difficult)
            float spreadAngle = Random.Range(-10f, 10f); // Small spread angle
            direction = Quaternion.Euler(0, 0, spreadAngle) * direction;
            
            egg.Fire(direction);
        }
    }
    
    private void scheduleNextShot()
    {
        // Set next shoot time to a random interval
        float interval = Random.Range(m_MinShootInterval, m_MaxShootInterval);
        m_NextShootTime = Time.time + interval;
    }
    
    /// <summary>
    /// Increase shooting intensity (called when boss health is low)
    /// </summary>
    public void IncreaseIntensity()
    {
        // Make boss shoot faster when health is low
        m_MinShootInterval = Mathf.Max(0.8f, m_MinShootInterval * 0.7f);
        m_MaxShootInterval = Mathf.Max(2.0f, m_MaxShootInterval * 0.7f);
        m_BurstCount = Mathf.Min(3, m_BurstCount + 1); // Increase burst count
    }
    
    /// <summary>
    /// Set shooting active state
    /// </summary>
    public void SetActive(bool i_Active)
    {
        m_IsActive = i_Active;
    }
}
