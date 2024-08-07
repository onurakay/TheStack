using System;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static event Action<int> OnScoreChanged;
    public static event Action<int> OnComboChanged;

    private int currentScore;
    private int currentCombo;

    public void SetScore(int score)
    {
        currentScore = score;
        OnScoreChanged?.Invoke(score);
    }

    public void SetCombo(int combo)
    {
        currentCombo = combo;
        OnComboChanged?.Invoke(combo);
    }
}
