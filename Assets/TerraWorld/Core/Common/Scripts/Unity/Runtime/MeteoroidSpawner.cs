using UnityEngine;

public class MeteoroidSpawner : MonoBehaviour
{
    public GameObject meteoroidObject;
    public float timeMin;
    public float timeMax;

    private float randomTime;
    private GameObject currentSpawn;

    void Start()
    {
        float startRsandom = Random.Range(5, 15);
        randomTime = Random.Range(timeMin, timeMax);
        InvokeRepeating("SpawnMeteoroid", startRsandom, randomTime);
    }

    public void SpawnMeteoroid ()
    {
        if (currentSpawn != null) return;
        randomTime = Random.Range(timeMin, timeMax);

#pragma warning disable CS0618 // Type or member is obsolete
        currentSpawn = Instantiate(meteoroidObject, transform.position, Quaternion.EulerAngles(new Vector3(45, 0, 0)),transform);
#pragma warning restore CS0618 // Type or member is obsolete

        currentSpawn.transform.eulerAngles = new Vector3 (45, Random.Range(0,360), 0);
    }
}

