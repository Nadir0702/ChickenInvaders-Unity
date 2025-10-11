using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveDirector : Singleton<WaveDirector>
{
    [SerializeField] private EnemyController m_EnemyPrefab;
    [SerializeField] private Transform m_Player;
    [SerializeField] private float m_InterWaveDelay = 2.5f;
    
    private Camera m_Camera;
    private readonly List<EnemyController> r_ActiveEnemies = new();
    private Coroutine m_WaveCoroutine; // Track the wave coroutine
    
    protected override void Awake()
    {
        base.Awake(); // Call singleton Awake first
        if (this == Instance) // Only initialize if we're the active instance
        {
            m_Camera = Camera.main;
        }
    }
    
    private void Start()
    {
        // Ensure we don't double-subscribe if this Start() is called multiple times
        GameManager.OnGameStateChanged -= OnGameStateChanged; // Unsubscribe first (safe even if not subscribed)
        GameManager.OnGameStateChanged += OnGameStateChanged; // Subscribe
    }
    
    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
    
    private void OnGameStateChanged(eGameState i_NewState)
    {
        if (i_NewState == eGameState.Playing)
        {
            // Only start waves if not already running (first time or after returning from menu/game over)
            if (m_WaveCoroutine == null)
            {
                // Clear any active enemies when starting fresh
                r_ActiveEnemies.Clear();
                
                // Start wave system when game begins
                m_WaveCoroutine = StartCoroutine(runWaves());
            }
        }
        else if (i_NewState == eGameState.Menu || i_NewState == eGameState.GameOver)
        {
            // Stop waves when returning to menu or game over (restart scenario)
            if (m_WaveCoroutine != null)
            {
                StopCoroutine(m_WaveCoroutine);
                m_WaveCoroutine = null;
            }
        }
        // Note: We don't stop the coroutine when pausing - let it continue but wait in the loop
    }
    
    /// <summary>
    /// Reset the wave director - called when restarting the game
    /// </summary>
    public void ResetWaves()
    {
        // Stop current wave coroutine if running
        if (m_WaveCoroutine != null)
        {
            StopCoroutine(m_WaveCoroutine);
            m_WaveCoroutine = null;
        }
        
        // Clear active enemies
        r_ActiveEnemies.Clear();
        
        // The next time Playing state is entered, it will start fresh from wave 1
    }

    private IEnumerator runWaves()
    {
        int waveNumber = 1;
        while(GameManager.Instance)
        {
            // Wait if game is not in Playing state (paused, menu, game over)
            while (GameManager.Instance && GameManager.Instance.GameState != eGameState.Playing)
            {
                yield return new WaitForSecondsRealtime(0.1f); // Wait 0.1 seconds using real time (unaffected by Time.timeScale)
            }
            
            // Exit if GameManager is destroyed
            if (!GameManager.Instance) break;
            
            // Update difficulty tier for enemy scaling
            GameManager.Instance.SetDifficultyTier(waveNumber);
            
            // Notify pickup manager of new wave
            PickupManager.Instance?.OnWaveStart();
            
            if(waveNumber % 5 == 1) yield return StartCoroutine(Wave_Line(waveNumber));
            else if(waveNumber % 5 == 2) yield return StartCoroutine(Wave_Arc(waveNumber));
            else if(waveNumber % 5 == 3) yield return StartCoroutine(Wave_Dive(waveNumber));
            else if(waveNumber % 5 == 4) yield return StartCoroutine(Wave_Columns(waveNumber));
            else if(waveNumber % 5 == 0) yield return StartCoroutine(Wave_Pincer(waveNumber));
            
            // Wait for complete wave clear
            yield return StartCoroutine(waitForWaveClear());
            
            // Banner/UI hook later (e.g., "Wave X cleared!")
            yield return new WaitForSeconds(m_InterWaveDelay);
            waveNumber++;
        }
        
        // Clean up coroutine reference when done
        m_WaveCoroutine = null;
        yield return null;
    }
    
    private IEnumerator waitForWaveClear()
    {
        // Clean up enemies that are inactive (returned to pool)
        r_ActiveEnemies.RemoveAll(i_Enemy => !i_Enemy.gameObject.activeInHierarchy);
        
        while(r_ActiveEnemies.Count > 0)
        {
            // Continuously clean up enemies that have been returned to pool
            r_ActiveEnemies.RemoveAll(i_Enemy => !i_Enemy.gameObject.activeInHierarchy);
            yield return null;
        }
    } 
    
    // ---------- Pattern 1: Line of enemies ----------
    private IEnumerator Wave_Line(int i_WaveNumber)
    {
        int count = Mathf.Clamp(6 + i_WaveNumber, 6, 14);
        float speed = Mathf.Clamp(2.5f + i_WaveNumber * 0.15f, 2.5f, 6f);
        
        // Check if we have enough enemies in pool
        if (!PoolManager.Instance.HasEnoughEnemies(count))
        {
            Debug.LogWarning($"Wave_Line: Not enough enemies in pool for {count} enemies. Reducing count.");
            count = Mathf.Min(count, PoolManager.Instance.GetAvailableEnemyCount());
        }
        
        Vector3 topMin = m_Camera.ViewportToWorldPoint(new Vector3(0.1f, 1f, 0));
        Vector3 topMax = m_Camera.ViewportToWorldPoint(new Vector3(0.9f, 1f, 0));

        for(int i = 0; i < count; i++)
        {
            float t = count == 1 ? 0.5f : i / (float)(count - 1);
            float x = Mathf.Lerp(topMin.x, topMax.x, t);
            var enemy = PoolManager.Instance.GetEnemy(new Vector3(x, topMax.y + 0.5f, 0), Quaternion.identity);
            if (enemy != null)
            {
                var mover = enemy.GetComponent<EnemyMover>();
                if (mover != null)
                {
                    mover.m_MoveType = eEnemyMoveType.StraightDown;
                    mover.Speed = speed;
                    r_ActiveEnemies.Add(enemy);
                }
                else
                {
                    Debug.LogError("WaveDirector: Enemy missing EnemyMover component!");
                    // Return enemy back to pool if it's missing components
                    PoolManager.Instance.ReturnEnemy(enemy);
                }
            }
            else
            {
                Debug.LogWarning($"WaveDirector: Failed to get enemy from pool for Wave_Line, enemy {i + 1}/{count}");
            }
        }
        
        yield return null;
    }
    
    // ---------- Pattern 2: Arc of enemies (Sine) ----------
    private IEnumerator Wave_Arc(int i_WaveNumber)
    {
        int rows = 2;
        int perRow = Mathf.Clamp(4 + i_WaveNumber / 2, 4, 10);
        int totalEnemies = rows * perRow;
        float baseSpeed = Mathf.Clamp(2.2f + i_WaveNumber * 0.12f, 2.2f, 5.2f);
        
        // Check if we have enough enemies in pool
        if (!PoolManager.Instance.HasEnoughEnemies(totalEnemies))
        {
            Debug.LogWarning($"Wave_Arc: Not enough enemies in pool for {totalEnemies} enemies. Reducing formation.");
            int availableEnemies = PoolManager.Instance.GetAvailableEnemyCount();
            // Reduce formation size proportionally
            perRow = Mathf.Max(1, availableEnemies / rows);
        }
        
        Vector3 topMin = m_Camera.ViewportToWorldPoint(new Vector3(0.15f, 1f, 0));
        Vector3 topMax = m_Camera.ViewportToWorldPoint(new Vector3(0.85f, 1f, 0));
        
        for(int r = 0; r < rows; r++)
        {
            for(int i = 0; i < perRow; i++)
            {
                float t = perRow == 1 ? 0.5f : i / (float)(perRow - 1);
                float x = Mathf.Lerp(topMin.x, topMax.x, t);
                float y = topMax.y + 0.5f + r * 0.8f;
                
                var enemy = PoolManager.Instance.GetEnemy(new Vector3(x, y, 0f), Quaternion.identity);
                if (enemy != null)
                {
                    var mover = enemy.GetComponent<EnemyMover>();
                    if (mover != null)
                    {
                        mover.m_MoveType = eEnemyMoveType.SineHorizontal;
                        mover.Speed = baseSpeed;
                        mover.SineAmplitude = 1f + r * 0.4f;
                        mover.SineFrequency = 2f + r * 0.3f;
                        r_ActiveEnemies.Add(enemy);
                    }
                    else
                    {
                        Debug.LogError("WaveDirector: Enemy missing EnemyMover component!");
                        PoolManager.Instance.ReturnEnemy(enemy);
                    }
                }
                else
                {
                    Debug.LogWarning($"Wave_Arc: Failed to get enemy from pool for row {r}, enemy {i + 1}/{perRow}");
                }
            }
            
            yield return new WaitForSeconds(0.4f);
        }
    }
    
    // ---------- Pattern 3: Diving enemies ----------
    private IEnumerator Wave_Dive(int i_WaveNumber)
    {
        int waves = 3;
        int perBurst = Mathf.Clamp(3 + i_WaveNumber / 3, 3, 8);
        int totalEnemies = waves * perBurst;
        float entrySpeed = Mathf.Clamp(2.8f + 0.1f * i_WaveNumber * 0.1f, 2.8f, 6.5f);
        
        // Check if we have enough enemies in pool
        if (!PoolManager.Instance.HasEnoughEnemies(totalEnemies))
        {
            Debug.LogWarning($"Wave_Dive: Not enough enemies in pool for {totalEnemies} enemies. Reducing formation.");
            int availableEnemies = PoolManager.Instance.GetAvailableEnemyCount();
            perBurst = Mathf.Max(1, availableEnemies / waves);
        }
        
        Vector3 left = m_Camera.ViewportToWorldPoint(new Vector3(0.1f, 1f, 0));
        Vector3 right = m_Camera.ViewportToWorldPoint(new Vector3(0.9f, 1f, 0));
        
        for (int b = 0; b < waves; b++)
        {
            for(int i = 0; i < perBurst; i++)
            {
                float x = Random.Range(left.x, right.x);
                var enemy = PoolManager.Instance.GetEnemy(new Vector3(x, right.y + 0.7f, 0f), Quaternion.identity);
                if (enemy != null)
                {
                    var mover = enemy.GetComponent<EnemyMover>();
                    if (mover != null)
                    {
                        mover.m_MoveType = eEnemyMoveType.DiveAtPlayer;
                        mover.Speed = entrySpeed;
                        mover.DiveDelay = Random.Range(0.4f, 0.9f);
                        mover.DiveSpeed = 7f + Random.Range(-0.5f, 0.5f);
                        mover.InitDive(m_Player);
                        r_ActiveEnemies.Add(enemy);
                    }
                    else
                    {
                        Debug.LogError("WaveDirector: Enemy missing EnemyMover component!");
                        PoolManager.Instance.ReturnEnemy(enemy);
                    }
                }
                else
                {
                    Debug.LogWarning($"Wave_Dive: Failed to get enemy from pool for wave {b}, enemy {i + 1}/{perBurst}");
                }
            }
            
            yield return new WaitForSeconds(0.8f);
        }
    }
    
    // ---------- Pattern 4: Columns ----------
    private IEnumerator Wave_Columns(int i_WaveNumber)
    {
        int columns = Mathf.Clamp(3 + i_WaveNumber / 3, 3, 6);
        int rosPerColumn = Mathf.Clamp(3 + i_WaveNumber / 2, 3, 8);
        int totalEnemies = columns * rosPerColumn;
        float columnSpacing = 1.6f;
        float rowSpacing = 0.8f;
        
        // Check if we have enough enemies in pool
        if (!PoolManager.Instance.HasEnoughEnemies(totalEnemies))
        {
            Debug.LogWarning($"Wave_Columns: Not enough enemies in pool for {totalEnemies} enemies. Reducing formation.");
            int availableEnemies = PoolManager.Instance.GetAvailableEnemyCount();
            // Reduce columns first, then rows
            if (availableEnemies < totalEnemies)
            {
                columns = Mathf.Max(1, availableEnemies / rosPerColumn);
                if (columns == 0) {
                    columns = 1;
                    rosPerColumn = availableEnemies;
                }
            }
        }
        
        Vector3 center = m_Camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0));
        float startX = center.x - (columns - 1) * columnSpacing * 0.5f;
        
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rosPerColumn; row++)
            {
                Vector3 position = new Vector3(startX + col * columnSpacing, center.y + 0.5f + row * rowSpacing, 0f);
                var enemy = PoolManager.Instance.GetEnemy(position, Quaternion.identity);
                if (enemy != null)
                {
                    var mover = enemy.GetComponent<EnemyMover>();
                    if (mover != null)
                    {
                        mover.m_MoveType = eEnemyMoveType.StraightDown;
                        mover.Speed = 2.5f + 0.15f * i_WaveNumber;
                        r_ActiveEnemies.Add(enemy);
                    }
                    else
                    {
                        Debug.LogError("WaveDirector: Enemy missing EnemyMover component!");
                        PoolManager.Instance.ReturnEnemy(enemy);
                    }
                }
                else
                {
                    Debug.LogWarning($"Wave_Columns: Failed to get enemy from pool for column {col}, row {row}");
                }
            }
            
            yield return new WaitForSeconds(0.25f);
        }
    }
    
    // ---------- Pattern 5: Pincer ----------
    private IEnumerator Wave_Pincer(int i_WaveNumber)
    {
        int pairs = Mathf.Clamp(4 + i_WaveNumber / 2, 4, 10);
        int totalEnemies = pairs * 2; // 2 enemies per pair
        
        // Check if we have enough enemies in pool
        if (!PoolManager.Instance.HasEnoughEnemies(totalEnemies))
        {
            Debug.LogWarning($"Wave_Pincer: Not enough enemies in pool for {totalEnemies} enemies. Reducing pairs.");
            int availableEnemies = PoolManager.Instance.GetAvailableEnemyCount();
            pairs = Mathf.Max(1, availableEnemies / 2);
        }
        
        float yTop = m_Camera.ViewportToWorldPoint(new Vector3(0, 1f, 0)).y;
        float yBottom = m_Camera.ViewportToWorldPoint(new Vector3(0, 0f, 0)).y;
        float xLeft = m_Camera.ViewportToWorldPoint(new Vector3(0f, 0f, 0)).x;
        float xRight = m_Camera.ViewportToWorldPoint(new Vector3(1f, 0f, 0)).x;
        
        // Calculate radius for semicircle (half the screen width)
        float screenHeightWorldUnits = yTop - yBottom;
        float radius = screenHeightWorldUnits * 0.5f;
        
        for(int p = 0; p < pairs; p++)
        {
            float y = yTop + 0.5f + p * 0.5f;
            
            // Left enemy starts at top-left, moves in semicircle clockwise to bottom-right
            var leftEnemy = PoolManager.Instance.GetEnemy(new Vector3(xLeft, y, 0f), Quaternion.identity);
            if (leftEnemy != null)
            {
                var leftMover = leftEnemy.GetComponent<EnemyMover>();
                if (leftMover != null)
                {
                    leftMover.m_MoveType = eEnemyMoveType.SemicircleArc;
                    leftMover.Speed = 2f + 0.2f * i_WaveNumber;
                    leftMover.InitSemicircle(new Vector2(xLeft, 0f), radius, true, Mathf.PI); // Start at top-left of circle (PI radians)
                    r_ActiveEnemies.Add(leftEnemy);
                }
                else
                {
                    Debug.LogError("WaveDirector: Left enemy missing EnemyMover component!");
                    PoolManager.Instance.ReturnEnemy(leftEnemy);
                }
            }
            else
            {
                Debug.LogWarning($"Wave_Pincer: Failed to get left enemy from pool for pair {p + 1}/{pairs}");
            }
            
            // Right enemy starts at top-right, moves in semicircle counter-clockwise to bottom-left
            var rightEnemy = PoolManager.Instance.GetEnemy(new Vector3(xRight, y + 0.2f, 0f), Quaternion.identity);
            if (rightEnemy != null)
            {
                var rightMover = rightEnemy.GetComponent<EnemyMover>();
                if (rightMover != null)
                {
                    rightMover.m_MoveType = eEnemyMoveType.SemicircleArc;
                    rightMover.Speed = 2f + 0.2f * i_WaveNumber;
                    rightMover.InitSemicircle(new Vector2(xRight, 0f), radius, false, 0f); // Start at top-right of circle (0 radians)
                    r_ActiveEnemies.Add(rightEnemy);
                }
                else
                {
                    Debug.LogError("WaveDirector: Right enemy missing EnemyMover component!");
                    PoolManager.Instance.ReturnEnemy(rightEnemy);
                }
            }
            else
            {
                Debug.LogWarning($"Wave_Pincer: Failed to get right enemy from pool for pair {p + 1}/{pairs}");
            }
            
            yield return new WaitForSeconds(0.18f);
        }
    }
}
