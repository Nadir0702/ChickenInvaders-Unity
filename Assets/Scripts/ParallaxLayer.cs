using UnityEngine;
using UnityEngine.Serialization;

public class ParallaxLayer : MonoBehaviour
{
    [SerializeField] private ParallaxManager m_ParallaxManager;
    [SerializeField] private float m_ScrollSpeed = 0.5f;
    [SerializeField] private int m_LayerIndex; 
    [SerializeField] private Transform m_FirstTile;
    [SerializeField] private Transform m_SecondTile;
    private float m_Height;
    private Camera m_Camera;
    
    private void Awake()
    {
        m_Camera = Camera.main;
        m_FirstTile.GetComponent<SpriteRenderer>().sprite = m_ParallaxManager.GetLayerSprite(m_LayerIndex);
        m_SecondTile.GetComponent<SpriteRenderer>().sprite = m_ParallaxManager.GetLayerSprite(m_LayerIndex);
        m_Height = m_FirstTile.GetComponent<SpriteRenderer>().bounds.size.y;
        m_SecondTile.position = m_FirstTile.position + new Vector3(0, m_Height, 0);
    }

    private void Update()
    {
        float dy = -m_ScrollSpeed * Time.deltaTime;
        m_FirstTile.Translate(0, dy, 0, Space.World);
        m_SecondTile.Translate(0, dy, 0, Space.World);
        
        var camY = m_Camera.transform.position.y;
        if (m_FirstTile.position.y < camY - m_Height) m_FirstTile.position += new Vector3(0, 2 * m_Height, 0);
        if (m_SecondTile.position.y < camY - m_Height) m_SecondTile.position += new Vector3(0, 2 * m_Height, 0);
    }
}
