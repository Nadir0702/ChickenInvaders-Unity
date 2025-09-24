using UnityEngine;

public class PoolManager : Singleton<PoolManager>
{
    [Header("Enemy Pooling")]
    [SerializeField] private EnemyController m_EnemyPrefab;
    [SerializeField] private int m_EnemyPoolSize = 20;
    
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
        return m_EnemyPool.Get(i_Position, i_Rotation);
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
        Debug.Log($"Pool Stats - Enemies: {m_EnemyPool.PoolSize}, Food: {m_FoodPool.PoolSize}, Bullets: {m_BulletPool.PoolSize}");
    }
}
