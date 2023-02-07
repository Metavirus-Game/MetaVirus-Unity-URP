using GameEngine.Fsm;
using GameEngine.Utils;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.FsmStates.NpcFsm.PartolFsm
{
    public class PartolStateBack : FsmState<NpcStatePartol>
    {
        public override void OnInit(FsmEntity<NpcStatePartol> fsm)
        {
        }

        public override void OnEnter(FsmEntity<NpcStatePartol> fsm)
        {
            fsm.Owner.BackToOrigin();
            fsm.Owner.NpcEntity.Animator.SetInteger(AniParamName.State, NpcAniState.Walk);
        }

        public override void OnPause(FsmEntity<NpcStatePartol> fsm)
        {
            fsm.Owner.NpcEntity.Animator.SetInteger(AniParamName.State, NpcAniState.Idle);
        }

        public override void OnResume(FsmEntity<NpcStatePartol> fsm)
        {
            fsm.Owner.NpcEntity.Animator.SetInteger(AniParamName.State, NpcAniState.Walk);
        }

        public override void OnUpdate(FsmEntity<NpcStatePartol> fsm, float elapseTime, float realElapseTime)
        {
            var npc = fsm.Owner.NpcEntity;
            var npcRot = npc.NpcResObject.transform.eulerAngles;

            if (npc.NavMeshAgent.remainingDistance == 0)
            {
                npc.NpcResObject.transform.eulerAngles = Vector3.MoveTowards(npcRot, fsm.Owner.OriginRot,
                    Time.deltaTime * 180);

                if (GameEngineUtils.IsEulerClose(npcRot, fsm.Owner.OriginRot))
                {
                    ChangeState<PartolStateIdle>(fsm);
                }
            }
        }
    }
}