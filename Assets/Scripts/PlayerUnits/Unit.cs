using UnityEngine;
using Assets.Scripts.GameLogic.Damageable;
using Assets.Scripts.PlayerUnits.UnitFiniteStateMachine;
using UnityEngine.AI;

namespace Assets.Scripts.PlayerUnits
{
    [RequireComponent(typeof(UnitAnimator))]
    [RequireComponent(typeof(NavMeshAgent))]
    internal class Unit : Selectable, IDamageable
    {
        private UnitData _unitData;
        private float _health;
        private bool _isDead;

        private FiniteStateMachine _fsm;
        private UnitAnimator _animator;
        private NavMeshAgent _agent;

        public bool IsDead => _isDead;

        public bool IsPlayerObject => true;

        public Transform Transform => transform;

        private void Start()
        {
            _animator = GetComponent<UnitAnimator>();
            _agent = GetComponent<NavMeshAgent>();

            _fsm = new FiniteStateMachine(_animator, _agent, this, _unitData);

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

        public void Init(UnitData data)
        {
            _unitData = data;
            _health = data.Health;
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