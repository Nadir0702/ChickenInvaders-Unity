using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class WaveDirector : MonoBehaviour
{
    [SerializeField] private EnemyController m_EnemyPrefab;
    [SerializeField] private Transform m_Player;
    [SerializeField] private float m_InterWaveDelay = 2.5f;
    
    private Camera m_Camera;
    private readonly List<EnemyController> r_ActiveEnemies = new();
    
    private void Awake()
    {
        m_Camera = Camera.main;
    }
    
    private void Start()
    {
        StartCoroutine(runWaves());
    }

    private IEnumerator runWaves()
    {
        int waveNumber = 1;
        while(GameManager.Instance && GameManager.Instance.GameState == eGameState.Playing)
        {
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
                mover.m_MoveType = eEnemyMoveType.StraightDown;
                mover.Speed = speed;
                r_ActiveEnemies.Add(enemy);
            }
        }
        
        yield return null;
    }
    
    // ---------- Pattern 2: Arc of enemies (Sine) ----------
    private IEnumerator Wave_Arc(int i_WaveNumber)
    {
        int rows = 2;
        int perRow = Mathf.Clamp(4 + i_WaveNumber / 2, 4, 10);
        float baseSpeed = Mathf.Clamp(2.2f + i_WaveNumber * 0.12f, 2.2f, 5.2f);
        
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
                    mover.m_MoveType = eEnemyMoveType.SineHorizontal;
                    mover.Speed = baseSpeed;
                    mover.SineAmplitude = 1f + r * 0.4f;
                    mover.SineFrequency = 2f + r * 0.3f;
                    r_ActiveEnemies.Add(enemy);
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
        float entrySpeed = Mathf.Clamp(2.8f + 0.1f * i_WaveNumber * 0.1f, 2.8f, 6.5f);
        
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
                    mover.m_MoveType = eEnemyMoveType.DiveAtPlayer;
                    mover.Speed = entrySpeed;
                    mover.DiveDelay = Random.Range(0.4f, 0.9f);
                    mover.DiveSpeed = 7f + Random.Range(-0.5f, 0.5f);
                    mover.InitDive(m_Player);
                    r_ActiveEnemies.Add(enemy);
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
        float columnSpacing = 1.6f;
        float rowSpacing = 0.8f;
        
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
                    mover.m_MoveType = eEnemyMoveType.StraightDown;
                    mover.Speed = 2.5f + 0.15f * i_WaveNumber;
                    r_ActiveEnemies.Add(enemy);
                }
            }
            
            yield return new WaitForSeconds(0.25f);
        }
    }
    
    // ---------- Pattern 5: Pincer ----------
    private IEnumerator Wave_Pincer(int i_WaveNumber)
    {
        int pairs = Mathf.Clamp(4 + i_WaveNumber / 2, 4, 10);
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
                leftMover.m_MoveType = eEnemyMoveType.SemicircleArc;
                leftMover.Speed = 2f + 0.2f * i_WaveNumber;
                leftMover.InitSemicircle(new Vector2(xLeft, 0f), radius, true, Mathf.PI); // Start at top-left of circle (PI radians)
                r_ActiveEnemies.Add(leftEnemy);
            }
            
            // Right enemy starts at top-right, moves in semicircle counter-clockwise to bottom-left
            var rightEnemy = PoolManager.Instance.GetEnemy(new Vector3(xRight, y + 0.2f, 0f), Quaternion.identity);
            if (rightEnemy != null)
            {
                var rightMover = rightEnemy.GetComponent<EnemyMover>();
                rightMover.m_MoveType = eEnemyMoveType.SemicircleArc;
                rightMover.Speed = 2f + 0.2f * i_WaveNumber;
                rightMover.InitSemicircle(new Vector2(xRight, 0f), radius, false, 0f); // Start at top-right of circle (0 radians)
                r_ActiveEnemies.Add(rightEnemy);
            }
            
            yield return new WaitForSeconds(0.18f);
        }
    }
}
