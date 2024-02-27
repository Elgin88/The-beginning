using Assets.Scripts.GameLogic.Damageable;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.PlayerUnits.UnitFiniteStateMachine
{
    internal class FiniteStateMachine
    {
        private FSMState _currentState;
        private Dictionary<Type, FSMState> _states = new Dictionary<Type, FSMState>();

        public Vector3 MovePosition { get; private set; }
        public IDamageable Target { get; private set; }

        public void AddState(FSMState state)
        {
            _states.Add(state.GetType(), state);
        }

        public void SetState<T>() where T : FSMState
        {
            var type = typeof(T);

            if (_currentState == null)
            {
                if (_states.TryGetValue(type, out var newState))
                {
                    _currentState = newState;

                    _currentState.Enter();

                    return;
                }
            }

            //если еще раз будет команда передвижения, он не выйдет из стейта
            if (_currentState.GetType() != type)
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
            Debug.Log(MovePosition);
        }

        public void SetEnemy(IDamageable target)
        {
            Target = target;
        }
    }
}