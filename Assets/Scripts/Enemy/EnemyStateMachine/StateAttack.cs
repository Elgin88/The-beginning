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
        private WaitForSeconds _timeBeforeAttackWFS;
        private WaitForSeconds _timeAfterAttackWFS;
        private Coroutine _attack;
        private float _timeBeforeAttack = 0.45f;
        private float _timeAfterAttack = 0.5f;
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
                _enemyAnimation.StartPlayAttack();
            }
        }

        internal override void StopState()
        {
            StopCoroutine(_attack);
            _attack = null;
            _transitionAttack.StopCheckTransition();
            _enemyAnimation.StopPlayAttack();
        }

        private void Awake()
        {
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();
            _transitionAttack = GetComponent<TransitionAttack>();
            _enemyAnimation = GetComponent<EnemyAnimation>();
        }

        private void Start()
        {
            _damage = GetComponent<IEnemy>().Damage;

            _timeBeforeAttackWFS = new WaitForSeconds(_timeBeforeAttack);
            _timeAfterAttackWFS = new WaitForSeconds(_timeAfterAttack);
        }

        private IEnumerator Attack()
        {
            if (_enemyNextTargetFinder.CurrentTarget != null)
            {
                if (_enemyNextTargetFinder.CurrentTarget.TryGetComponent(out IDamageable idamageable))
                {
                    yield return _timeBeforeAttackWFS;

                    idamageable.TakeDamage(_damage);

                    Debug.Log(GetComponent<IEnemy>().Damage);
                }
            }

            yield return _timeAfterAttackWFS;

            _transitionAttack.SetStateIdle();
        }
    }
}