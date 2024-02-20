using System.Collections;
using Assets.Scripts.Enemy;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(EnemyNextTargetFinder))]
    [RequireComponent(typeof(TransitionAttack))]
    [RequireComponent(typeof(EnemyAnimation))]

    internal class StateAttack : State
    {
        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private TransitionAttack _transitionAttack;
        private EnemyAnimation _enemyAnimation;
        private WaitForSeconds _timeToAttackWFS;
        private Coroutine _attack;
        private float _timeToAttack = 0.4f;
        private float _damage;

        internal override State TryGetNextState()
        {
            return _transitionAttack.GetNextState();
        }

        internal override void StartState()
        {
            if (_attack == null)
            {
                _attack = StartCoroutine(Attack());
                _transitionAttack.StartCheckTransition();
                _enemyAnimation.PlayAttack();
            }
        }

        internal override void StopState()
        {
            if (_attack != null)
            {
                StopCoroutine(_attack);
                _attack = null;
                _transitionAttack.StopCheckTransition();
                _enemyAnimation.StopPlayAttack();
            }
        }

        private void Awake()
        {
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();
            _transitionAttack = GetComponent<TransitionAttack>();
            _enemyAnimation = GetComponent<EnemyAnimation>();
            _damage = GetComponent<IEnemy>().Damage;

            _timeToAttackWFS = new WaitForSeconds(_timeToAttack);
        }

        private IEnumerator Attack()
        {
            yield return _timeToAttackWFS;

            if (_enemyNextTargetFinder.CurrentTarget != null)
            {
                if (_enemyNextTargetFinder.CurrentTarget.TryGetComponent(out IDamageable idamageable))
                {
                    idamageable.TakeDamage(_damage);
                }
            }

            yield return _timeToAttackWFS;

            _transitionAttack.SetStateIdle();
        }
    }
}