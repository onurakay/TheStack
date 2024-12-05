using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [Header("Color Transition Settings")]
    [Tooltip("List of colors to cycle through based on score increments.")]
    [SerializeField] private Color[] colorList;

    [Tooltip("Speed of color transition.")]
    [SerializeField] private float transitionSpeed = 2f;

    [Tooltip("Number of score points required to change the background color.")]
    [SerializeField] private int scoreThreshold = 5;

    private Camera mainCamera;
    private Color targetColor;
    private int currentColorIndex = 0;
    private int lastScoreForColorChange = 0;

    private Coroutine colorTransitionCoroutine;

    void Start()
    {
        InitializeCamera();
    }

    private void OnEnable()
    {
        GameState.OnScoreChanged += OnScoreChanged; // subscribe
    }

    private void OnDisable()
    {
        GameState.OnScoreChanged -= OnScoreChanged; //unsubscribe
    }

    private void InitializeCamera()
    {
        mainCamera = Camera.main;

        if (colorList.Length == 0)
        {
            Debug.LogError("empty color list");
            return;
        }

        targetColor = colorList[currentColorIndex];
        mainCamera.backgroundColor = targetColor;
    }

    private void OnScoreChanged(int newScore)
    {
        int deltaScore = newScore - lastScoreForColorChange;

        if (deltaScore >= scoreThreshold)
        {
            int colorChanges = deltaScore / scoreThreshold; // threshold count

            for (int i = 0; i < colorChanges; i++)
            {
                currentColorIndex = (currentColorIndex + 1) % colorList.Length;
            }
            targetColor = colorList[currentColorIndex];
            lastScoreForColorChange += colorChanges * scoreThreshold;

            if (colorTransitionCoroutine != null)
            {
                StopCoroutine(colorTransitionCoroutine);
            }
            colorTransitionCoroutine = StartCoroutine(TransitionBackgroundColor(targetColor));
        }
    }

    private IEnumerator TransitionBackgroundColor(Color newColor)
    {
        Color initialColor = mainCamera.backgroundColor;
        float elapsedTime = 0f;
        float duration = 1f / transitionSpeed;

        while (elapsedTime < duration)
        {
            mainCamera.backgroundColor = Color.Lerp(initialColor, newColor, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        mainCamera.backgroundColor = newColor;
    }
}
