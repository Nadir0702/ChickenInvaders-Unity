using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveDirector : Singleton<WaveDirector>
{
    [SerializeField] private EnemyController m_EnemyPrefab;
    [SerializeField] private GameObject m_BossPrefab; // Boss prefab reference
    [SerializeField] private Transform m_Player;
    [SerializeField] private float m_InterWaveDelay = 2.5f;
    
    private Camera m_Camera;
    private readonly List<EnemyController> r_ActiveEnemies = new();
    private Coroutine m_WaveCoroutine; // Track the wave coroutine
    private BossController m_CurrentBoss; // Track current boss
    
    // Sine wave pattern progression tracking
    private static int s_SineWaveLines = 3; // Start with 3 lines, increases each time up to max 7
    
    // Formation pattern tracking
    private Vector3 m_FormationCenter;
    private Vector3 m_FormationVelocity;
    private bool m_FormationMovingHorizontal = false;
    private float m_FormationDirection = 1f; // 1 for right, -1 for left
    private readonly List<EnemyController> r_FormationEnemies = new();
    
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
    
    private new void OnDestroy()
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
            // Stop ALL coroutines when returning to menu or game over (restart scenario)
            StopAllCoroutines();
            m_WaveCoroutine = null;
        }
    }
    
    /// <summary>
    /// Reset the wave director - called when restarting the game
    /// </summary>
    public void ResetWaves()
    {
        // Stop ALL coroutines to ensure no leftover spawning continues
        StopAllCoroutines();
        m_WaveCoroutine = null;
        
        // Clear active enemies
        r_ActiveEnemies.Clear();
        
        // Destroy current boss if exists
        if (m_CurrentBoss != null)
        {
            m_CurrentBoss.DeactivateBoss();
            m_CurrentBoss = null;
        }
        
        // Reset sine wave pattern progression
        s_SineWaveLines = 3;
        
        // Reset formation pattern
        r_FormationEnemies.Clear();
        m_FormationMovingHorizontal = false;
        m_FormationDirection = 1f;
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
            
            if(waveNumber % 6 == 1) yield return StartCoroutine(Wave_Line(waveNumber));
            else if(waveNumber % 6 == 2) yield return StartCoroutine(Wave_Arc(waveNumber));
            else if(waveNumber % 6 == 3) yield return StartCoroutine(Wave_Dive(waveNumber));
            else if(waveNumber % 6 == 4) yield return StartCoroutine(Wave_Columns(waveNumber));
            else if(waveNumber % 6 == 5) yield return StartCoroutine(Wave_Pincer(waveNumber));
            else if(waveNumber % 6 == 0) yield return StartCoroutine(Wave_Boss(waveNumber));
            
            // Wait for complete wave clear
            yield return StartCoroutine(waitForWaveClear());
            
            // Check if this was a pincer wave (wave 5) - fade out theme music before boss
            if (waveNumber % 6 == 5)
            {
                AudioManager.Instance?.FadeMusic();
            }
            
            bool shouldGoToLightSpeed = waveNumber % 6 == 0; // After boss battles
            if(shouldGoToLightSpeed)
            {
                m_InterWaveDelay = 12;
                PlayerController.Instance?.GoToLightSpeed();
            }
            else
            {
                m_InterWaveDelay = 3;
                
                // For regular waves, show next wave message after 0.5 seconds
                yield return new WaitForSeconds(0.5f);
                int nextWaveNumber = waveNumber + 1;
                InterWaveMessageManager.Instance?.ShowWaveMessage(nextWaveNumber);
                
                // Wait remaining time
                yield return new WaitForSeconds(m_InterWaveDelay - 0.5f);
            }
            
            if(shouldGoToLightSpeed)
            {
                // For light speed, just wait the full delay (no message during light speed)
                yield return new WaitForSeconds(m_InterWaveDelay);
            }
            if(shouldGoToLightSpeed)
            {
                PlayerController.Instance?.ExitLightSpeed();
                // After boss battle and light speed, start new wave cycle with theme music
                AudioManager.Instance?.OnNewWaveCycleStart();
            }
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
        
        // Wait for regular enemies to be cleared
        while(r_ActiveEnemies.Count > 0)
        {
            // Continuously clean up enemies that have been returned to pool
            r_ActiveEnemies.RemoveAll(i_Enemy => !i_Enemy.gameObject.activeInHierarchy);
            yield return null;
        }
        
        // If there's a boss, wait for it to be defeated
        while (m_CurrentBoss != null && m_CurrentBoss.IsActive)
        {
            yield return null;
        }
    } 
    
    // ---------- Pattern 1: Classic Formation ----------
    private IEnumerator Wave_Line(int i_WaveNumber)
    {
        const int columns = 8;
        const int rows = 5;
        int totalEnemies = columns * rows;
        const float formationSpeed = 1.5f;

        yield return new WaitForSeconds(2f);
        
        // Check if we have enough enemies in pool
        if (!PoolManager.Instance.HasEnoughEnemies(totalEnemies))
        {
            Debug.LogWarning($"Wave_Formation: Not enough enemies in pool for {totalEnemies} enemies. Skipping formation.");
            yield return null;
            yield break;
        }
        
        // Clear previous formation
        r_FormationEnemies.Clear();
        m_FormationMovingHorizontal = false;
        m_FormationDirection = 1f;
        
        AudioManager.Instance?.Play(eSFXId.NewRound, 0.8f, 1.5f);
        
        // Calculate formation layout - stop at upper portion of screen
        Vector3 stopPosition = m_Camera.ViewportToWorldPoint(new Vector3(0.5f, 0.7f, 0));
        stopPosition.z = 0f; // Ensure Z is 0
        Vector3 topCenter = m_Camera.ViewportToWorldPoint(new Vector3(0.5f, 1.2f, 0)); // Start above screen
        topCenter.z = 0f; // Ensure Z is 0
        
        float columnSpacing = 1.2f;
        float rowSpacing = 1f;
        
        // Initialize formation center
        m_FormationCenter = topCenter;
        m_FormationVelocity = Vector3.down * formationSpeed;
        
        // Spawn formation
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                // Calculate offset from formation center
                float xOffset = (col - (columns - 1) * 0.5f) * columnSpacing;
                float yOffset = (row - (rows - 1) * 0.5f) * rowSpacing;
                Vector3 offset = new Vector3(xOffset, yOffset, 0f);
                
                Vector3 spawnPosition = m_FormationCenter + offset;
                var enemy = PoolManager.Instance.GetEnemy(spawnPosition, Quaternion.identity);
                
                if (enemy != null)
                {
                    var mover = enemy.GetComponent<EnemyMover>();
                    if (mover != null)
                    {
                        mover.m_MoveType = eEnemyMoveType.Formation;
                        mover.Speed = formationSpeed;
                        mover.InitFormation(offset);
                        r_ActiveEnemies.Add(enemy);
                        r_FormationEnemies.Add(enemy);
                    }
                    else
                    {
                        Debug.LogError("WaveDirector: Enemy missing EnemyMover component!");
                        PoolManager.Instance.ReturnEnemy(enemy);
                    }
                }
                else
                {
                    Debug.LogWarning($"Wave_Formation: Failed to get enemy from pool for row {row}, col {col}");
                }
            }
        }
        
        // Update formation center for all enemies
        EnemyMover.SetFormationCenter(m_FormationCenter);
        
        // Start formation movement coroutine
        StartCoroutine(UpdateFormationMovement(stopPosition.y));
        
        yield return null;
    }
    
    private IEnumerator UpdateFormationMovement(float midScreenY)
    {
        const float horizontalSpeed = 1.0f;
        const int columns = 8;
        const float columnSpacing = 1.2f;
        float formationHalfWidth = (columns - 1) * columnSpacing * 0.5f;
        
        Vector3 leftEdge = m_Camera.ViewportToWorldPoint(new Vector3(0.1f, 0.5f, 0));
        leftEdge.z = 0f; // Ensure Z is 0
        Vector3 rightEdge = m_Camera.ViewportToWorldPoint(new Vector3(0.9f, 0.5f, 0));
        rightEdge.z = 0f; // Ensure Z is 0
        
        while (r_FormationEnemies.Count > 0)
        {
            // Clean up destroyed enemies
            r_FormationEnemies.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
            
            if (r_FormationEnemies.Count == 0) break;
            
            // Move formation downward until it reaches middle of screen
            if (!m_FormationMovingHorizontal && m_FormationCenter.y > midScreenY)
            {
                m_FormationCenter += m_FormationVelocity * Time.deltaTime;
            }
            else if (!m_FormationMovingHorizontal)
            {
                // Switch to horizontal movement
                m_FormationMovingHorizontal = true;
                m_FormationVelocity = Vector3.right * horizontalSpeed * m_FormationDirection;
            }
            
            // Handle horizontal movement and edge detection
            if (m_FormationMovingHorizontal)
            {
                m_FormationCenter += m_FormationVelocity * Time.deltaTime;
                
                // Check if formation reached screen edges and change direction
                // Account for formation width so outermost columns don't exit screen
                if (m_FormationDirection > 0 && m_FormationCenter.x + formationHalfWidth > rightEdge.x)
                {
                    m_FormationDirection = -1f;
                    m_FormationVelocity = Vector3.right * horizontalSpeed * m_FormationDirection;
                }
                else if (m_FormationDirection < 0 && m_FormationCenter.x - formationHalfWidth < leftEdge.x)
                {
                    m_FormationDirection = 1f;
                    m_FormationVelocity = Vector3.right * horizontalSpeed * m_FormationDirection;
                }
            }
            
            // Update formation center for all enemies
            EnemyMover.SetFormationCenter(m_FormationCenter);
            
            yield return null;
        }
    }
    
    // ---------- Pattern 2: Arc of enemies (Sine) ----------
    private IEnumerator Wave_Arc(int i_WaveNumber)
    {
        // Fixed configuration: 7 chickens per line, constant speed, progressive line count
        const int perRow = 7;
        int rows = s_SineWaveLines; // Use current line count (3-7)
        int totalEnemies = rows * perRow;
        const float baseSpeed = 2.2f; // Constant speed as requested
        const float lineGap = 1.6f; // Doubled gap between lines (was 0.8f)
        
        // Check if we have enough enemies in pool
        if (!PoolManager.Instance.HasEnoughEnemies(totalEnemies))
        {
            Debug.LogWarning($"Wave_Arc: Not enough enemies in pool for {totalEnemies} enemies. Reducing formation.");
            int availableEnemies = PoolManager.Instance.GetAvailableEnemyCount();
            // Reduce number of rows if not enough enemies
            rows = Mathf.Max(1, availableEnemies / perRow);
        }
        
        Vector3 topMin = m_Camera.ViewportToWorldPoint(new Vector3(0.15f, 1f, 0));
        Vector3 topMax = m_Camera.ViewportToWorldPoint(new Vector3(0.85f, 1f, 0));
        
        for(int r = 0; r < rows; r++)
        {
            for(int i = 0; i < perRow; i++)
            {
                float t = perRow == 1 ? 0.5f : i / (float)(perRow - 1);
                float x = Mathf.Lerp(topMin.x, topMax.x, t);
                float y = topMax.y + 0.5f + r * lineGap; // Use doubled gap
                
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
        
        // Increase line count for next time this pattern appears (max 7 lines)
        if (s_SineWaveLines < 7)
        {
            s_SineWaveLines++;
        }
    }
    
    // ---------- Pattern 3: Diving enemies ----------
    private IEnumerator Wave_Dive(int i_WaveNumber)
    {
        int waves = Mathf.Clamp(3 + i_WaveNumber / 5, 3, 6);
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
            
            yield return new WaitForSeconds(1f);
        }
    }
    
    // ---------- Pattern 4: Columns ----------
    private IEnumerator Wave_Columns(int i_WaveNumber)
    {
        int columns = Mathf.Clamp(3 + i_WaveNumber / 5, 3, 6);
        int rosPerColumn = Mathf.Clamp(5 + i_WaveNumber / 5, 3, 8);
        int totalEnemies = columns * rosPerColumn;
        float rowSpacing = 0.8f;
        
        // Reduced base speed and smaller increments for better gameplay
        float baseSpeed = 1.5f + 0.08f * i_WaveNumber; // Reduced from 1.5f + 0.15f
        
        // Dynamic column spacing to cover full viewport width
        Vector3 leftEdge = m_Camera.ViewportToWorldPoint(new Vector3(0.1f, 1f, 0));
        Vector3 rightEdge = m_Camera.ViewportToWorldPoint(new Vector3(0.9f, 1f, 0));
        float availableWidth = rightEdge.x - leftEdge.x;
        float columnSpacing = columns > 1 ? availableWidth / (columns - 1) : 0f;
        
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
                // Recalculate spacing with new column count
                columnSpacing = columns > 1 ? availableWidth / (columns - 1) : 0f;
            }
        }
        
        Vector3 center = m_Camera.ViewportToWorldPoint(new Vector3(0.5f, 1f, 0));
        
        for (int col = 0; col < columns; col++)
        {
            for (int row = 0; row < rosPerColumn; row++)
            {
                // Calculate X position to spread across full viewport width
                float x = columns == 1 ? center.x : leftEdge.x + col * columnSpacing;
                Vector3 position = new Vector3(x, center.y + 0.5f + row * rowSpacing, 0f);
                
                var enemy = PoolManager.Instance.GetEnemy(position, Quaternion.identity);
                if (enemy != null)
                {
                    var mover = enemy.GetComponent<EnemyMover>();
                    if (mover != null)
                    {
                        mover.m_MoveType = eEnemyMoveType.StraightDown;
                        mover.Speed = baseSpeed;
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
        int pairs = Mathf.Clamp(8 + i_WaveNumber / 5 * 2, 10, 25);
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
            
            yield return new WaitForSeconds(0.25f);
        }
    }
    
    // ---------- Pattern 6: Boss Battle ----------
    private IEnumerator Wave_Boss(int i_WaveNumber)
    {
        // Check if boss prefab is assigned
        if (m_BossPrefab == null)
        {
            Debug.LogError("WaveDirector: Boss prefab not assigned! Skipping boss wave.");
            yield break;
        }
        
        // Ensure no previous boss exists
        if (m_CurrentBoss != null)
        {
            m_CurrentBoss.DeactivateBoss();
            m_CurrentBoss = null;
        }
        
        // Start boss music
        AudioManager.Instance?.OnBossWaveStart();
        
        // Spawn boss above screen center
        Vector3 bossSpawnPosition = m_Camera.ViewportToWorldPoint(new Vector3(0.5f, 1.2f, 0));
        bossSpawnPosition.z = 0f;
        
        GameObject bossObject = Instantiate(m_BossPrefab, bossSpawnPosition, Quaternion.identity);
        m_CurrentBoss = bossObject.GetComponent<BossController>();
        
        if (m_CurrentBoss != null)
        {
            m_CurrentBoss.InitializeBoss();
        }
        else
        {
            Debug.LogError("WaveDirector: Boss prefab missing BossController component!");
            Destroy(bossObject);
        }
        
        yield return null;
    }
    
    /// <summary>
    /// Called by BossController when boss is killed
    /// </summary>
    public void OnBossKilled()
    {
        m_CurrentBoss = null;
    }
    
}
