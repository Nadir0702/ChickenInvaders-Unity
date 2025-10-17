using System.Collections;
using UnityEngine;

public class PlayerRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    [SerializeField] private GameObject m_ExplosionPrefab; // Assign explosion animation prefab in inspector
    [SerializeField] private float m_RespawnDelay = 1f; // Time before respawn starts
    [SerializeField] private float m_RespawnSpeed = 5f; // Speed of respawn movement
    [SerializeField] private Vector3 m_RespawnPosition = new Vector3(0f, -3.5f, 0f); // Final respawn position
    
    private Camera m_Camera;
    private Vector3 m_OffScreenPosition; // Position below screen where respawn starts
    private bool m_IsRespawning = false;
    
    [Header("Component References")]
    [SerializeField] private PlayerController m_PlayerController;
    [SerializeField] private PlayerShooting m_PlayerShooting;
    [SerializeField] private PlayerBombing m_PlayerBombing;
    [SerializeField] private SpriteRenderer m_SpriteRenderer;
    [SerializeField] private Collider2D m_Collider;
    [SerializeField] private Rigidbody2D m_Rigidbody2D;
    [SerializeField] private GameObject m_Jet;

    private void Awake()
    {
        m_Camera = Camera.main;
        
        // Calculate off-screen position (below camera view)
        Vector3 bottomOfScreen = m_Camera.ViewportToWorldPoint(new Vector3(0.5f, -0.2f, 0f));
        m_OffScreenPosition = new Vector3(0f, bottomOfScreen.y, 0f);
    }

    /// <summary>
    /// Initialize player as hidden (called at game start before spawning)
    /// </summary>
    public void InitializePlayerAsHidden()
    {
        // Reset respawning state to ensure clean initialization
        m_IsRespawning = false;
        
        // Stop any ongoing coroutines to prevent conflicts
        StopAllCoroutines();
        
        disablePlayer();
    }

    /// <summary>
    /// Triggers the explosion and respawn sequence
    /// </summary>
    public void TriggerExplosionAndRespawn()
    {
        if (m_IsRespawning) return; // Prevent multiple calls
        
        StartCoroutine(explosionAndRespawnSequence());
    }

    /// <summary>
    /// Spawns the player at game start (no explosion, just respawn from bottom)
    /// </summary>
    public void SpawnPlayerAtGameStart()
    {
        if (m_IsRespawning) 
        {
            return; // Prevent multiple calls
        }
        
        StartCoroutine(spawnAtGameStart());
    }

    private IEnumerator spawnAtGameStart()
    {
        m_IsRespawning = true;
        
        // Start with player disabled and off-screen
        disablePlayer();
        
        // Wait a brief moment for game initialization
        yield return new WaitForSeconds(0.2f);
        
        // Spawn from bottom
        yield return StartCoroutine(respawnFromBottom());
        
        m_IsRespawning = false;
    }

    private IEnumerator explosionAndRespawnSequence()
    {
        m_IsRespawning = true;
        
        // Steps 5-6: Always play explosion and remove player (regardless of game state)
        yield return StartCoroutine(playExplosionAndDisablePlayer());
        
        // Wait for respawn delay
        yield return new WaitForSeconds(m_RespawnDelay);
        
        // Step 7: Only respawn if game is not over
        if (GameManager.Instance?.GameState != eGameState.GameOver)
        {
            yield return StartCoroutine(respawnFromBottom());
        }
        
        m_IsRespawning = false;
    }

    private IEnumerator playExplosionAndDisablePlayer()
    {
        // Step 5: Play explosion animation at player position
        Vector3 explosionPosition = transform.position;
        if (m_ExplosionPrefab != null)
        {
            GameObject explosion = Instantiate(m_ExplosionPrefab, explosionPosition, Quaternion.identity);
            // Destroy explosion after animation (adjust time based on your animation length)
            Destroy(explosion, 2f);
        }
        
        disablePlayer();
        
        yield return null; // Wait one frame to ensure everything is processed
    }

    private void disablePlayer()
    {
        // Hide player sprite
        if (m_SpriteRenderer) m_SpriteRenderer.enabled = false;
        
        disableAllColliders();
        
        // Reset movement input and velocity to prevent lingering movement
        if (m_PlayerController) 
        {
            m_PlayerController.ResetMovement();
            m_PlayerController.enabled = false;
        }
        
        // Additional safety: reset rigidbody velocity
        if (m_Rigidbody2D) m_Rigidbody2D.linearVelocity = Vector2.zero;
        if (m_PlayerShooting) m_PlayerShooting.enabled = false;
        if (m_PlayerBombing) m_PlayerBombing.enabled = false;
        if (m_Jet) m_Jet.SetActive(false); // Hide jet effect if applicable
        
        // Move player off-screen
        transform.position = m_OffScreenPosition;
    }

    private void enablePlayer()
    {
        // Show player sprite
        if (m_SpriteRenderer) m_SpriteRenderer.enabled = true;
        
        // Enable all colliders
        enableAllColliders();
        
        // Enable player controls
        if (m_PlayerController) m_PlayerController.enabled = true;
        if (m_PlayerShooting) m_PlayerShooting.enabled = true;
        if (m_PlayerBombing) m_PlayerBombing.enabled = true;
        if (m_Jet) m_Jet.SetActive(true); // Show jet effect if applicable
    }

    private IEnumerator respawnFromBottom()
    {
        // Start from off-screen position
        transform.position = m_OffScreenPosition;
        
        // Reset velocity to ensure clean movement
        if (m_Rigidbody2D) m_Rigidbody2D.linearVelocity = Vector2.zero;
        
        // Enable rendering but keep collision and controls disabled during movement
        if (m_SpriteRenderer) m_SpriteRenderer.enabled = true;
        
        // Ensure all colliders remain disabled during movement to prevent interference
        disableAllColliders();
        
        // Ensure controls remain disabled during movement
        if (m_PlayerController) m_PlayerController.enabled = false;
        if (m_PlayerShooting) m_PlayerShooting.enabled = false;
        if (m_PlayerBombing) m_PlayerBombing.enabled = false;
        
        // Move player to respawn position
        while (Vector3.Distance(transform.position, m_RespawnPosition) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, m_RespawnPosition, m_RespawnSpeed * Time.deltaTime);
            yield return null;
        }
        
        // Snap to exact position
        transform.position = m_RespawnPosition;
        
        // Enable all player functionality only after reaching final position
        enablePlayer();
    }

    /// <summary>
    /// Check if player is currently in respawn sequence
    /// </summary>
    public bool IsRespawning => m_IsRespawning;

    /// <summary>
    /// Disable all colliders on player and children (including shield)
    /// </summary>
    private void disableAllColliders()
    {
        // Disable main collider
        if (m_Collider) m_Collider.enabled = false;
        
        // Disable all child colliders (like shield colliders)
        var childColliders = GetComponentsInChildren<Collider2D>();
        foreach (var collider in childColliders)
        {
            collider.enabled = false;
        }
    }

    /// <summary>
    /// Enable all colliders on player and children (including shield)
    /// </summary>
    private void enableAllColliders()
    {
        // Enable main collider
        if (m_Collider) m_Collider.enabled = true;
        
        // Enable all child colliders (like shield colliders)
        var childColliders = GetComponentsInChildren<Collider2D>();
        foreach (var collider in childColliders)
        {
            collider.enabled = true;
        }
    }
}
