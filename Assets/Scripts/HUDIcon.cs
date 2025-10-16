using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A poolable HUD icon component for lives and bombs display
/// </summary>
public class HUDIcon : MonoBehaviour, IPoolable
{
    [SerializeField] private Image m_Image;
    [SerializeField] private RectTransform m_RectTransform;
    
    public void OnPoolGet()
    {
        // Reset any state when taken from pool
        gameObject.SetActive(true);
    }
    
    public void OnPoolReturn()
    {
        // Clean up state when returned to pool
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// Set the sprite for this icon
    /// </summary>
    public void SetSprite(Sprite sprite)
    {
        if (m_Image)
        {
            m_Image.sprite = sprite;
        }
    }
    
    /// <summary>
    /// Set the size of this icon
    /// </summary>
    public void SetSize(Vector2 size)
    {
        if (m_RectTransform)
        {
            m_RectTransform.sizeDelta = size;
        }
    }
}
