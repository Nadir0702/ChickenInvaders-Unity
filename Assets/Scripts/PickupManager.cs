using UnityEngine;

public class PickupManager : Singleton<PickupManager>
{
    [Header("Prefabs")]
    [SerializeField] private GameObject m_WeaponPowerUpPrefab;
    [SerializeField] private GameObject m_FoodPrefab;
    
    [Header("Wave Tracking")]
    private int m_WeaponPowerUpsDroppedThisWave;
    private const int k_MaxWeaponPowerUps = 2;
    private const int k_MinWeaponPowerUps = 1;
    
    public void OnWaveStart()
    {
        m_WeaponPowerUpsDroppedThisWave = 0;
    }
    
    public void OnEnemyKilled(Vector3 i_Position)
    {
        // Always drop food
        if (m_FoodPrefab)
        {
            Instantiate(m_FoodPrefab, i_Position, Quaternion.identity);
        }
        
        // Check if we should drop weapon powerup
        tryDropWeaponPowerUp(i_Position);
    }
    
    private void tryDropWeaponPowerUp(Vector3 i_Position)
    {
        // Don't drop more than max per wave
        if (m_WeaponPowerUpsDroppedThisWave >= k_MaxWeaponPowerUps) return;
        
        // Calculate drop chance based on how many we've already dropped
        float dropChance;
        if (m_WeaponPowerUpsDroppedThisWave < k_MinWeaponPowerUps)
        {
            dropChance = 0.1f; // 10% chance for minimum drops
        }
        else
        {
            dropChance = 0.05f; // 5% chance for additional drops
        }
        
        if (Random.value <= dropChance && m_WeaponPowerUpPrefab)
        {
            Instantiate(m_WeaponPowerUpPrefab, i_Position, Quaternion.identity);
            m_WeaponPowerUpsDroppedThisWave++;
        }
    }
}
