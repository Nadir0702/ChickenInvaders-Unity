using UnityEngine;

public class BulletPool : Singleton<BulletPool>
{
    [SerializeField] private Bullet m_BulletPrefab;
    private BestObjectPool<Bullet> m_BulletPool;
    
    void Awake()
    {
        m_BulletPool = new BestObjectPool<Bullet>(m_BulletPrefab, defaultPoolSize: 20, maxSize: 50);
    }
    
    public Bullet GetBullet()
    {
        return m_BulletPool.GetObject();
    }
    
    public void ReturnBullet(Bullet i_Bullet)
    {
        m_BulletPool.ReleaseObject(i_Bullet);
    }
}
