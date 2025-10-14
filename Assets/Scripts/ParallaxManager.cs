using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParallaxManager : Singleton<ParallaxManager>
{
    [SerializeField] private Sprite[] m_LayerSprites;
    
    // Acceleration settings
    [Header("LightSpeed Acceleration")]
    [SerializeField] private float m_AccelerationAmount = 1f; // Speed increase per layer
    [SerializeField] private float m_TransitionDuration = 1f; // Transition time in seconds
    
    // Layer management
    private List<ParallaxLayer> m_RegisteredLayers = new List<ParallaxLayer>();
    private bool m_IsAccelerating = false;
    
    public Sprite GetLayerSprite(int i_Index) => m_LayerSprites[(i_Index - 1) % m_LayerSprites.Length];
    
    /// <summary>
    /// Register a parallax layer for acceleration control
    /// </summary>
    public void RegisterLayer(ParallaxLayer i_Layer)
    {
        if (!m_RegisteredLayers.Contains(i_Layer))
        {
            m_RegisteredLayers.Add(i_Layer);
            Debug.Log($"Registered parallax layer: {i_Layer.name}");
        }
    }
    
    /// <summary>
    /// Unregister a parallax layer
    /// </summary>
    public void UnregisterLayer(ParallaxLayer i_Layer)
    {
        if (m_RegisteredLayers.Contains(i_Layer))
        {
            m_RegisteredLayers.Remove(i_Layer);
            Debug.Log($"Unregistered parallax layer: {i_Layer.name}");
        }
    }
    
    /// <summary>
    /// Accelerate all background layers for lightspeed effect
    /// </summary>
    public void AccelerateBackground()
    {
        if (m_IsAccelerating) return;
        
        m_IsAccelerating = true;
        Debug.Log("Starting background acceleration for lightspeed");
        
        foreach (var layer in m_RegisteredLayers)
        {
            if (layer != null)
            {
                float targetSpeed = layer.GetBaseSpeed() + m_AccelerationAmount;
                layer.TransitionToSpeed(targetSpeed, m_TransitionDuration);
            }
        }
    }
    
    /// <summary>
    /// Decelerate all background layers back to normal speed
    /// </summary>
    public void DecelerateBackground()
    {
        if (!m_IsAccelerating) return;
        
        m_IsAccelerating = false;
        Debug.Log("Starting background deceleration from lightspeed");
        
        foreach (var layer in m_RegisteredLayers)
        {
            if (layer != null)
            {
                layer.TransitionToSpeed(layer.GetBaseSpeed(), m_TransitionDuration);
            }
        }
    }
    
    /// <summary>
    /// Get current acceleration settings for debugging
    /// </summary>
    public bool IsAccelerating => m_IsAccelerating;
    public float AccelerationAmount => m_AccelerationAmount;
    public float TransitionDuration => m_TransitionDuration;
}
