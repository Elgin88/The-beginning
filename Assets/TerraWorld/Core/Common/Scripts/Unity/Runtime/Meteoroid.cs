using UnityEngine;

public class Meteoroid : MonoBehaviour
{
    public float speed;
    public ParticleSystem tail;
    public ParticleSystem head;
    public GameObject explostioneffect;

    private bool dead;

    private void Start()
    {
        Destroy(gameObject, 50);
    }

    void Update()
    {
        if(dead == false)
          transform.Translate(Vector3.forward * Time.deltaTime * speed);
    }

    private void OnCollisionEnter (Collision coll)
    {
        dead = true;
        tail.Stop();
        head.Stop();
        explostioneffect.SetActive(true);
        Destroy(gameObject, 5f);
    }
}

