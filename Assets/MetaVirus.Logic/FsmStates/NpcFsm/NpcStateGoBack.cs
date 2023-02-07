using GameEngine.Fsm;
using GameEngine.Utils;
using MetaVirus.Logic.Data.Entities;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.FsmStates.NpcFsm
{
    public class NpcStateGoBack : NpcStateBase
    {
        private Vector3 _backPosition;
        private Vector3 _backRotation;

        public override void OnInit(FsmEntity<NpcEntity> fsm)
        {
            DoScanEnemy = false;
        }

        public override void OnEnter(FsmEntity<NpcEntity> fsm)
        {
            var info = fsm.Owner.Info;
            var nav = fsm.Owner.NavMeshAgent;
            fsm.Owner.NavMeshAgent.stoppingDistance = 0;
            fsm.Owner.NavMeshAgent.speed = fsm.Owner.RunSpeed;
            nav.SetDestination(info.Position);

            _backPosition = info.Position;
            _backRotation = GameEngineUtils.ProcessEulerAngle(info.Rotation);
        }

        public override void OnUpdate(FsmEntity<NpcEntity> fsm, float elapseTime, float realElapseTime)
        {
            base.OnUpdate(fsm, elapseTime, realElapseTime);
            var info = fsm.Owner.Info;
            var npc = fsm.Owner.NpcResObject;
            var nav = fsm.Owner.NavMeshAgent;

            fsm.Owner.Animator.SetInteger(AniParamName.State, NpcAniState.Run);

            if (nav.remainingDistance == 0)
            {
                fsm.Owner.Animator.SetInteger(AniParamName.State, NpcAniState.Walk);
                npc.transform.eulerAngles =
                    Vector3.MoveTowards(npc.transform.eulerAngles, _backRotation, Time.deltaTime * 180);

                if (GameEngineUtils.IsEulerClose(npc.transform.eulerAngles, _backRotation))
                {
                    fsm.Owner.Animator.SetInteger(AniParamName.State, NpcAniState.Idle);
                    ChangeState(fsm.Owner.GetNormalState());
                }
            }
        }

        public override void OnLeave(FsmEntity<NpcEntity> fsm, bool isShutdown)
        {
        }

        public override void OnDestroy(FsmEntity<NpcEntity> fsm)
        {
        }
    }
}