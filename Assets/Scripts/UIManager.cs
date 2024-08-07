using UnityEngine;
using TMPro; // Import TextMesh Pro namespace

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject comboField; // Reference to the streak field GameObject

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
            // Show streak field
            comboField.SetActive(true);

            // Assuming comboField contains a TextMeshProUGUI component for the streak score combo
            TextMeshProUGUI streakScoreComboText = comboField.GetComponentInChildren<TextMeshProUGUI>();
            if (streakScoreComboText != null)
            {
                streakScoreComboText.text = combo.ToString();
            }
        }
        else
        {
            // Hide streak field
            comboField.SetActive(false);
        }
    }
}
