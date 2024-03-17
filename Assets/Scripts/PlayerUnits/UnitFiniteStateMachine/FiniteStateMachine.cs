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

        public Vector3 MovePosition { get; private set; }
        public IDamageable Target { get; private set; }

        public FiniteStateMachine(UnitAnimator animator, NavMeshAgent agent, Unit unit, UnitData data)
        {
            AddState(new FSMStateIdle(this, unit, agent, animator, data));
            AddState(new FSMStateMove(this, unit, agent, animator, data));
            AddState(new FSMStateChaseEnemy(this, unit, agent, animator, data));
            AddState(new FSMStateAttack(this, unit, agent, animator, data));
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