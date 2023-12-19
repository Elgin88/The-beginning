using System.Collections;
using UnityEngine;

public class StateMove : State
{
    private Coroutine _move;

    public override void StartState()
    {
        if (_move == null)
        {
            _move = StartCoroutine(Move());
        }
    }

    public override void StopState()
    {
        if (_move != null)
        {
            StopCoroutine(_move);
            _move = null;
        }
    }

    public override void GetNextTransition()
    {
    }

    private IEnumerator Move()
    {
        while (true)
        {
            yield return null;
        }        
    }
}