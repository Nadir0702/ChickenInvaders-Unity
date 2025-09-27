using UnityEngine;

public class PickupManager : Singleton<PickupManager>
{
    [Header("Prefabs")]
    [SerializeField] private GameObject m_WeaponPowerUpPrefab;
    [SerializeField] private GameObject m_FoodPrefab;
    
    [Header("Wave Tracking")]
    private int m_WeaponPowerUpsDroppedThisWave;
    private const int k_MaxWeaponPowerUps = 1;
    private const int k_MinWeaponPowerUps = 0;
    
    [Header("Settings")]
    [SerializeField] private float m_FoodDropChance = 0.75f;
    
    public void OnWaveStart()
    {
        m_WeaponPowerUpsDroppedThisWave = 0;
    }
    
    public void OnEnemyKilled(Vector3 i_Position)
    {
        if (Random.value <= m_FoodDropChance)
        {
            PoolManager.Instance?.GetFood(i_Position);
        }
        
        // Check if we should drop weapon power up
        tryDropWeaponPowerUp(i_Position);
    }
    
    private void tryDropWeaponPowerUp(Vector3 i_Position)
    {
        // Don't drop more than max per wave
        if (m_WeaponPowerUpsDroppedThisWave >= k_MaxWeaponPowerUps) return;
        
        // Calculate drop chance based on how many we've already dropped
        float dropChance = 0;
        if (m_WeaponPowerUpsDroppedThisWave == k_MinWeaponPowerUps)
        {
            dropChance = 0.1f; // 10% chance for minimum drops
        }
        
        if (Random.value <= dropChance && m_WeaponPowerUpPrefab)
        {
            Instantiate(m_WeaponPowerUpPrefab, i_Position, Quaternion.identity);
            m_WeaponPowerUpsDroppedThisWave++;
        }
    }
}
