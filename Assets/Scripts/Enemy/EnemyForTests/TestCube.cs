using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCube : MonoBehaviour
{
    [SerializeField] private GameObject _target;

    private void Update()
    {
        transform.rotation = Quaternion.LookRotation(_target.transform.position, Vector3.up);
    }
}