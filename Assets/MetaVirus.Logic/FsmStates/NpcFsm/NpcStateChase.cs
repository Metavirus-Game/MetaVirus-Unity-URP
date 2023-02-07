using GameEngine.Fsm;
using MetaVirus.Logic.Data.Entities;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.FsmStates.NpcFsm
{
    public class NpcStateChase : NpcStateBase
    {
        public override void OnInit(FsmEntity<NpcEntity> fsm)
        {
            DoScanEnemy = false;
        }

        public override void OnEnter(FsmEntity<NpcEntity> fsm)
        {
            fsm.Owner.NavMeshAgent.stoppingDistance = EnterBattleDist;
            fsm.Owner.NavMeshAgent.speed = fsm.Owner.RunSpeed;
        }

        public override void OnLeave(FsmEntity<NpcEntity> fsm, bool isShutdown)
        {
            fsm.Owner.Animator.SetInteger(AniParamName.State, NpcAniState.Idle);
        }

        public override void OnDestroy(FsmEntity<NpcEntity> fsm)
        {
        }

        public override void OnUpdate(FsmEntity<NpcEntity> fsm, float elapseTime, float realElapseTime)
        {
            base.OnUpdate(fsm, elapseTime, realElapseTime);
            var target = fsm.Owner.ChaseTarget;
            var info = fsm.Owner.Info;
            var player = PlayerEntity.Current;

            if (target == null || player.AvoidBattle)
            {
                ChangeState<NpcStateGoBack>(fsm);
                return;
            }

            var position = target.transform.position;

            fsm.Owner.NavMeshAgent.SetDestination(position);
            var dist = Vector3.Distance(position, info.Position);

            if (dist > info.ScanRadius * 2)
            {
                //超出追击半径了，归位
                ChangeState<NpcStateGoBack>(fsm);
                return;
            }

            var ani = fsm.Owner.Animator;

            dist = Vector3.Distance(position, fsm.Owner.NpcResObject.transform.position);

            if (dist > EnterBattleDist)
            {
                ani.SetInteger(AniParamName.State, NpcAniState.Run);
            }
            else
            {
                ani.SetInteger(AniParamName.State, NpcAniState.Idle);
            }
        }
    }
}