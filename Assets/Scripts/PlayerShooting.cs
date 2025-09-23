using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [SerializeField] private PlayerStats m_PlayerStats;
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
            firePattern(m_PlayerStats.WeaponLevel);
        }
    }
    
    private void firePattern(int i_WeaponLevel)
    {
        spawnBullet(Vector2.up, 0f);
        
        if (i_WeaponLevel >= 3) spawnDirectionSpread(2, 7f);
        if (i_WeaponLevel >= 5) spawnDirectionSpread(3, 13f);
        if (i_WeaponLevel >= 7) spawnDirectionSpread(4, 20f);
    }

    private void spawnDirectionSpread(int i_Pairs, float i_StepDegree)
    {
        for(int i = 1; i < i_Pairs; i++)
        {
            spawnBullet(Vector2.up, i_StepDegree);
            spawnBullet(Vector2.up, -i_StepDegree);
        }
    }

    private void spawnBullet(Vector2 i_Direction, float i_AngleOffset)
    {
        var bullet = BulletPool.Instance.Get();
        bullet.transform.position = m_Muzzle.position;
        bullet.transform.rotation = Quaternion.identity;
        var angle = Quaternion.Euler(0, 0, i_AngleOffset) * i_Direction;
        bullet.Fire(angle);
    }
}