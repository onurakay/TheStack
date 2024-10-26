using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Color Transition Settings")]
    [Tooltip("List of colors to cycle through based on score increments.")]
    [SerializeField] private Color[] colorList;

    [Tooltip("Speed of color transition.")]
    [SerializeField] private float transitionSpeed = 2f; // Speed of color transition

    [Tooltip("Number of score points required to change the background color.")]
    [SerializeField] private int scoreThreshold = 5; // Change color every 'scoreThreshold' points
    
    private Camera mainCamera;
    private Color targetColor;
    private int currentColorIndex = 0;
    private int lastScoreForColorChange = 0;

    void Start()
    {
        mainCamera = Camera.main;

        if (colorList.Length == 0)
        {
            Debug.LogError("Color List is empty! Please assign at least one color in the Inspector.");
            return;
        }

        // Initialize with the first color in the list
        targetColor = colorList[currentColorIndex];
        mainCamera.backgroundColor = targetColor;
    }

    private void OnEnable()
    {
        GameState.OnScoreChanged += OnScoreChanged;
    }

    private void OnDisable()
    {
        GameState.OnScoreChanged -= OnScoreChanged;
    }

    /// <summary>
    /// Called whenever the score changes.
    /// </summary>
    /// <param name="newScore">The updated score.</param>
    private void OnScoreChanged(int newScore)
    {
        int deltaScore = newScore - lastScoreForColorChange;

        if (deltaScore >= scoreThreshold)
        {
            // Calculate how many times the threshold has been passed
            int colorChanges = deltaScore / scoreThreshold;

            for (int i = 0; i < colorChanges; i++)
            {
                currentColorIndex = (currentColorIndex + 1) % colorList.Length;
            }

            // Set the new target color
            targetColor = colorList[currentColorIndex];

            // Update the last score that triggered a color change
            lastScoreForColorChange += colorChanges * scoreThreshold;
        }
    }

    void Update()
    {
        // Smoothly transition to the target color
        if (mainCamera.backgroundColor != targetColor)
        {
            mainCamera.backgroundColor = Color.Lerp(
                mainCamera.backgroundColor,
                targetColor,
                Time.deltaTime * transitionSpeed
            );
        }
    }
}
