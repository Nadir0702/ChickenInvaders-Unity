using UnityEngine;
using System.Collections;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private ParallaxManager m_ParallaxManager;
    [SerializeField] private float m_ScrollSpeed = 0.5f;
    [SerializeField] private int m_LayerIndex; 
    [SerializeField] private Transform m_FirstTile;
    [SerializeField] private Transform m_SecondTile;
    
    // Speed transition system
    private float m_BaseSpeed; // Original speed
    private float m_CurrentSpeed; // Current active speed
    private float m_TargetSpeed; // Target speed for transitions
    private Coroutine m_SpeedTransitionCoroutine;
    
    private float m_Height;
    private Camera m_Camera;
    
    private void Awake()
    {
        m_Camera = Camera.main;
        m_FirstTile.GetComponent<SpriteRenderer>().sprite = m_ParallaxManager.GetLayerSprite(m_LayerIndex);
        m_SecondTile.GetComponent<SpriteRenderer>().sprite = m_ParallaxManager.GetLayerSprite(m_LayerIndex);
        m_Height = m_FirstTile.GetComponent<SpriteRenderer>().bounds.size.y;
        m_SecondTile.position = m_FirstTile.position + new Vector3(0, m_Height, 0);
        
        // Initialize speed values
        m_BaseSpeed = m_ScrollSpeed;
        m_CurrentSpeed = m_ScrollSpeed;
        m_TargetSpeed = m_ScrollSpeed;
    }
    
    private void Start()
    {
        // Register with ParallaxManager for acceleration control
        if (ParallaxManager.Instance != null)
        {
            ParallaxManager.Instance.RegisterLayer(this);
        }
    }
    
    private void OnDestroy()
    {
        // Unregister from ParallaxManager
        if (ParallaxManager.Instance != null)
        {
            ParallaxManager.Instance.UnregisterLayer(this);
        }
        
        // Stop any ongoing transitions
        if (m_SpeedTransitionCoroutine != null)
        {
            StopCoroutine(m_SpeedTransitionCoroutine);
        }
    }

    private void Update()
    {
        float dy = -m_CurrentSpeed * Time.deltaTime;
        m_FirstTile.Translate(0, dy, 0, Space.World);
        m_SecondTile.Translate(0, dy, 0, Space.World);
        
        var camY = m_Camera.transform.position.y;
        if (m_FirstTile.position.y < camY - m_Height) m_FirstTile.position += new Vector3(0, 2 * m_Height, 0);
        if (m_SecondTile.position.y < camY - m_Height) m_SecondTile.position += new Vector3(0, 2 * m_Height, 0);
    }
    
    /// <summary>
    /// Smoothly transition to a new speed over the specified duration
    /// </summary>
    public void TransitionToSpeed(float i_TargetSpeed, float i_Duration)
    {
        if (m_SpeedTransitionCoroutine != null)
        {
            StopCoroutine(m_SpeedTransitionCoroutine);
        }
        
        m_TargetSpeed = i_TargetSpeed;
        m_SpeedTransitionCoroutine = StartCoroutine(SpeedTransitionCoroutine(i_Duration));
    }
    
    /// <summary>
    /// Coroutine to handle smooth speed transitions
    /// </summary>
    private IEnumerator SpeedTransitionCoroutine(float i_Duration)
    {
        float startSpeed = m_CurrentSpeed;
        float timer = 0f;
        
        while (timer < i_Duration)
        {
            timer += Time.deltaTime;
            float normalizedTime = timer / i_Duration;
            
            // Use smooth ease-in-out curve for natural acceleration/deceleration
            float smoothTime = Mathf.SmoothStep(0f, 1f, normalizedTime);
            m_CurrentSpeed = Mathf.Lerp(startSpeed, m_TargetSpeed, smoothTime);
            
            yield return null;
        }
        
        // Ensure we reach the exact target speed
        m_CurrentSpeed = m_TargetSpeed;
        m_SpeedTransitionCoroutine = null;
    }
    
    /// <summary>
    /// Get the base (original) speed of this layer
    /// </summary>
    public float GetBaseSpeed() => m_BaseSpeed;
}
