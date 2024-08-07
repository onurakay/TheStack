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
            // The pool is empty, but we won't create more. 
            // You might log a warning here if needed.
            return null; 
        }

        GameObject rubble = rubblePool.Dequeue();
        rubble.SetActive(true);
        
        // Start deactivation coroutine
        StartCoroutine(DeactivateRubble(rubble, 2f)); // Deactivate after 2 seconds

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
