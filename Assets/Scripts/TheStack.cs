using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections.Concurrent; // Ensure this is included with other using statements

public class TheStack : MonoBehaviour
{
    // Serialized Fields
    [Header("General Settings")]
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private Material stackMat;
    [SerializeField] private RubblePool rubblePool;
    [SerializeField] private GameMode gameMode = GameMode.Normal;

    // Game Mode Settings
    [Header("Normal Mode Settings")]
    [SerializeField] private float normalErrorMargin = 0.1f;
    [SerializeField] private float normalStackBoundsGain = 0.25f;
    [SerializeField] private float normalComboStartGain = 3f;

    [Header("Easy Mode Settings")]
    [SerializeField] private float easyErrorMargin = 0.2f;
    [SerializeField] private float easyStackBoundsGain = 0.4f;
    [SerializeField] private float easyComboStartGain = 5f;

    // Constants
    private const float boundsSize = 3.5f;
    private const float stackMovingSpeed = 5f;

    // Game State Variables
    private Dictionary<GameObject, Coroutine> glowCoroutines = new Dictionary<GameObject, Coroutine>();
    private GameObject[] theStack;
    private Vector2 stackBounds = new Vector2(boundsSize, boundsSize);

    private int currentStackIndex;
    private int previousStackIndex;
    private int currentScore = 0;
    private int currentCombo = 0;

    private float errorMargin;
    private float stackBoundsGain;
    private float comboStartGain;

    private float tileTransition = 0.0f;
    private float tileSpeed = 2.5f;
    private float secondaryPosition;

    private bool isMovingHorizontal = true;
    private bool gameOver = false;

    private Vector3 velocity = Vector3.zero;
    private Vector3 targetPosition;
    private Vector3 lastTilePosition;

    public GameState gameState;


    void Start()
    {
        // Initialize theStack array and color meshes
        theStack = new GameObject[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            theStack[i] = transform.GetChild(i).gameObject;
            ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);

            // Setup and deactivate the GlowEffect initially
            SetupGlowEffect(theStack[i], false);
        }

        currentStackIndex = transform.childCount - 1;

