using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class State : MonoBehaviour
{
    [SerializeField] private List<Transition> _transitions;

    public abstract void StartState();
    public abstract void StopState();
    public abstract void GetNextTransition();
}