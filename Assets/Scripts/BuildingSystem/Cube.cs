using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;


public class Cube : MonoBehaviour
{
    public void Start()
    {
        Debug.Log("Куб стартанул");
    }

    public void Work()
    {
        Debug.Log("Куб работает по приказу квадрата");
    }
}
