using Assets.Scripts.Bildings;
using Assets.Scripts.Camera;
using Assets.Scripts.ConStants;
using Assets.Scripts.Enemy;
using Assets.Scripts.UnitStateMachine;
using Assets.Scripts.Tests;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using ModestTree;

namespace Assets.Scripts.Tests
{
    internal class TestCamera: MonoBehaviour
    {
        [SerializeField] private GameObject _orc;

        private void Update()
        {
            transform.position = new Vector3(_orc.transform.position.x, 20, _orc.transform.position.z - 20);
        }
    }
}