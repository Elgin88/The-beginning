using UnityEngine;

public class TestPlayer : MonoBehaviour
{
    private void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        Debug.DrawRay(transform.position, ray.direction * 100, Color.red, 0.1f);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log(hit.collider.gameObject.name);
        }
    }
}