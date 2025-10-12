using UnityEngine;

public class EnemyShootingSimple : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private float m_MinShootInterval = 5f; // Minimum time between shots (increased from 3f)
    [SerializeField] private float m_MaxShootInterval = 12f; // Maximum time between shots (increased from 8f)
    
    private float m_NextShootTime;

    private void OnEnable()
    {
        // Schedule first shot
        scheduleNextShot();
    }

    private void Update()
    {
        // Only shoot if game is playing and we can shoot
        if (GameManager.Instance?.GameState != eGameState.Playing)
            return;

        // Check if it's time to shoot
        if (Time.time >= m_NextShootTime)
        {
            shootEgg();
            AudioManager.Instance?.Play(eSFXId.LayEgg, 0.5f);
            
            // Schedule next shot
            scheduleNextShot();
        }
    }

    private void shootEgg()
    {
        // Get egg from pool and fire it downward
        var egg = PoolManager.Instance?.GetEnemyBullet(transform.position);
        if (egg != null)
        {
            egg.Fire(Vector2.down);
        }
    }

    private void scheduleNextShot()
    {
        // Set next shoot time to a random interval - this is the ONLY randomization needed
        float interval = Random.Range(m_MinShootInterval, m_MaxShootInterval);
        m_NextShootTime = Time.time + interval;
    }

    
}
