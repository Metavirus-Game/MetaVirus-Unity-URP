using GameEngine.Fsm;
using UnityEngine;
using UnityEngine.AI;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.FsmStates.NpcFsm.RandomFsm
{
    public class RandomStateWalk : FsmState<NpcStateRandomWalk>
    {
        private Vector3 _toPos;

        public override void OnEnter(FsmEntity<NpcStateRandomWalk> fsm)
        {
            _toPos = fsm.Owner.RandomToPosition();
            var path = new NavMeshPath();
            fsm.Owner.NpcEntity.NavMeshAgent.CalculatePath(_toPos, path);
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                fsm.Owner.NpcEntity.NavMeshAgent.SetDestination(_toPos);
                fsm.Owner.NpcEntity.Animator.SetInteger(AniParamName.State, NpcAniState.Walk);
            }
            //if (fsm.Owner.NpcEntity.NavMeshAgent.pathStatus != NavMeshPathStatus.PathComplete)
            else
            {
                //此路不通
                //fsm.Owner.NpcEntity.NavMeshAgent.SetDestination(fsm.Owner.NpcEntity.NpcResObject.transform.position);
                ChangeState<RandomStateIdle>(fsm);
            }
        }

        public override void OnPause(FsmEntity<NpcStateRandomWalk> fsm)
        {
            fsm.Owner.NpcEntity.Animator.SetInteger(AniParamName.State, NpcAniState.Idle);
        }

        public override void OnResume(FsmEntity<NpcStateRandomWalk> fsm)
        {
            fsm.Owner.NpcEntity.Animator.SetInteger(AniParamName.State, NpcAniState.Walk);
        }

        public override void OnUpdate(FsmEntity<NpcStateRandomWalk> fsm, float elapseTime, float realElapseTime)
        {
            if (fsm.Owner.NpcEntity.NavMeshAgent.remainingDistance == 0)
            {
                ChangeState<RandomStateIdle>(fsm);
            }
        }
    }
}