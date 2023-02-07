using GameEngine;
using GameEngine.Fsm;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.FsmStates.NpcFsm.RandomFsm;
using UnityEngine;

namespace MetaVirus.Logic.FsmStates.NpcFsm
{
    public class NpcStateRandomWalk : NpcStateBase
    {
        public NpcEntity NpcEntity => Fsm.Owner;

        private FsmEntity<NpcStateRandomWalk> _rndWalkFsmEntity;

        public override void OnInit(FsmEntity<NpcEntity> fsm)
        {
            var player = PlayerEntity.Current;
            DoCheckInteractive = player != null &&
                                 Constants.GetNpcRelationWithPlayer(player, fsm.Owner) ==
                                 Constants.NpcRelation.Friendly;

            AutoTurnBack = false;
        }

        public override void OnEnter(FsmEntity<NpcEntity> fsm)
        {
            _rndWalkFsmEntity = GameFramework.GetService<FsmService>().CreateFsm("random-walk-fsm-" + NpcEntity.Id
                , this, new RandomStateIdle(), new RandomStateWalk());
            _rndWalkFsmEntity.Start<RandomStateIdle>();
        }

        public Vector3 RandomToPosition()
        {
            var info = Fsm.Owner.Info;
            var radius = info.BehaviourParam.x;

            var toPos = Fsm.Owner.MapNpc.Position;
            toPos.x += Random.Range(-radius, radius);
            toPos.z += Random.Range(-radius, radius);

            return toPos;
        }

        public override void OnLeave(FsmEntity<NpcEntity> fsm, bool isShutdown)
        {
            GameFramework.GetService<FsmService>().DestroyFsm<NpcStateRandomWalk>(_rndWalkFsmEntity.Name);
        }

        public override void OnDestroy(FsmEntity<NpcEntity> fsm)
        {
        }
    }
}