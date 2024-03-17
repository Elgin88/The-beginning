using UnityEngine.AI;

namespace Assets.Scripts.PlayerUnits.UnitFiniteStateMachine
{
    internal class FSMStateChaseEnemy : FSMState
    {
        public FSMStateChaseEnemy(FiniteStateMachine fsm, Unit unit, NavMeshAgent navMesh, UnitAnimator animator, UnitData data)
            : base(fsm, unit, navMesh, animator, data)
        {
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
            if (UnitNavMesh.remainingDistance <= Data.AttackRange)
            {
                FSM.SetState<FSMStateAttack>();
            }
        }
    }
}
