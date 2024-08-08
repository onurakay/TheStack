using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheStack : MonoBehaviour
{
    // Serialized Fields
    [SerializeField] Gradient colorGradient;
    [SerializeField] Material stackMat;
    [SerializeField] RubblePool rubblePool;

    // Constants
    const float boundsSize = 3.5f;
    const float stackMovingSpeed = 5f;
    const float errorMargin = 0.1f;
    const float stackBoundsGain = 0.25f;
    const float comboStartGain = 3f;

    GameObject[] theStack;
    Vector2 stackBounds = new Vector2(boundsSize, boundsSize);
    int currentStackIndex;
    int currentScore = 0;
    int currentCombo = 0;

    float tileTransition = 0.0f;
    float tileSpeed = 2.5f;
    float secondaryPosition;
    bool isMovingHorizontal = true;

    Vector3 velocity = Vector3.zero;
    Vector3 targetPosition;
    Vector3 lastTilePosition;

    bool gameOver = false;

    public GameState gameState;


    void Start()
    {
        theStack = new GameObject[transform.childCount];

        for (int i = 0; i < transform.childCount; i++)
        {
            theStack[i] = transform.GetChild(i).gameObject;
            ColorMesh(theStack[i].GetComponent<MeshFilter>().mesh);
        }

        currentStackIndex = transform.childCount - 1;
    }

    void Update()
    {
        if (gameOver)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (PlaceTile())
            {
                SpawnTile();
                currentScore++;
                UpdateGameState();
            }
            else
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
            }
        }

        MoveTile();
        transform.position = Vector3.Lerp(transform.position, targetPosition, 
            stackMovingSpeed * Time.deltaTime);
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

    Color32 Lerp4(Color32 a, Color32 b, Color32 c, Color32 d, float t)
    {
        if (t < 0.33f)
        {
            return Color.Lerp(a, b, t / 0.33f);
        }
        else if (t < 0.66f)
        {
            return Color.Lerp(b, c, (t - 0.33f) / 0.33f);
        }
        else
        {
            return Color.Lerp(c, d, (t - 0.66f) / 0.66f);
        }
    }

    void MoveTile()
    {
        tileTransition += Time.deltaTime * tileSpeed;

        if (isMovingHorizontal)
        {
            theStack[currentStackIndex].transform.localPosition =
                new Vector3(Mathf.Sin(tileTransition) * boundsSize,
                currentScore, secondaryPosition);
        }
        else
        {
            theStack[currentStackIndex].transform.localPosition =
                new Vector3(secondaryPosition,
                currentScore, Mathf.Sin(tileTransition) * boundsSize);
        }
    }

    bool PlaceTile()
    {
        Transform t = theStack[currentStackIndex].transform;

        if (isMovingHorizontal)
        {
            float deltaX = lastTilePosition.x - t.position.x;

            if (Mathf.Abs(deltaX) > errorMargin)
            {
                currentCombo = 0;
                stackBounds.x -= Mathf.Abs(deltaX);

                if (stackBounds.x <= 0)
                    return false;

                float middle = lastTilePosition.x + t.localPosition.x / 2f;
                t.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);

                CreateRubble(
                    new Vector3((t.position.x > 0) ?
                        t.position.x + (t.localScale.x / 2f) : t.position.x -
                        (t.localScale.x / 2f), t.position.y, t.position.z),
                    new Vector3(Mathf.Abs(deltaX), 1f, t.localScale.z));

                t.localPosition = new Vector3(middle - (lastTilePosition.x / 2f),
                currentScore, lastTilePosition.z);
            }
            else
            {
                if (currentCombo > comboStartGain)
                {
                    stackBounds.x += stackBoundsGain;

                    if (stackBounds.x > boundsSize)
                        stackBounds.x = boundsSize;

                    float middle = lastTilePosition.x + t.localPosition.x / 2f;
                    t.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);
                    t.localPosition = new Vector3(middle - (lastTilePosition.x / 2f),
                    currentScore, lastTilePosition.z);
                }

                currentCombo++;
                t.localPosition =
                    new Vector3(lastTilePosition.x, currentScore, lastTilePosition.z);
            }
        }
        else
        {
            float deltaZ = lastTilePosition.z - t.position.z;

            if (Mathf.Abs(deltaZ) > errorMargin)
            {
                currentCombo = 0;
                stackBounds.y -= Mathf.Abs(deltaZ);

                if (stackBounds.y <= 0)
                    return false;

                float middle = lastTilePosition.z + t.localPosition.z / 2f;
                t.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);

                CreateRubble(
                    new Vector3(t.position.x, t.position.y,
                        (t.position.z > 0) ? t.position.z + (t.localScale.z / 2f) :
                        t.position.z - (t.localScale.z / 2f)),
                    new Vector3(t.localScale.x, 1f, Mathf.Abs(deltaZ)));

                t.localPosition = new Vector3(
                lastTilePosition.x / 2f, currentScore, middle - (lastTilePosition.z / 2f));
            }
            else
            {
                if (currentCombo > comboStartGain)
                {
                    stackBounds.y += comboStartGain;

                    if (stackBounds.y > boundsSize)
                        stackBounds.y = boundsSize;

                    float middle = lastTilePosition.z + t.localPosition.z / 2f;
                    t.localScale = new Vector3(stackBounds.x, 1f, stackBounds.y);
                    t.localPosition = new Vector3(lastTilePosition.x / 2f,
                    currentScore, middle - (lastTilePosition.z / 2f));
                }

                currentCombo++;
                t.localPosition = new Vector3(lastTilePosition.x, currentScore,
                lastTilePosition.z);
            }
        }

        secondaryPosition = (isMovingHorizontal) ? t.localPosition.x : t.localPosition.z;
        isMovingHorizontal = !isMovingHorizontal;

        return true;
    }

    void CreateRubble(Vector3 pos, Vector3 scale)
    {
        GameObject rubble = rubblePool.GetRubble();
        rubble.transform.localPosition = pos;
        rubble.transform.localScale = scale;
        rubble.GetComponent<Rigidbody>().velocity = Vector3.zero;

        rubble.GetComponent<MeshRenderer>().material = stackMat;
        ColorMesh(rubble.GetComponent<MeshFilter>().mesh);
    }

    void SpawnTile()
    {
        lastTilePosition = theStack[currentStackIndex].transform.localPosition;
        currentStackIndex--;

        if (currentStackIndex < 0)
            currentStackIndex = transform.childCount - 1;

        targetPosition = Vector3.down * currentScore;

        theStack[currentStackIndex].transform.localPosition =
            new Vector3(0f, currentScore, 0f);

        theStack[currentStackIndex].transform.localScale =
            new Vector3(stackBounds.x, 1f, stackBounds.y);

        ColorMesh(theStack[currentStackIndex].GetComponent<MeshFilter>().mesh);
    }

    void UpdateGameState()
    {
        gameState.SetScore(currentScore);
        gameState.SetCombo(currentCombo);
    }
}
