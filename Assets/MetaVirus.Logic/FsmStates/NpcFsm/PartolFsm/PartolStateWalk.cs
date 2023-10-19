using GameEngine.Fsm;
using GameEngine.Utils;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.FsmStates.NpcFsm.PartolFsm
{
    public class PartolStateWalk : FsmState<NpcStatePartol>
    {
        private Vector3 _npcRot;

        private Vector3 _npcDir;

        public override void OnInit(FsmEntity<NpcStatePartol> fsm)
        {
        }

        public override void OnEnter(FsmEntity<NpcStatePartol> fsm)
        {
            _npcRot = new Vector3(0, Random.Range(0, 360), 0);
            _npcDir = (Quaternion.Euler(_npcRot) * Vector3.forward).normalized;
            fsm.Owner.WalkToTarget();
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
            if (npc.NavMeshAgent.remainingDistance == 0)
            {
                if (npc.NavMeshAgent.remainingDistance == 0)
                {
                    npc.NpcResObject.transform.forward = Vector3.MoveTowards(
                        npc.NpcResObject.transform.forward, _npcDir,
                        1 * elapseTime);

                    // npc.NpcResObject.transform.eulerAngles = Vector3.MoveTowards(npc.NpcResObject.transform.eulerAngles,
                    //     _npcRot, Time.deltaTime * 180);

                    if (GameEngineUtils.IsDirectionClose(_npcDir, npc.NpcResObject.transform.forward))
                    {
                        ChangeState<PartolStateIdle>(fsm);
                    }
                }
            }
        }
    }
}