        // Set mode-specific settings
        SetGameModeSettings();
    }

    private void SetGameModeSettings()
    {
        switch (gameMode)
        {
            case GameMode.Normal:
                errorMargin = normalErrorMargin;
                stackBoundsGain = normalStackBoundsGain;
                comboStartGain = normalComboStartGain;
                break;
            case GameMode.Easy:
                errorMargin = easyErrorMargin;
                stackBoundsGain = easyStackBoundsGain;
                comboStartGain = easyComboStartGain;
                break;
        }
    }

    void Update()
    {
        if (gameOver) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceTile()) // if placement is successful
            {
                SpawnTile();
                currentScore++;
                UpdateGameState();
            }
            else
            {
                // change restarting the scene if placement fails with menu
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                    UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }

        MoveTile(); // Move the current tile back and forth
        transform.position = Vector3.Lerp(transform.position, targetPosition,
            stackMovingSpeed * Time.deltaTime); // Smoothly update the stack's position
    }

    void ColorMesh(Mesh mesh)
    {
        // Apply gradient coloring based on the current score
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];
        float t = Mathf.Sin(currentScore * 0.1f);

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = colorGradient.Evaluate(t);
        }

        mesh.colors32 = colors;
    }

    void MoveTile()
    {
        // Move the current tile along the appropriate axis
        tileTransition += Time.deltaTime * tileSpeed;

        if (isMovingHorizontal)
        {
            theStack[currentStackIndex].transform.localPosition =
                new Vector3(Mathf.Sin(tileTransition) * boundsSize, currentScore, secondaryPosition);
        }
        else
        {
            theStack[currentStackIndex].transform.localPosition =
                new Vector3(secondaryPosition, currentScore, Mathf.Sin(tileTransition) * boundsSize);
        }
    }

    bool PlaceTile()
    {
        // Handles the placement of a tile and checks for alignment
        Transform t = theStack[currentStackIndex].transform;

        if (isMovingHorizontal)
        {
            float deltaX = lastTilePosition.x - t.position.x;

            if (Mathf.Abs(deltaX) > errorMargin) // Misaligned
            {
                currentCombo = 0; // Reset combo
                stackBounds.x -= Mathf.Abs(deltaX); // Shrink the stack bounds

                if (stackBounds.x <= 0) return false; // Game over if the stack bounds reach zero

                float middle = lastTilePosition.x + t.localPosition.x / 2f;
                t.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);

                // Create a rubble piece for the cut-off part
                CreateRubble(new Vector3(
                    t.position.x > 0 ? t.position.x + t.localScale.x / 2f : t.position.x - t.localScale.x / 2f,
                    t.position.y, t.position.z),
                    new Vector3(Mathf.Abs(deltaX), 1f, t.localScale.z));

                // Snap to the rounded position to prevent drift
                t.localPosition = new Vector3(
                    Mathf.Round((middle - lastTilePosition.x / 2f) * 100f) / 100f,
                    currentScore,
                    Mathf.Round(lastTilePosition.z * 100f) / 100f
                );
            }
            else
            {
                currentCombo++; // Increase combo
                // Align perfectly with the previous tile
                t.localPosition = new Vector3(
                    Mathf.Round(lastTilePosition.x * 100f) / 100f,
                    currentScore,
                    Mathf.Round(lastTilePosition.z * 100f) / 100f
                );
            }
        }
        else // Vertical movement
        {
            float deltaZ = lastTilePosition.z - t.position.z;

            if (Mathf.Abs(deltaZ) > errorMargin) // Misaligned
            {
                currentCombo = 0;
                stackBounds.y -= Mathf.Abs(deltaZ);

                if (stackBounds.y <= 0) return false;

                float middle = lastTilePosition.z + t.localPosition.z / 2f;
                t.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);

                CreateRubble(new Vector3(
                    t.position.x, t.position.y,
                    t.position.z > 0 ? t.position.z + t.localScale.z / 2f : t.position.z - t.localScale.z / 2f),
                    new Vector3(t.localScale.x, 1f, Mathf.Abs(deltaZ)));

                // Snap to the rounded position
                t.localPosition = new Vector3(
                    Mathf.Round(lastTilePosition.x * 100f) / 100f,
                    currentScore,
                    Mathf.Round((middle - lastTilePosition.z / 2f) * 100f) / 100f
                );
            }
            else
            {
                currentCombo++;
                t.localPosition = new Vector3(
                    Mathf.Round(lastTilePosition.x * 100f) / 100f,
                    currentScore,
                    Mathf.Round(lastTilePosition.z * 100f) / 100f
                );
            }
        }

        // Start the glow effect for the placed tile
        SetupGlowEffect(t.gameObject, true);

        secondaryPosition = isMovingHorizontal ? t.localPosition.x : t.localPosition.z;
        isMovingHorizontal = !isMovingHorizontal;

        return true;
    }

    void CreateRubble(Vector3 pos, Vector3 scale)
    {
        // Generate a rubble piece at the given position and scale
        GameObject rubble = rubblePool.GetRubble();
        rubble.transform.localPosition = pos;
        rubble.transform.localScale = scale;
        rubble.GetComponent<Rigidbody>().velocity = Vector3.zero;

        rubble.GetComponent<MeshRenderer>().material = stackMat;
        ColorMesh(rubble.GetComponent<MeshFilter>().mesh);
        SetupGlowEffect(rubble, false); // Disable glow for rubble
    }



    void SpawnTile()
    {
        // Store the current tile's index as the previous one (since we are about to place the next tile)
        previousStackIndex = currentStackIndex;

        lastTilePosition = theStack[currentStackIndex].transform.localPosition;
        currentStackIndex--;

        if (currentStackIndex < 0)
            currentStackIndex = transform.childCount - 1;

        targetPosition = Vector3.down * currentScore;

        theStack[currentStackIndex].transform.localPosition = new Vector3(0f, currentScore, 0f);
        theStack[currentStackIndex].transform.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);

        ColorMesh(theStack[currentStackIndex].GetComponent<MeshFilter>().mesh);
    }

    void SetupGlowEffect(GameObject tile, bool activate)
    {
        // If there's an existing coroutine for this tile, stop it
        if (glowCoroutines.ContainsKey(tile))
        {
            StopCoroutine(glowCoroutines[tile]);
            glowCoroutines.Remove(tile);
        }

        if (activate)
        {
            // Start the fade-in and fade-out effect
            Coroutine fadeCoroutine = StartCoroutine(FadeInOutGlowEffect(tile));
            glowCoroutines[tile] = fadeCoroutine;
        }
        else
        {
            // Ensure the glow is fully transparent
            Transform glowEffect = tile.transform.Find("GlowEffect");
            if (glowEffect != null)
            {
                Transform plane = glowEffect.Find("Plane");
                if (plane != null)
                {
                    MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        Material mat = renderer.material;
                        Color color = mat.color;
                        mat.color = new Color(color.r, color.g, color.b, 0f);
                    }
                }
            }
        }
    }

    IEnumerator FadeInOutGlowEffect(GameObject tile)
    {
        // Get the GlowEffect's Plane child
        Transform glowEffect = tile.transform.Find("GlowEffect");
        if (glowEffect != null)
        {
            Transform plane = glowEffect.Find("Plane");
            if (plane != null)
            {
                MeshRenderer renderer = plane.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    Material mat = renderer.material;
                    Color color = mat.color;
                    float duration = 1f;
                    float halfDuration = duration / 2f;

                    // Fade in
                    float elapsed = 0f;
                    while (elapsed < halfDuration)
                    {
                        float alpha = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
                        mat.color = new Color(color.r, color.g, color.b, alpha);
                        elapsed += Time.deltaTime;
                        yield return null;
                    }
                    mat.color = new Color(color.r, color.g, color.b, 1f);

                    // Fade out
                    elapsed = 0f;
                    while (elapsed < halfDuration)
                    {
                        float alpha = Mathf.Lerp(1f, 0f, elapsed / halfDuration);
                        mat.color = new Color(color.r, color.g, color.b, alpha);
                        elapsed += Time.deltaTime;
                        yield return null;
                    }
                    mat.color = new Color(color.r, color.g, color.b, 0f);
                }
                else
                {
                    Debug.LogWarning($"No MeshRenderer found on Plane in {tile.name}");
                }
            }
            else
            {
                Debug.LogWarning($"No Plane child found under GlowEffect in {tile.name}");
            }
        }
        else
        {
            Debug.LogWarning($"No GlowEffect found in {tile.name}");
        }
    }

    void UpdateGameState()
    {
        gameState.SetScore(currentScore);
        gameState.SetCombo(currentCombo);

        if (currentScore % 5 == 0)
        {
            PlayParticleEffect(theStack[previousStackIndex]); // Trigger on the last placed block
        }
    }

void PlayParticleEffect(GameObject tile)
{
    Transform particleEffect = tile.transform.Find("MilestoneEffect");
    if (particleEffect != null)
    {
        var particleSystem = particleEffect.GetComponent<ParticleSystem>();
        if (particleSystem != null)
        {
            // Check if the combo is greater than 0 and set the color to red
            if (currentCombo > 0)
            {
                var main = particleSystem.main;
                main.startColor = Color.red; // Set the milestone effect color to red
            }

            particleSystem.Play();
        }
        else
        {
            Debug.LogWarning($"No ParticleSystem found on {particleEffect.name}.");
        }
    }
    else
    {
        Debug.LogWarning($"No MilestoneEffect child found in {tile.name}.");
    }
}

}