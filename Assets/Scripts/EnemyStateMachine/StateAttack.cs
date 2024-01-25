using System.Collections;
using Assets.Scripts.Enemy;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(Animator))]

    internal class StateAttack : State
    {
        private EnemyAnimation _enemyAnimation;
        private WaitForSeconds _speedOfAttackWFS;
        private Coroutine _attack;
        private float _speedOfAttack = 1;

        internal override bool IsNeedNextState { get; set; }

        internal override State GetNextState()
        {
            throw new System.NotImplementedException();
        }

        internal override void StartState()
        {
            if (_attack == null)
            {
                _attack = StartCoroutine(Attack());
            }
        }

        internal override void StopState()
        {
            if (_attack != null)
            {
                StopCoroutine(_attack);
                _attack = null;
            }
        }

        private void Awake()
        {
            _enemyAnimation = GetComponent<EnemyAnimation>();

            _speedOfAttackWFS = new WaitForSeconds(_speedOfAttack);
        }

        private IEnumerator Attack()
        {
            while (true)
            {
                _enemyAnimation.Attack();
                yield return _speedOfAttackWFS;
            }
        }
    }
}