using TMPro;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] private TextMeshProUGUI m_ScoreText;
    [SerializeField] private TextMeshProUGUI m_LivesText;
    [SerializeField] private TextMeshProUGUI m_WeaponLevelText;
    [SerializeField] private TextMeshProUGUI m_BombsAmountText;
    [SerializeField] private GameObject m_GameOverPanel;
    [SerializeField] private GameObject m_PausePanel;

    private void Start()
    {
        SetScore(0);
        SetLives(3);
        m_GameOverPanel.SetActive(false);
        m_PausePanel.SetActive(false);
        ShowState(eGameState.Playing);
    }
    
    public void SetScore(int i_Value)
    {
        if (m_ScoreText) m_ScoreText.text = $"Score: {i_Value}";
    }
    
    public void SetLives(int i_Value)
    {
        if (m_LivesText) m_LivesText.text = $"Lives: {i_Value}";
    }       

    public void ShowState(eGameState i_GameState)
    {
        if (m_GameOverPanel) m_GameOverPanel.SetActive(i_GameState == eGameState.GameOver);
        if (m_PausePanel) m_PausePanel.SetActive(i_GameState == eGameState.Paused);
    }

    public void SetWeaponLevel(int i_WeaponLevel)
    {
        if(m_WeaponLevelText) m_WeaponLevelText.text = $"Wpn Lvl: {i_WeaponLevel}";
    }
    
    public void SetBombs(int i_Amount)
    {
        if (m_BombsAmountText) m_BombsAmountText.text = $"Bombs: {i_Amount}";
    }
}
