using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    [SerializeField] private Bullet m_BulletPrefab;
    [SerializeField] private int m_InitialSize = 32;

    private readonly Queue<Bullet> r_Pool = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;

        for (int i = 0; i < m_InitialSize; i++)
            r_Pool.Enqueue(create());
    }

    private Bullet create()
    {
        var b = Instantiate(m_BulletPrefab, transform);
        b.gameObject.SetActive(false);
        return b;
    }

    public Bullet Get()
    {
        var bullet = r_Pool.Count > 0 ? r_Pool.Dequeue() : create();
        bullet.gameObject.SetActive(true);
        return bullet;
    }

    public void Release(Bullet i_Bullet)
    {
        i_Bullet.gameObject.SetActive(false);
        r_Pool.Enqueue(i_Bullet);
    }
}