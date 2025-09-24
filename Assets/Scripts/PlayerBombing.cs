using UnityEngine;

public class PlayerBombing : MonoBehaviour
{
    [SerializeField] private PlayerStats m_PlayerStats;

    private void Update()
    {
        if (GameManager.Instance.GameState != eGameState.Playing) return;

        if(Input.GetKeyDown(KeyCode.LeftShift) || Input.GetMouseButtonDown(1))
        {
            tryBomb();
        }
        
    }

    private void tryBomb()
    {
        if(m_PlayerStats.BombCount <= 0) return;

        m_PlayerStats.AddBomb(-1);
        PoolManager.Instance?.GetBomb(transform.position);
    }
}
