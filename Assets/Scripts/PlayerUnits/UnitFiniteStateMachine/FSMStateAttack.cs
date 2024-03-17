using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.PlayerUnits.UnitFiniteStateMachine
{
    internal class FSMStateAttack : FSMState
    {
        private float _distance;
        private float _timePast;

        public FSMStateAttack(FiniteStateMachine fsm, Unit unit, NavMeshAgent navMesh, UnitAnimator animator, UnitData data)
            : base(fsm, unit, navMesh, animator, data)
        {
        }

        public override void Update()
        {
            if (NeedChaseEnemy())
                FSM.SetState<FSMStateChaseEnemy>();

            if (FSM.Target.Transform.gameObject.activeSelf)
            {
                Attack();
            }
            else
            {
                FSM.SetState<FSMStateIdle>();
            }
        }

        private bool NeedChaseEnemy()
        {
            _distance = Vector3.Distance(Unit.transform.position, FSM.Target.Transform.position);

            if (_distance > Data.AttackRange)
                return true;

            return false;
        }

        private void Attack()
        {
            _timePast += Time.deltaTime;

            if (_timePast >= Data.AttackSpeed)
            {
                FSM.Target.TakeDamage(Data.Damage);
                Animator.SetTriggerAttack();
                _timePast = 0;
            }
        }
    }
}
