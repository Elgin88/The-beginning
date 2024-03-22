using System.Collections;
using Assets.Scripts.Enemy;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(Rigidbody))]

    internal class StateAttack : State
    {
        [SerializeField] private EnemyNextTargetFinder _enemyNextTargetFinder;
        [SerializeField] private TransitionAttack _transitionAttack;
        [SerializeField] private EnemyAnimation _enemyAnimation;
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private EnemyMelee _enemyMelee;
        [SerializeField] private EnemyRange _enemyRange;

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
            _attack = StartCoroutine(Attack());
            _transitionAttack.StartCheckTransition();
            _enemyAnimation.StartPlayAttack();
            _rigidbody.isKinematic = false;
        }

        internal override void StopState()
        {
            StopCoroutine(_attack);
            _attack = null;
            _transitionAttack.StopCheckTransition();
            _enemyAnimation.StopPlayAttack();
            _rigidbody.isKinematic = true;
        }

        private void Start()
        {
            _damage = GetComponent<IEnemy>().Damage;

            _timeBeforeAttackWFS = new WaitForSeconds(_timeBeforeAttack);
            _timeAfterAttackWFS = new WaitForSeconds(_timeAfterAttack);
        }

        private IEnumerator Attack()
        {
            if (_enemyMelee != null)
            {
                if (_enemyNextTargetFinder.CurrentTarget != null)
                {
                    if (_enemyNextTargetFinder.CurrentTarget.TryGetComponent(out IDamageable idamageable))
                    {
                        yield return _timeBeforeAttackWFS;

                        idamageable.TakeDamage(_damage);
                    }
                }
            }
            else if (_enemyRange != null)
            {
                yield return _timeBeforeAttackWFS;

                if (_enemyNextTargetFinder.CurrentTarget != null)
                {
                    _enemyRange.EnableArrow(_enemyNextTargetFinder.CurrentTargetPosition);
                }
            }

            yield return _timeAfterAttackWFS;

            _transitionAttack.SetStateIdle();
        }
    }
}