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
        if(i_WeaponLevel % 2 == 0) spawnBullet(Vector2.up, 0f, true);
        else spawnBullet(Vector2.up, 0f);
        
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

    private void spawnBullet(Vector2 i_Direction, float i_AngleOffset, bool i_DoubleCenter = false)
    {
        if(i_DoubleCenter)
        {
            var bullet1 = PoolManager.Instance.GetBullet(m_Muzzle.position + Vector3.left * 0.2f, Quaternion.identity);
            var bullet2 = PoolManager.Instance.GetBullet(m_Muzzle.position + Vector3.right * 0.2f, Quaternion.identity);
            bullet1.Fire(Vector2.up);
            bullet2.Fire(Vector2.up);
            return;
        }
        var bullet = PoolManager.Instance.GetBullet(m_Muzzle.position, Quaternion.identity);
        var angle = Quaternion.Euler(0, 0, i_AngleOffset) * i_Direction;
        bullet.Fire(angle);
    }
}