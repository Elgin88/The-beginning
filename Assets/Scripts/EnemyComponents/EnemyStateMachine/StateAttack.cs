using System.Collections;
using Assets.Scripts.Enemy;
using Assets.Scripts.GameLogic.Damageable;
using UnityEngine;

namespace Assets.Scripts.UnitStateMachine
{
    [RequireComponent(typeof(Animator))]

    internal class StateAttack : State
    {
        private EnemyNextTargetFinder _enemyNextTargetFinder;
        private TransitionAttack _transitionAttack;
        private EnemyAnimation _enemyAnimation;
        private WaitForSeconds _intervalBetweenAttacksWFS;
        private Coroutine _attack;
        private float _intervalBetweenAttacks = 0.5f;
        private float _damage;

        protected override bool IsNeedNextState { get; set; }

        internal override State GetNextState()
        {
            return _transitionAttack.GetNextState();
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

        internal override bool GetIsNeedNextState()
        {
            return IsNeedNextState;
        }

        private void Awake()
        {
            _enemyNextTargetFinder = GetComponent<EnemyNextTargetFinder>();
            _enemyAnimation = GetComponent<EnemyAnimation>();

            _intervalBetweenAttacksWFS = new WaitForSeconds(_intervalBetweenAttacks);
            _damage = GetComponent<IEnemy>().Damage;
        }

        private IEnumerator Attack()
        {
            while (true)
            {
                _enemyAnimation.PlayAttack();

                yield return _intervalBetweenAttacksWFS;

                _enemyNextTargetFinder.CurrentTarget.GetComponent<IDamageable>().TakeDamage(_damage);

                Debug.Log(_enemyNextTargetFinder.CurrentTarget.GetComponent<IDamageable>().IsDead);

                _enemyAnimation.StopPlayAttack();

                yield return _intervalBetweenAttacksWFS;
            }
        }
    }
}