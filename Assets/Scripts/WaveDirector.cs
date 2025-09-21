using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
            if(waveNumber % 3 == 1) yield return StartCoroutine(Wave_Line(waveNumber));
            else if(waveNumber % 3 == 2) yield return StartCoroutine(Wave_Arc(waveNumber));
            else yield return StartCoroutine(Wave_Dive(waveNumber));
            
            // Wait for clear or timeout (safety)
            yield return StartCoroutine(waitForWaveClear(8f));
            
            // Banner/UI hook later (e.g., "Wave X cleared!")
            yield return new WaitForSeconds(m_InterWaveDelay);
            waveNumber++;
        }
        
        yield return null;
    }
    
    private IEnumerator waitForWaveClear(float i_Timeout)
    {
        float waitTime = 0f;
        r_ActiveEnemies.RemoveAll(i_Enemy => i_Enemy == null); // Clean up null references
        while(r_ActiveEnemies.Count > 0 && waitTime < i_Timeout)
        {
            r_ActiveEnemies.RemoveAll(i_Enemy => i_Enemy == null); // Clean up null references
            waitTime += Time.deltaTime;
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
            var enemy = Instantiate(m_EnemyPrefab, new Vector3(x, topMax.y + 0.5f, 0), Quaternion.identity);
            var mover = enemy.GetComponent<EnemyMover>();
            mover.m_MoveType = eEnemyMoveType.StraightDown;
            mover.Speed = speed;
            r_ActiveEnemies.Add(enemy);
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
                
                var enemy = Instantiate(m_EnemyPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                var mover = enemy.GetComponent<EnemyMover>();
                
                mover.m_MoveType = eEnemyMoveType.SineHorizontal;
                mover.Speed = baseSpeed;
                mover.SineAmplitude = 1f + r * 0.4f;
                mover.SineFrequency = 2f + r * 0.3f;
                r_ActiveEnemies.Add(enemy);
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
                var enemy = Instantiate(m_EnemyPrefab, new Vector3(x, right.y + 0.7f, 0f), Quaternion.identity);
                var mover = enemy.GetComponent<EnemyMover>();
                mover.m_MoveType = eEnemyMoveType.DiveAtPlayer;
                mover.Speed = entrySpeed;
                mover.DiveDelay = Random.Range(0.4f, 0.9f);
                mover.DiveSpeed = 7f + Random.Range(-0.5f, 0.5f);
                mover.InitDive(m_Player);
                r_ActiveEnemies.Add(enemy);
            }
            
            yield return new WaitForSeconds(0.8f);
        }
    }
}
