using UnityEngine;

public class PlayerController : Singleton<PlayerController>
{
    [SerializeField] private Rigidbody2D m_Rigidbody2D;
    [SerializeField] private float m_MoveSpeed = 8f;
    [SerializeField] private Vector2 m_Padding = new Vector2(0.5f, 0.5f);

    private Camera m_Camera;
    private Vector2 m_Input;

    private void Awake()
    {
        m_Camera = Camera.main;
    }

    private void Update()
    {
        // Old Input Manager
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        m_Input = new Vector2(x, y).normalized;
    }

    private void FixedUpdate()
    {
        Vector2 targetVel = m_Input * m_MoveSpeed;
        m_Rigidbody2D.linearVelocity = targetVel;

        // Soft-clamp using camera bounds
        Vector3 pos = transform.position;
        Vector3 min = m_Camera.ViewportToWorldPoint(new Vector3(0, 0, 0));
        Vector3 max = m_Camera.ViewportToWorldPoint(new Vector3(1, 1, 0));
        pos.x = Mathf.Clamp(pos.x, min.x + m_Padding.x, max.x - m_Padding.x);
        pos.y = Mathf.Clamp(pos.y, min.y + m_Padding.y, max.y - m_Padding.y);
        transform.position = pos;
    }
}