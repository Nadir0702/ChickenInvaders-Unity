using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [Header("Enemy Pooling")]
    [SerializeField] private EnemyController m_EnemyPrefab;
    [SerializeField] private int m_EnemyPoolSize = 30; // Increased from 20 to handle larger formations
    
    [Header("Food Pooling")]
    [SerializeField] private Food m_FoodPrefab;
    [SerializeField] private int m_FoodPoolSize = 50; // Food spawns very frequently
    
    [Header("Bullet Pooling")]
    [SerializeField] private Bullet m_BulletPrefab;
    [SerializeField] private int m_BulletPoolSize = 32; // Bullets fire very frequently
    
    [Header("Bomb Pooling")]
    [SerializeField] private Bomb m_BombPrefab;
    [SerializeField] private int m_BombPoolSize = 10;
    
    [Header("Enemy Bullet Pooling")]
    [SerializeField] private EnemyBullet m_EnemyBulletPrefab;
    [SerializeField] private int m_EnemyBulletPoolSize = 20; // Enemies shoot less frequently than player
    
    [Header("HUD Icon Pooling")]
    [SerializeField] private HUDIcon m_HUDIconPrefab;
    [SerializeField] private int m_HUDIconPoolSize = 20; // Max lives + bombs that could be displayed
    
    private ObjectPool<EnemyController> m_EnemyPool;
    private ObjectPool<Food> m_FoodPool;
    private ObjectPool<Bullet> m_BulletPool;
    private ObjectPool<Bomb> m_BombPool;
    private ObjectPool<EnemyBullet> m_EnemyBulletPool;
    private ObjectPool<HUDIcon> m_HUDIconPool;
    
    private void Start()
    {
        // Create pools
        m_EnemyPool = new ObjectPool<EnemyController>(m_EnemyPrefab, m_EnemyPoolSize, transform);
        m_FoodPool = new ObjectPool<Food>(m_FoodPrefab, m_FoodPoolSize, transform);
        m_BulletPool = new ObjectPool<Bullet>(m_BulletPrefab, m_BulletPoolSize, transform);
        m_BombPool = new ObjectPool<Bomb>(m_BombPrefab, m_BombPoolSize, transform);
        m_EnemyBulletPool = new ObjectPool<EnemyBullet>(m_EnemyBulletPrefab, m_EnemyBulletPoolSize, transform);
        
        // Create HUD Icon pool only if prefab is assigned
        if (m_HUDIconPrefab != null)
        {
            m_HUDIconPool = new ObjectPool<HUDIcon>(m_HUDIconPrefab, m_HUDIconPoolSize, transform);
        }
        else
        {
            Debug.LogWarning("PoolManager: HUD Icon prefab not assigned! HUD icons will not work until prefab is created and assigned.");
        }
        
        Debug.Log($"Pools initialized - "
                  + $"Enemies: {m_EnemyPoolSize},"
                  + $" Food: {m_FoodPoolSize},"
                  + $" Bullets: {m_BulletPoolSize},"
                  + $" Bombs: {m_BombPoolSize},"
                  + $" EnemyBullets: {m_EnemyBulletPoolSize},"
                  + $" HUDIcons: {(m_HUDIconPool != null ? m_HUDIconPoolSize.ToString() : "DISABLED")}");
        
        // Refresh HUD display now that pools are ready
        UIManager.Instance?.RefreshHUDDisplay();
    }
    
    public EnemyController GetEnemy(Vector3 i_Position, Quaternion i_Rotation = default)
    {
        if (m_EnemyPool.PoolSize == 0)
        {
            Debug.LogWarning($"PoolManager: Enemy pool is empty! Created: {m_EnemyPool.TotalCreated}, Available: {m_EnemyPool.PoolSize}");
        }
        
        var enemy = m_EnemyPool.Get(i_Position, i_Rotation);
        if (enemy == null)
        {
            Debug.LogError("PoolManager: Failed to get enemy from pool - returned null!");
        }
        
        return enemy;
    }
    
    public void ReturnEnemy(EnemyController i_Enemy)
    {
        m_EnemyPool.Return(i_Enemy);
    }
    
    public Food GetFood(Vector3 i_Position)
    {
        return m_FoodPool.Get(i_Position);
    }
    
    public void ReturnFood(Food i_Food)
    {
        m_FoodPool.Return(i_Food);
    }
    
    public Bullet GetBullet(Vector3 i_Position, Quaternion i_Rotation = default)
    {
        return m_BulletPool.Get(i_Position, i_Rotation);
    }
    
    public void ReturnBullet(Bullet i_Bullet)
    {
        m_BulletPool.Return(i_Bullet);
    }
    
    public Bomb GetBomb(Vector3 i_Position, Quaternion i_Rotation = default)
    {
        return m_BombPool.Get(i_Position, i_Rotation);
    }
    
    public void ReturnBomb(Bomb i_Bomb)
    {
        m_BombPool.Return(i_Bomb);
    }
    
    public EnemyBullet GetEnemyBullet(Vector3 i_Position, Quaternion i_Rotation = default)
    {
        return m_EnemyBulletPool.Get(i_Position, i_Rotation);
    }
    
    public void ReturnEnemyBullet(EnemyBullet i_EnemyBullet)
    {
        m_EnemyBulletPool.Return(i_EnemyBullet);
    }
    
    public HUDIcon GetHUDIcon(Vector3 i_Position)
    {
        if (m_HUDIconPool == null)
        {
            Debug.LogWarning("PoolManager: HUD Icon pool not initialized yet!");
            return null;
        }
        return m_HUDIconPool.Get(i_Position);
    }
    
    public void ReturnHUDIcon(HUDIcon i_HUDIcon)
    {
        if (m_HUDIconPool == null)
        {
            Debug.LogWarning("PoolManager: HUD Icon pool not initialized yet!");
            return;
        }
        m_HUDIconPool.Return(i_HUDIcon);
    }
    
    /// <summary>
    /// Check if HUD Icon pool is ready for use
    /// </summary>
    public bool IsHUDIconPoolReady()
    {
        return m_HUDIconPool != null;
    }
    
    // Method to check if we have enough enemies available for a formation
    public bool HasEnoughEnemies(int i_RequiredCount)
    {
        bool hasEnough = m_EnemyPool.PoolSize >= i_RequiredCount;
        if (!hasEnough)
        {
            Debug.LogWarning($"PoolManager: Not enough enemies available! Required: {i_RequiredCount}, Available: {m_EnemyPool.PoolSize}");
        }
        return hasEnough;
    }
    
    public int GetAvailableEnemyCount()
    {
        return m_EnemyPool.PoolSize;
    }
    
    /// <summary>
    /// Reset all pools by returning all active objects - called when restarting game
    /// </summary>
    public void ResetAllPools()
    {
        resetEnemyPool();
        resetFoodPool();
        resetBulletPool();
        resetBombPool();
        resetEnemyBulletPool();
        // Note: HUD icons are NOT reset here as they need to persist during game state changes
        // They are managed separately by UIManager based on game state
    }
    
    /// <summary>
    /// Reset only HUD icon pool - called separately when needed
    /// </summary>
    public void ResetHUDIconPool()
    {
        resetHUDIconPool();
    }

    private void resetBombPool()
    {
        var activeBombs = FindObjectsByType<Bomb>(FindObjectsSortMode.None);
        foreach (var bomb in activeBombs)
        {
            if (bomb.gameObject.activeInHierarchy)
            {
                ReturnBomb(bomb);
            }
        }
    }

    private void resetBulletPool()
    {
        var activeBullets = FindObjectsByType<Bullet>(FindObjectsSortMode.None);
        foreach (var bullet in activeBullets)
        {
            if (bullet.gameObject.activeInHierarchy)
            {
                ReturnBullet(bullet);
            }
        }
    }

    private void resetFoodPool()
    {
        var activeFood = FindObjectsByType<Food>(FindObjectsSortMode.None);
        foreach (var food in activeFood)
        {
            if (food.gameObject.activeInHierarchy)
            {
                ReturnFood(food);
            }
        }
    }

    private void resetEnemyPool()
    {
        var activeEnemies = FindObjectsByType<EnemyController>(FindObjectsSortMode.None);
        foreach (var enemy in activeEnemies)
        {
            if (enemy.gameObject.activeInHierarchy)
            {
                ReturnEnemy(enemy);
            }
        }
    }

    private void resetEnemyBulletPool()
    {
        var activeEnemyBullets = FindObjectsByType<EnemyBullet>(FindObjectsSortMode.None);
        foreach (var enemyBullet in activeEnemyBullets)
        {
            if (enemyBullet.gameObject.activeInHierarchy)
            {
                ReturnEnemyBullet(enemyBullet);
            }
        }
    }
    
    private void resetHUDIconPool()
    {
        var activeHUDIcons = FindObjectsByType<HUDIcon>(FindObjectsSortMode.None);
        foreach (var hudIcon in activeHUDIcons)
        {
            if (hudIcon.gameObject.activeInHierarchy)
            {
                ReturnHUDIcon(hudIcon);
            }
        }
    }
}
