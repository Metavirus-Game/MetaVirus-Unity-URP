using GameEngine.Fsm;
using MetaVirus.Logic.Data.Entities;

namespace MetaVirus.Logic.FsmStates.NpcFsm
{
    public class NpcStateInteractive : NpcStateBase
    {
        public NpcEntity NpcEntity => Fsm.Owner;

        public override void OnInit(FsmEntity<NpcEntity> fsm)
        {
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