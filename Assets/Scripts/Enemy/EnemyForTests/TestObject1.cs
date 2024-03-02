using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestObject1 : MonoBehaviour
{
    [SerializeField] private TestObject2 _testObjects2;

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(_testObjects2.transform.position, Vector3.up);

        Debug.DrawRay(transform.position, transform.forward * 20, Color.green);

        Debug.DrawLine(transform.position, _testObjects2.transform.position * 5, Color.red);
    }
}