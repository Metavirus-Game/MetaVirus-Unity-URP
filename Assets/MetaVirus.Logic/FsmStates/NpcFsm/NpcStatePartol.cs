using GameEngine;
using GameEngine.Fsm;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.FsmStates.NpcFsm.PartolFsm;
using UnityEngine;

namespace MetaVirus.Logic.FsmStates.NpcFsm
{
    public class NpcStatePartol : NpcStateBase
    {
        public Vector3 OriginPos { get; private set; }

        public Vector3 ToPos { get; private set; }

        public Vector3 OriginRot { get; private set; }

        public NpcEntity NpcEntity => Fsm.Owner;

        private FsmEntity<NpcStatePartol> _partolFsm;

        public override void OnInit(FsmEntity<NpcEntity> fsm)
        {
            DoScanEnemy = true;
            var player = PlayerEntity.Current;
            DoCheckInteractive = player != null &&
                                 Constants.GetNpcRelationWithPlayer(player, fsm.Owner) ==
                                 Constants.NpcRelation.Friendly;
            AutoTurnBack = false;
        }

        public override void OnEnter(FsmEntity<NpcEntity> fsm)
        {
            var info = fsm.Owner.Info;
            OriginPos = info.Position;
            OriginRot = GameEngineUtils.ProcessEulerAngle(info.Rotation);
            fsm.Owner.NavMeshAgent.speed = fsm.Owner.WalkSpeed;
            fsm.Owner.NavMeshAgent.stoppingDistance = 0;
            ToPos = info.BehaviourParam;

            _partolFsm = GameFramework.GetService<FsmService>().CreateFsm("PartolFsm-" + fsm.Owner.Id, this,
                new PartolStateBack(),
                new PartolStateIdle(),
                new PartolStateWalk()
            );

            _partolFsm.Start<PartolStateIdle>();
        }

        public override void OnNavMeshAgentInterrupted()
        {
            _partolFsm.Pause();
        }

        public override void OnNavMeshAgentResume()
        {
            _partolFsm.Resume();
        }

        public void WalkToTarget()
        {
            Fsm.Owner.NavMeshAgent.SetDestination(ToPos);
        }

        public void BackToOrigin()
        {
            Fsm.Owner.NavMeshAgent.SetDestination(OriginPos);
        }

        public override void OnLeave(FsmEntity<NpcEntity> fsm, bool isShutdown)
        {
            GameFramework.GetService<FsmService>().DestroyFsm<NpcStatePartol>(_partolFsm.Name);
        }

        public override void OnDestroy(FsmEntity<NpcEntity> fsm)
        {
        }
    }
}