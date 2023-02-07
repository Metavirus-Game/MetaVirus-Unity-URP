using GameEngine.Fsm;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;

namespace MetaVirus.Logic.FsmStates.NpcFsm
{
    public class NpcStateIdle : NpcStateBase
    {
        public override void OnInit(FsmEntity<NpcEntity> fsm)
        {
            var player = PlayerEntity.Current;
            DoCheckInteractive = player != null &&
                                 Constants.GetNpcRelationWithPlayer(player, fsm.Owner) ==
                                 Constants.NpcRelation.Friendly;
        }

        public override void OnEnter(FsmEntity<NpcEntity> fsm)
        {
        }

        public override void OnLeave(FsmEntity<NpcEntity> fsm, bool isShutdown)
        {
        }

        public override void OnDestroy(FsmEntity<NpcEntity> fsm)
        {
        }
    }
}