using System.Collections;
using UnityEngine;

public class Bomb : MonoBehaviour, IPoolable
{
    private static readonly int sr_Launch = Animator.StringToHash("Launch");
    private static readonly int sr_Explode = Animator.StringToHash("Explode");
    [SerializeField] private float m_Speed = 5f;
    [SerializeField] private int m_BombDamage = 999;
    [SerializeField] private Animator m_Animator;
    [SerializeField] private float m_ExplosionAnimationDuration = 1.2f; // Duration of explosion animation
    
    private Transform m_Player;
    private Vector3 m_ExplosionLocation;
    private Vector3 m_LaunchDirection;
    private Camera m_Camera;
    private bool m_Active;
    private bool m_IsExploding;
    private Coroutine m_ExplosionCoroutine;

    private void Awake() => m_Camera = Camera.main;
    
    private void Update()
    {
        if (!m_Active) return;
        
        // If exploding, don't move - just wait for animation to finish
        if (m_IsExploding) return;
        
        // Check if we've reached the explosion location
        float currentDistance = Vector3.Distance(transform.position, m_ExplosionLocation);
        if (currentDistance < 0.1f)
        {
            detonate();
            return;
        }
        
        // Move toward explosion location
        transform.Translate(m_Speed * Time.deltaTime * m_LaunchDirection, Space.World);
    }
    
    private void detonate()
    {
        m_IsExploding = true;
        
        // Damage all enemies on screen
        foreach(var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
        {
            if(enemy) enemy.TakeDamage(m_BombDamage);
        }
        
        // TODO (when you add enemy bullets): clear all EnemyBullet layer instances.
        // foreach (var b in FindObjectsOfType<EnemyBullet>()) Destroy(b.gameObject);
        
        AudioManager.Instance?.Play(eSFXId.Explosion);
        m_Animator.SetTrigger(sr_Explode);
        
        // Wait for explosion animation to complete before returning to pool
        m_ExplosionCoroutine = StartCoroutine(waitForExplosionAnimation());
    }
    
    private IEnumerator waitForExplosionAnimation()
    {
        yield return new WaitForSeconds(m_ExplosionAnimationDuration);
        m_ExplosionCoroutine = null;
        PoolManager.Instance?.ReturnBomb(this);
    }

    public void OnPoolGet()
    {
        m_Active = true;
        m_IsExploding = false;
        m_ExplosionCoroutine = null;
        
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
        
        m_LaunchDirection = (m_ExplosionLocation - transform.position).normalized;
        float angle = Mathf.Atan2(m_LaunchDirection.y, m_LaunchDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        
        m_Animator.SetTrigger(sr_Launch);
    }

    public void OnPoolReturn()
    {
        m_Active = false;
        m_IsExploding = false;
        m_Animator.ResetTrigger(sr_Explode);
        m_Animator.ResetTrigger(sr_Launch);
        
        // Stop only our specific explosion coroutine if it's running
        if (m_ExplosionCoroutine != null)
        {
            StopCoroutine(m_ExplosionCoroutine);
            m_ExplosionCoroutine = null;
        }
    }
}
