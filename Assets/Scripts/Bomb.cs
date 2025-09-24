using UnityEngine;

public class Bomb : MonoBehaviour, IPoolable
{
    [SerializeField] private float m_Speed = 5f;
    [SerializeField] private int m_BombDamage = 999;
    
    private Transform m_Player;
    private Vector3 m_ExplosionLocation;
    private Vector3 m_LaunchDirection;
    private Camera m_Camera;
    private bool m_Active;

    private void Awake() => m_Camera = Camera.main;
    
    private void Update()
    {
        if (!m_Active) return;
        
        // Check if we've reached the explosion location
        if (Vector3.Distance(transform.position, m_ExplosionLocation) < 0.1f)
        {
            detonate();
            return;
        }
        
        // Move toward explosion location
        transform.Translate(m_Speed * Time.deltaTime * m_LaunchDirection, Space.World);
    }
    
    private void detonate()
    {
        // Damage all enemies on screen
        foreach(var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
        {
            if(enemy) enemy.TakeDamage(m_BombDamage);
        }
        
        // TODO (when you add enemy bullets): clear all EnemyBullet layer instances.
        // foreach (var b in FindObjectsOfType<EnemyBullet>()) Destroy(b.gameObject);
        
        AudioManager.Instance?.Play(eSFXId.Explosion);
        PoolManager.Instance?.ReturnBomb(this);
    }

    public void OnPoolGet()
    {
        m_Active = true;
        
        // Find player transform and set bomb position to player position
        var player = FindFirstObjectByType<PlayerBombing>();
        if (player) 
        {
            m_Player = player.transform;
            transform.position = m_Player.position;
        }
        
        // Calculate explosion location (center of screen)
        if (m_Camera)
        {
            m_ExplosionLocation = m_Camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            m_ExplosionLocation.z = transform.position.z; // Keep same Z position
        }
        
        // Calculate launch direction from player to explosion location
        m_LaunchDirection = (m_ExplosionLocation - transform.position).normalized;
        
        // Rotate bomb to face the explosion location
        float angle = Mathf.Atan2(m_LaunchDirection.y, m_LaunchDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    public void OnPoolReturn()
    {
        m_Active = false;
    }
}
