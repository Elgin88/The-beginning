using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Assets.Scripts.GameLogic.Damageable;

namespace Assets.Scripts.PlayerUnits.UnitFiniteStateMachine
{
    internal class FiniteStateMachine
    {
        private FSMState _currentState;
        private Dictionary<Type, FSMState> _states = new Dictionary<Type, FSMState>();

        private UnitAnimator _animator;
        private NavMeshAgent _agent;
        private Unit _unit;

        public Vector3 MovePosition { get; private set; }
        public IDamageable Target { get; private set; }

        public FiniteStateMachine(UnitAnimator animator, NavMeshAgent agent, Unit unit)
        {
            _animator = animator;
            _agent = agent;
            _unit = unit;

            AddState(new FSMStateIdle(this, _unit, _agent, _animator));
            AddState(new FSMStateMove(this, _unit, _agent, _animator));
            AddState(new FSMStateChaseEnemy(this, _unit, _agent, _animator));
            AddState(new FSMStateAttack(this, _unit, _agent, _animator));
        }

        public void AddState(FSMState state)
        {
            _states.Add(state.GetType(), state);
        }

        public void SetState<T>() where T : FSMState
        {
            var type = typeof(T);

            if (_currentState?.GetType() == typeof(FSMStateMove) || _currentState?.GetType() != type)
            {
                if (_states.TryGetValue(type, out var newState))
                {
                    _currentState?.Exit();

                    _currentState = newState;

                    _currentState.Enter();
                }
            }
        }

        public void Update()
        {
            _currentState?.Update();
        }

        public void SetMovePosition(Vector3 position)
        {
            MovePosition = position;
        }

        public void SetEnemy(IDamageable target)
        {
            Target = target;
        }
    }
}