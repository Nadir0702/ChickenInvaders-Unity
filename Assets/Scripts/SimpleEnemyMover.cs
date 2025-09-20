using UnityEngine;

public class SimpleEnemyMover : MonoBehaviour
{
    [SerializeField] private float m_Speed = 2.5f;
    private Camera m_Camera;
    
    private void Awake()
    {
        m_Camera = Camera.main;
    }
    
    private void Update()
    {
        transform.Translate( m_Speed * Time.deltaTime * Vector2.down, Space.World);
        
        // Destroy if out of screen
        Vector3 viewPortPosition = m_Camera.WorldToViewportPoint(transform.position);
        if (viewPortPosition.y < 0.1f)
        {
            Destroy(gameObject);
        }
    }
}
