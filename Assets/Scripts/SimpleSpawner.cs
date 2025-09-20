using UnityEngine;

public class SimpleSpawner : MonoBehaviour
{
    [SerializeField] private EnemyController m_EnemyPrefab;
    [SerializeField] private float m_SpawnInterval = 1.2f;
    
    private Camera m_Camera;
    private float m_Timer;
    
    void Awake()
    {
        m_Camera = Camera.main;
    }
    
    void Update()
    {
        m_Timer += Time.deltaTime;
        if (m_Timer >= m_SpawnInterval)
        {
            m_Timer = 0f;
            spawnEnemy();
        }
    }

    private void spawnEnemy()
    {
        Vector3 min = m_Camera.ViewportToWorldPoint(new Vector3(0, 1, 0));
        Vector3 max = m_Camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        float x = Random.Range(min.x, max.x);
        Vector3 spawnPos = new Vector3(x, max.y + 1f, 0);
        Instantiate(m_EnemyPrefab, spawnPos, Quaternion.identity);
    }
}
