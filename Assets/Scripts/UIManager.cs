using System;
using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private TextMeshProUGUI m_ScoreText;
    [SerializeField] private TextMeshProUGUI m_LivesText;
    
    private int m_Score = 0;
    private int m_Lives = 3;

    private void Awake()
    {
        updateHUD();
    }
    
    public void AddScore(int i_Amount)
    {
        m_Score += i_Amount;
        updateHUD();
    }
    
    public void SetLives(int i_Value)
    {
        m_Lives = i_Value;
        updateHUD();
    }       

    private void updateHUD()
    {
        if (m_ScoreText) m_ScoreText.text = $"Score: {m_Score}";
        if (m_LivesText) m_LivesText.text = $"Lives: {m_Lives}";
    }
}
