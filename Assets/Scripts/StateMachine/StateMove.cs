using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMove : State
{
    private Coroutine _move;

    private IEnumerator Move()
    {
        yield return null;
    }

    private void StartMove()
    {
        if (_move == null)
        {
            _move = StartCoroutine(Move());
        }
    }

    private void StopMove()
    {
        if (_move != null)
        {
            StopCoroutine(_move);
            _move = null;
        }
    }
}