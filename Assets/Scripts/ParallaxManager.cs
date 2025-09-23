using UnityEngine;

public class ParallaxManager : MonoBehaviour
{
    [SerializeField] private Sprite[] m_LayerSprites;
    
    public Sprite GetLayerSprite(int i_Index) => m_LayerSprites[(i_Index - 1) % m_LayerSprites.Length];
}
