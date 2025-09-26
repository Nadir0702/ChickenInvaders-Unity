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
    
    private ObjectPool<EnemyController> m_EnemyPool;
    private ObjectPool<Food> m_FoodPool;
    private ObjectPool<Bullet> m_BulletPool;
    private ObjectPool<Bomb> m_BombPool;
    
    private void Start()
    {
        // Create pools
        m_EnemyPool = new ObjectPool<EnemyController>(m_EnemyPrefab, m_EnemyPoolSize, transform);
        m_FoodPool = new ObjectPool<Food>(m_FoodPrefab, m_FoodPoolSize, transform);
        m_BulletPool = new ObjectPool<Bullet>(m_BulletPrefab, m_BulletPoolSize, transform);
        m_BombPool = new ObjectPool<Bomb>(m_BombPrefab, m_BombPoolSize, transform);
        
        Debug.Log($"Pools initialized - "
                  + $"Enemies: {m_EnemyPoolSize},"
                  + $" Food: {m_FoodPoolSize},"
                  + $" Bullets: {m_BulletPoolSize}"
                  + $" Bombs: {m_BombPoolSize}");
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
    
    // Debug info
    public void LogPoolStats()
    {
        Debug.Log($"Pool Stats - " +
                  $"Enemies: {m_EnemyPool.PoolSize}/{m_EnemyPool.TotalCreated}, " +
                  $"Food: {m_FoodPool.PoolSize}/{m_FoodPool.TotalCreated}, " +
                  $"Bullets: {m_BulletPool.PoolSize}/{m_BulletPool.TotalCreated}, " +
                  $"Bombs: {m_BombPool.PoolSize}/{m_BombPool.TotalCreated}");
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
}
