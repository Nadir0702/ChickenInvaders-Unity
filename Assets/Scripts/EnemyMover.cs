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

    [Header("Semicircle")]
    private Vector2 m_SemicircleCenter;
    private float m_SemicircleRadius = 3f;
    private bool m_SemicircleClockwise = true;
    private float m_SemicircleCurrentAngle;
    private float m_SemicircleAngularSpeed;
    
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
    
    public void InitSemicircle(Vector2 i_Center, float i_Radius, bool i_Clockwise, float i_StartAngle)
    {
        m_SemicircleCenter = i_Center;
        m_SemicircleRadius = i_Radius;
        m_SemicircleClockwise = i_Clockwise;
        m_SemicircleCurrentAngle = i_StartAngle;
        m_SemicircleAngularSpeed = Speed / i_Radius; // Convert linear speed to angular speed
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
            case(eEnemyMoveType.SemicircleArc):
                handleSemicircleArcWave();
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

    private void handleSemicircleArcWave()
    {
        // Update the current angle based on angular speed
        float angleDirection = m_SemicircleClockwise ? -1f : 1f;
        m_SemicircleCurrentAngle += m_SemicircleAngularSpeed * angleDirection * Time.deltaTime;
        
        // Calculate the new position based on the current angle
        float x = m_SemicircleCenter.x + m_SemicircleRadius * Mathf.Cos(m_SemicircleCurrentAngle);
        float y = m_SemicircleCenter.y + m_SemicircleRadius * Mathf.Sin(m_SemicircleCurrentAngle);
        
        transform.position = new Vector3(x, y, transform.position.z);
    }

    private void handleOffScreen()
    {
        // Destroy if out of screen
        Vector3 viewPortPosition = m_Camera.WorldToViewportPoint(transform.position);
        if (viewPortPosition.y < -0.15f || viewPortPosition.x < -1f || viewPortPosition.x > 2f)
        {
            Destroy(gameObject);
        }
    }
}
