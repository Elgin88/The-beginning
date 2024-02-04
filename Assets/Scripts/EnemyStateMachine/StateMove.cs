using Assets.Scripts.BuildingSystem.Buildings;
using Assets.Scripts.Enemy;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(Animator))]

    internal class StateMove : State
    {
        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private NavMeshAgent _navMeshAgent;
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

        public override State GetNextState()
        {
            return null;
        }

        public override void GetNextTransition()
        {
        }

        private void Awake()
        {
            _navMeshAgent = GetComponent<NavMeshAgent>();
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();
        }

        private IEnumerator Move()
        {
            while (true)
            {
                if (_navMeshAgent != null & _enemyNextTargetFinder != null)
                {
                    _navMeshAgent.destination = _enemyNextTargetFinder.NextTarget.transform.position;
                }

                yield return null;
            }
        }
    }
}