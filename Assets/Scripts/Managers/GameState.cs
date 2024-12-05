using System;
using UnityEngine;

public class GameState : MonoBehaviour
{
    // singleton
    public static GameState Instance { get; private set; }

    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnComboChanged;
    public static event Action OnGameOver;

    // Current game state
    private int currentScore;
    private int currentCombo;

    private int highScore = 0;
    private int highCombo = 0;

    private bool isNewRecord = false;

    public bool IsNewRecord => isNewRecord;
    public int GetCurrentScore() => currentScore;
    public int GetCurrentCombo() => currentCombo;

    private void Awake()
    {
        // implementing singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        highScore = PlayerPrefs.GetInt("HighScore", 0);
        highCombo = PlayerPrefs.GetInt("HighCombo", 0);
    }

    // private void Update()
    // {
    //     Debug.Log($"Current score: {currentScore}");
    // }

    public void SetScore(int score)
    {
        currentScore = score;
        OnScoreChanged?.Invoke(score);
        CheckAndUpdateHighScore(score);
    }

    public void SetCombo(int combo)
    {
        if (combo > currentCombo)
        {
            currentCombo = combo;
        }
        OnComboChanged?.Invoke(combo);
        CheckAndUpdateHighCombo(combo);
    }

    public void TriggerGameOver()
    {
        OnGameOver?.Invoke();
    }

    public void ResetGame()
    {
        currentScore = 0;
        currentCombo = 0;
        isNewRecord = false;
        OnScoreChanged?.Invoke(currentScore);
        OnComboChanged?.Invoke(currentCombo);
    }

    public int GetHighScore() => highScore;
    public int GetHighCombo() => highCombo;

    private void CheckAndUpdateHighScore(int score)
    {
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("HighScore", highScore);
            isNewRecord = true;
        }
        else
        {
            isNewRecord = false;
        }
    }

    private void CheckAndUpdateHighCombo(int combo)
    {
        if (combo > highCombo)
        {
            highCombo = combo;
            PlayerPrefs.SetInt("HighCombo", highCombo);
        }
    }
}
