using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyRangeArrow : MonoBehaviour
{
    [SerializeField] private TestStart _testStart;
    [SerializeField] private TestFinish _testFinish;
    [SerializeField] private float _speed;

    private Coroutine _fly;

    internal void StartFly()
    {
        _fly = StartCoroutine(Fly());
    }

    internal void StopFly()
    {
        StopCoroutine(_fly);
    }

    private IEnumerator Fly()
    {
        while (transform.position != _testFinish.transform.position)
        {
            yield return null;
        }

        StopFly();
    }
}