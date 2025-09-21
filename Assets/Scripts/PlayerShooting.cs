using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private Transform m_Muzzle;
    [SerializeField] private float m_FireRate = 8f; // shots per second
    
    private float m_NextFireTime;

    void Update()
    {
        if (GameManager.Instance.GameState != eGameState.Playing) return;
        
        bool firePressed = Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0);
        if (firePressed && Time.time >= m_NextFireTime)
        {
            m_NextFireTime = Time.time + (1f / m_FireRate);
            spawnBullet();
        }
    }

    private void spawnBullet()
    {
        var bullet = BulletPool.Instance.Get();
        bullet.transform.position = m_Muzzle.position;
        bullet.transform.rotation = m_Muzzle.rotation;
        bullet.Fire(Vector2.up); // player bullets go up
        AudioManager.Instance?.Play(eSFXId.Shoot, 0.6f, Random.Range(0.95f, 1.05f));
    }
}