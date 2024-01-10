using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class Squere : MonoBehaviour
{
    private Cube _cube;


    //public Squere(Cube cube)
    //{
    //    _cube = cube;
    //}

    [Inject]
    private void Construct(Cube cube)
    {
        _cube = cube;
    }

    private void Start()
    {
        _cube.Work();
    }
}
