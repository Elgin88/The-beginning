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

namespace Assets.Scripts.Tests
{
    internal class TestMover : MonoBehaviour
    {
        [SerializeField] private GameObject _target;

        private NavMeshAgent _navMeshAgent;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            _navMeshAgent.destination = _target.transform.position;
        }
    }
}
