using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject comboField;

    void OnEnable()
    {
        GameState.OnScoreChanged += UpdateScore;
        GameState.OnComboChanged += UpdateCombo;
    }

    void OnDisable()
    {
        GameState.OnScoreChanged -= UpdateScore;
        GameState.OnComboChanged -= UpdateCombo;
    }

    private void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }

    private void UpdateCombo(int combo)
    {
        if (combo > 0)
        {
            comboField.SetActive(true);

            TextMeshProUGUI streakScoreComboText = comboField.GetComponentInChildren<TextMeshProUGUI>();
            if (streakScoreComboText != null)
            {
                streakScoreComboText.text = combo.ToString();
            }
        }
        else
        {
            comboField.SetActive(false);
        }
    }
}
