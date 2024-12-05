using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheStack : MonoBehaviour
{
    private const float boundsSize = 3.5f;
    private const float stackMovingSpeed = 5f;

    [Header("General Settings")]
    [SerializeField] private Gradient colorGradient;
    [SerializeField] private Material stackMat;
    [SerializeField] private RubblePool rubblePool;
    [SerializeField] private GameMode gameMode = GameMode.Normal;

    [Header("Normal Mode Settings")]
    [SerializeField] private float normalErrorMargin = 0.1f;
    [SerializeField] private float normalStackBoundsGain = 0.25f;
    [SerializeField] private float normalComboStartGain = 3f;

    [Header("Easy Mode Settings")]
    [SerializeField] private float easyErrorMargin = 0.2f;
    [SerializeField] private float easyStackBoundsGain = 0.4f;
    [SerializeField] private float easyComboStartGain = 5f;

    [SerializeField] private float touchCooldownDuration = 0.2f;

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

    //singleton
    private GameState gameState;

    private bool isTouchCooldown = false;


    void Start()
    {
        theStack = new GameObject[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            theStack[i] = transform.GetChild(i).gameObject;
            ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
            SetupGlowEffect(theStack[i], false);
        }

        currentStackIndex = transform.childCount - 1;
        SetGameModeSettings();
        gameState = GameState.Instance;
    }

    void Update()
    {
        if (gameOver) return;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleTilePlacement();
            }
        }

        MoveTile();
        transform.position = Vector3.Lerp(transform.position, targetPosition,
            stackMovingSpeed * Time.deltaTime); // smooth update stack's position
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

    private void HandleTilePlacement()
    {
        if (isTouchCooldown) return;

        if (PlaceTile()) // returns true or fasle
        {
            StartCoroutine(TouchCooldown());
            SpawnTile();
            VibrateOnPlacement();
            currentScore++;
            UpdateGameState();
        }
        else
        {
            gameState.TriggerGameOver();
        }
    }

    private void HandleTouchInput()
    {
        if (Input.touchCount > 0)
        {
            // Only consider the first touch to avoid conflicts
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                HandleTilePlacement();
            }
        }
    }

    bool PlaceTile()
    {
        Transform t = theStack[currentStackIndex].transform;

        if (isMovingHorizontal) //horizontal
        {
            float deltaX = lastTilePosition.x - t.position.x;

            if (Mathf.Abs(deltaX) > errorMargin) // misaligned
            {
                currentCombo = 0;
                stackBounds.x -= Mathf.Abs(deltaX); // shrink the stack bounds

                if (stackBounds.x <= 0) return false; // Game over if the stack bounds reach zero

                float middle = lastTilePosition.x + t.localPosition.x / 2f;
                t.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);

                CreateRubble(new Vector3(
                    t.position.x > 0 ? t.position.x + t.localScale.x / 2f : t.position.x - t.localScale.x / 2f,
                    t.position.y, t.position.z),
                    new Vector3(Mathf.Abs(deltaX), 1f, t.localScale.z));

                // snapping to the rounded position
                t.localPosition = new Vector3(
                    Mathf.Round((middle - lastTilePosition.x / 2f) * 100f) / 100f,
                    currentScore,
                    Mathf.Round(lastTilePosition.z * 100f) / 100f
                );
            }
            else
            {
                currentCombo++;
                // alignment with the previous tile
                t.localPosition = new Vector3(
                    Mathf.Round(lastTilePosition.x * 100f) / 100f,
                    currentScore,
                    Mathf.Round(lastTilePosition.z * 100f) / 100f
                );
            }
        }
        else // vertical
        {
            float deltaZ = lastTilePosition.z - t.position.z;

            if (Mathf.Abs(deltaZ) > errorMargin) // misaligned
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

                // snapping to the rounded position
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

        SetupGlowEffect(t.gameObject, true);

        if (currentCombo % 3 == 0 && currentCombo > 0)
        {
            PlayComboEffect(t.gameObject);
        }
        secondaryPosition = isMovingHorizontal ? t.localPosition.x : t.localPosition.z;
        isMovingHorizontal = !isMovingHorizontal;

        return true;
    }

    void MoveTile()
    {
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


    void SpawnTile()
    {
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

    private IEnumerator TouchCooldown()
    {
        isTouchCooldown = true;
        yield return new WaitForSeconds(touchCooldownDuration);
        isTouchCooldown = false;
    }

    IEnumerator FadeInOutGlowEffect(GameObject tile)
    {
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

                    // fade in
                    float elapsed = 0f;
                    while (elapsed < halfDuration)
                    {
                        float alpha = Mathf.Lerp(0f, 1f, elapsed / halfDuration);
                        mat.color = new Color(color.r, color.g, color.b, alpha);
                        elapsed += Time.deltaTime;
                        yield return null;
                    }
                    mat.color = new Color(color.r, color.g, color.b, 1f);

                    // fade out
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

    void ColorMesh(Mesh mesh)
    {
        Vector3[] vertices = mesh.vertices;
        Color32[] colors = new Color32[vertices.Length];
        float t = Mathf.Sin(currentScore * 0.1f);

        for (int i = 0; i < vertices.Length; i++)
        {
            colors[i] = colorGradient.Evaluate(t);
        }

        mesh.colors32 = colors;
    }

    void SetupGlowEffect(GameObject tile, bool activate)
    {
        // killing existing coroutine for the tile
        if (glowCoroutines.ContainsKey(tile))
        {
            StopCoroutine(glowCoroutines[tile]);
            glowCoroutines.Remove(tile);
        }

        if (activate)
        {
            // fadein, fadeout effect
            Coroutine fadeCoroutine = StartCoroutine(FadeInOutGlowEffect(tile));
            glowCoroutines[tile] = fadeCoroutine;
        }
        else
        {
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

    void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject rubble = rubblePool.GetRubble();
        rubble.transform.localPosition = pos;
        rubble.transform.localScale = scale;
        rubble.GetComponent<Rigidbody>().velocity = Vector3.zero;

        rubble.GetComponent<MeshRenderer>().material = stackMat;
        ColorMesh(rubble.GetComponent<MeshFilter>().mesh);
        SetupGlowEffect(rubble, false);
    }

    void VibrateOnPlacement()
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            Handheld.Vibrate();
        }
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            Handheld.Vibrate();
        }
    }

    void PlayMilestoneEffect(GameObject tile)
    {
        Transform particleEffect = tile.transform.Find("MilestoneEffect");
        if (particleEffect != null)
        {
            var particleSystem = particleEffect.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
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

    void PlayComboEffect(GameObject tile)
    {
        Transform comboEffect = tile.transform.Find("ComboEffect");

        // multiple effects for the future
        var particleSystems = comboEffect.GetComponentsInChildren<ParticleSystem>();
        foreach (var ps in particleSystems)
        {
            ps.Play();
        }

    }

    void UpdateGameState()
    {
        gameState.SetScore(currentScore);
        gameState.SetCombo(currentCombo);

        if (currentScore % 5 == 0)
        {
            PlayMilestoneEffect(theStack[previousStackIndex]);
        }
    }
}