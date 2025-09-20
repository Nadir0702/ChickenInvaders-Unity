using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyMover : MonoBehaviour
{
    [Header("Shared")]
    public eEnemyMoveType m_MoveType = eEnemyMoveType.StraightDown;
    public float Speed { get; set; } = 2f;
    
    [Header("Sine")]
    public float SineAmplitude { get; set; } = 1.2f;
    public float SineFrequency { get; set; } = 2f;

    private float m_SinePhase;
    
    [Header("Dive")]
    [SerializeField] private float m_SpawnTime;
    public float DiveDelay { get; set; } = 0.6f;
    public float DiveSpeed { get; set; } = 7f;
    private Transform m_Target;
    private Vector2 m_DiveDirection;
    private bool m_Dived;
    
    private Camera m_Camera;
    
    private void Awake()
    {
        m_Camera = Camera.main;
    }

    private void OnEnable()
    {
        m_SpawnTime = Time.time;
        m_SinePhase = Random.value * Mathf.PI * 2f; // random phase
    }
    
    public void InitDive(Transform i_Target)
    {
        m_Target = i_Target;
        m_Dived = false;
    }

    private void Update()
    {
        switch(m_MoveType)
        {
            case(eEnemyMoveType.StraightDown):
                handleStraightDownWave();
                break;
            case(eEnemyMoveType.SineHorizontal):
                handleSineHorizontalWave();
                break;
            case(eEnemyMoveType.DiveAtPlayer):
                handleDiveAtPlayerWave();
                break;
            default:
                Debug.LogError("EnemyMover: Unknown move type " + m_MoveType);
                break;
        }

        handleOffScreen();
    }

    private void handleDiveAtPlayerWave()
    {
        if (!m_Dived && Time.time - m_SpawnTime >= DiveDelay && m_Target != null)
        {
            m_DiveDirection = (m_Target.position - transform.position).normalized;
            m_Dived = true;
        }
        
        transform.Translate(Time.deltaTime * 
                            (m_Dived ? DiveSpeed : Speed) *
                            (m_Dived ? m_DiveDirection : Vector2.down )
                            , Space.World);
    }

    private void handleSineHorizontalWave()
    {
        transform.Translate(Time.deltaTime * Speed * Vector2.down, Space.World);
        float dx = Mathf.Sin((Time.time + m_SinePhase) * SineFrequency) * SineAmplitude * Time.deltaTime;
        transform.Translate(new Vector2(dx, 0f), Space.World);
    }

    private void handleStraightDownWave()
    {
        transform.Translate(Time.deltaTime * Speed * Vector2.down, Space.World); 
    }

    private void handleOffScreen()
    {
        // Destroy if out of screen
        Vector3 viewPortPosition = m_Camera.WorldToViewportPoint(transform.position);
        if (viewPortPosition.y < -0.15f || viewPortPosition.x < -0.15f || viewPortPosition.x > 1.15f)
        {
            Destroy(gameObject);
        }
    }
}
