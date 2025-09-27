using System.Collections;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Weapon")]
    [Range(1, 8), SerializeField] private int m_WeaponLevel = 1;

    [Header("Shield")]
    [SerializeField] private float m_ShieldDuration = 4f;
    [SerializeField] private GameObject m_ShieldVisualPrefab;
    private GameObject m_ShieldVisualInstance;
    public bool ShieldActive { get; private set; }
    
    [Header("Bombs")]
    [SerializeField] private int m_BombCount;
    private const int k_MaxBombs = 10;

    private void Start()
    {
        UIManager.Instance?.SetWeaponLevel(WeaponLevel);
        UIManager.Instance?.SetBombs(m_BombCount);
        
        // Create shield visual instance once at start and keep it disabled
        if (m_ShieldVisualPrefab && !m_ShieldVisualInstance)
        {
            m_ShieldVisualInstance = Instantiate(m_ShieldVisualPrefab, transform);
            m_ShieldVisualInstance.transform.localPosition = Vector3.zero;
            m_ShieldVisualInstance.SetActive(false); // Start disabled
        }
    }

    public int WeaponLevel
    {
        get => m_WeaponLevel;
        private set => m_WeaponLevel = value;
    }
    
    public int BombCount => m_BombCount;
    
    public void AddWeaponLevel(int i_Delta)
    {
        WeaponLevel = Mathf.Clamp(WeaponLevel + i_Delta, 1, 8);
        UIManager.Instance?.SetWeaponLevel(WeaponLevel);
    }
    
    public void AddBomb(int i_Delta)
    {
        m_BombCount = Mathf.Clamp(m_BombCount + i_Delta, 0, k_MaxBombs);
        UIManager.Instance?.SetBombs(m_BombCount);
    }
    
    /// <summary>
    /// Reset player stats to initial values - called when starting new game
    /// </summary>
    public void ResetStats()
    {
        // Reset to initial values
        m_WeaponLevel = 1;
        m_BombCount = 0;
        
        // Stop any active shield
        StopAllCoroutines();
        ShieldActive = false;
        if (m_ShieldVisualInstance)
        {
            m_ShieldVisualInstance.SetActive(false);
        }
        
        // Update UI
        UIManager.Instance?.SetWeaponLevel(WeaponLevel);
        UIManager.Instance?.SetBombs(m_BombCount);
    }
    
    public void ActivateShield()
    {
        if (ShieldActive) StopAllCoroutines();
        StartCoroutine(shieldCR(m_ShieldDuration));
    }

    private IEnumerator shieldCR(float i_Duration)
    {
        ShieldActive = true;
        if(m_ShieldVisualInstance)
        {
            m_ShieldVisualInstance.SetActive(true);
        }
        
        yield return new WaitForSeconds(i_Duration);
        
        ShieldActive = false;
        if(m_ShieldVisualInstance)
        {
            m_ShieldVisualInstance.SetActive(false);
        }
    }
}
