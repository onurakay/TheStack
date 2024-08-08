using System.Collections.Generic;
using UnityEngine;

public class RubblePool : MonoBehaviour
{
    public GameObject rubblePrefab;
    public int initialPoolSize = 10;

    private Queue<GameObject> rubblePool;

    void Start()
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
            return null; 
        }

        GameObject rubble = rubblePool.Dequeue();
        rubble.SetActive(true);
        
        StartCoroutine(DeactivateRubble(rubble, 2f));

        return rubble;
    }

    private System.Collections.IEnumerator DeactivateRubble(GameObject rubble, float delay)
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
