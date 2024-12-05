using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private GameObject comboField;
    [SerializeField] private GameObject regularCanvas;
    [SerializeField] private GameObject endGameCanvas;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private TextMeshProUGUI highComboText;
    [SerializeField] private TextMeshProUGUI recordMessageText;
    [SerializeField] private Button againButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private PostProcessVolume postProcessVolume;


    void Start()
    {
        postProcessVolume.weight = 0f;
        InitializeButtons();
    }

    private void OnEnable() //subscribe
    {
        GameState.OnScoreChanged += UpdateScore;
        GameState.OnComboChanged += UpdateCombo;
        GameState.OnGameOver += ShowEndGameUI;
    }

    private void OnDisable() //unsubscribe
    {
        GameState.OnScoreChanged -= UpdateScore;
        GameState.OnComboChanged -= UpdateCombo;
        GameState.OnGameOver -= ShowEndGameUI;
    }

    private void InitializeButtons()
    {
        againButton.onClick.AddListener(RestartGame);
        resetButton.onClick.AddListener(ResetPlayerPrefs);
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

    private IEnumerator FadeInBlur(float duration)
    {
        float elapsedTime = 0f;

        while (elapsedTime <= duration)
        {
            float weight = Mathf.SmoothStep(0f, 1f, elapsedTime / duration);
            postProcessVolume.weight = weight;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        postProcessVolume.weight = 1f;
    }

    private void ShowEndGameUI()
    {
        regularCanvas.SetActive(false);
        endGameCanvas.SetActive(true);

        StartCoroutine(FadeInBlur(2f));

        //highscore
        int playerScore = GameState.Instance.GetCurrentScore();
        int highScore = GameState.Instance.GetHighScore();
        highScoreText.text = $"{playerScore}<size=50%>/{highScore}</size>";

        //highcombo
        int playerCombo = GameState.Instance.GetCurrentCombo();
        int highCombo = GameState.Instance.GetHighCombo();
        highComboText.text = $"{playerCombo}<size=50%>/{highCombo}</size>";

        recordMessageText.text = GameState.Instance.IsNewRecord ? "New Record!" : "Try Again!";
    }

    public void RestartGame()
    {
        GameState.Instance.ResetGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResetPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        GameState.Instance.ResetGame();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
