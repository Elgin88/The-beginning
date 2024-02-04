using UnityEngine;

public class MovePlant : MonoBehaviour
{
    public float minTime;
    public float maxTime;

    private Rigidbody rb;
    private float currentTime; 
    private float timer;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        currentTime = Random.Range(minTime, maxTime);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= currentTime)
        {
            rb.AddForce(Vector3.forward * 1f, ForceMode.Impulse);
            timer = 0;
            currentTime = Random.Range(minTime, maxTime);
        }
    }
}

