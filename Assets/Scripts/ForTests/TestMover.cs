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
using Zenject;

namespace Assets.Scripts.Tests
{
    internal class TestMover : MonoBehaviour
    {
        [Inject] private PlayerMainBilding _playerMainBildin;

        private NavMeshAgent _navMeshAgent;

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            Debug.Log(_playerMainBildin.transform.position);
          
            _navMeshAgent.destination = _playerMainBildin.transform.position;
        }
    }
}
