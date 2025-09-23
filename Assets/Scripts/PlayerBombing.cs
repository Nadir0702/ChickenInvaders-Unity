using System;
using UnityEngine;

public class PlayerBombing : MonoBehaviour
{
    [SerializeField] private PlayerStats m_PlayerStats;
    [SerializeField] private int m_BombDamage = 999;

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
        if (m_PlayerStats.BombCount <= 0) return;
        
        m_PlayerStats.AddBomb(-1);
        foreach(var enemy in FindObjectsByType<EnemyController>(FindObjectsSortMode.None))
        {
            if(enemy) enemy.TakeDamage(m_BombDamage);
        }
        
        // TODO (when you add enemy bullets): clear all EnemyBullet layer instances.
        // foreach (var b in FindObjectsOfType<EnemyBullet>()) Destroy(b.gameObject);
        
        AudioManager.Instance?.Play(eSFXId.Explosion);
    }
}
