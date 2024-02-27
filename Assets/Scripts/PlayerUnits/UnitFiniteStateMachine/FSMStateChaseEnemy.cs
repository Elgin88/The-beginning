using UnityEngine;
using UnityEngine.AI;

namespace Assets.Scripts.PlayerUnits.UnitFiniteStateMachine
{
    internal class FSMStateChaseEnemy : FSMState
    {
        private float _distance;

        public FSMStateChaseEnemy(FiniteStateMachine fsm, Unit unit, NavMeshAgent navMesh, UnitAnimator animator) : base(fsm, unit, navMesh, animator)
        {
            UnitNavMesh.stoppingDistance = Unit.AttackRange;
        }

        public override void Enter()
        {
            UnitNavMesh.SetDestination(FSM.Target.Transform.position);

            Animator.SetMovingBool(true);
        }

        public override void Exit()
        {
            UnitNavMesh.ResetPath();
            Animator.SetMovingBool(false);
        }

        public override void Update()
        {
            _distance = Vector3.Distance(Unit.transform.position, FSM.Target.Transform.position);

            if (_distance <= UnitNavMesh.stoppingDistance)
            {
                FSM.SetState<FSMStateAttack>();
            }
        }
    }
}
