using System.Collections.Generic;
using UnityEngine;

public class RubblePool : MonoBehaviour
{
    public GameObject rubblePrefab;
    public int initialPoolSize = 10;
    public int poolExpansionSize = 5;

    private Queue<GameObject> rubblePool;

    void Awake()
    {
        InitializePool();
    }

    void InitializePool()
    {
        rubblePool = new Queue<GameObject>();

        for (int i = 0; i < initialPoolSize; i++)
        {
            GameObject rubble = Instantiate(rubblePrefab);
            rubble.SetActive(false);
            rubblePool.Enqueue(rubble);
        }
    }

    public GameObject GetRubble()
    {
        if (rubblePool.Count == 0)
        {
            ExpandPool(poolExpansionSize);
        }

        GameObject rubble = rubblePool.Dequeue();
        rubble.SetActive(true);

        StartCoroutine(DeactivateRubble(rubble, 2f));

        return rubble;
    }

    private void ExpandPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject rubble = Instantiate(rubblePrefab);
            rubble.SetActive(false);
            rubblePool.Enqueue(rubble);
        }
        Debug.Log($"Rubble pool expanded by {count}. New pool size: {rubblePool.Count}");
    }

    private IEnumerator<WaitForSeconds> DeactivateRubble(GameObject rubble, float delay)
    {
        yield return new WaitForSeconds(delay);
        ReturnRubble(rubble);
    }

    public void ReturnRubble(GameObject rubble)
    {
        rubble.SetActive(false);
        rubblePool.Enqueue(rubble);
    }
}
