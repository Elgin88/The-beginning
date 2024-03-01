using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.PlayerUnits.UnitFiniteStateMachine;
using UnityEngine.AI;

namespace Assets.Scripts.PlayerUnits
{
    [RequireComponent(typeof(UnitAnimator))]
    [RequireComponent(typeof(NavMeshAgent))]
    internal abstract class Unit : Selectable, IDamageable
    {
        private float _health;
        private bool _isDead;
        private float _attackRange;
        private float _aggroRange;
        private LayerMask _enemyMask;

        private FiniteStateMachine _fsm;
        private UnitAnimator _animator;
        private NavMeshAgent _agent;

        public float AttackRange => 2;
        public float AggroRange => 5;
        public LayerMask EnemyMask => _enemyMask;

        public float Damage => 5;
        public float AttackSpeed => 2;

        public bool IsDead => _isDead;

        public bool IsPlayerObject => true;

        public Transform Transform => transform;

        private void Start()
        {
            _animator = GetComponent<UnitAnimator>();
            _agent = GetComponent<NavMeshAgent>();

            _fsm = new FiniteStateMachine(_animator, _agent, this);

            _fsm.SetState<FSMStateIdle>();
        }

        public void Update() 
        {
            _fsm.Update();
        }

        public void TakeDamage(float damage)
        {
            _health -= damage;

            if (_health <= 0)
                Die();
        }

        public void InitUnit(float health, LayerMask layer)
        {
            _health = health;
            _enemyMask = layer;
        }

        public void Move(Vector3 position)
        {
            _fsm.SetMovePosition(position);
            _fsm.SetState<FSMStateMove>();
        }

        private void Die()
        {
            gameObject.SetActive(false);
        }
    }
}