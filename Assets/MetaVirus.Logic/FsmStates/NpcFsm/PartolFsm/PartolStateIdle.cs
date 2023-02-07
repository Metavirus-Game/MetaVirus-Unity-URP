using GameEngine.Fsm;
using GameEngine.Utils;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.FsmStates.NpcFsm.PartolFsm
{
    public class PartolStateIdle : FsmState<NpcStatePartol>
    {
        private float _rndTime = 0;

        public override void OnInit(FsmEntity<NpcStatePartol> fsm)
        {
        }

        public override void OnEnter(FsmEntity<NpcStatePartol> fsm)
        {
            _rndTime = Random.Range(3, 10);
            fsm.Owner.NpcEntity.Animator.SetInteger(AniParamName.State, NpcAniState.Idle);
        }

        public override void OnUpdate(FsmEntity<NpcStatePartol> fsm, float elapseTime, float realElapseTime)
        {
            _rndTime -= Time.deltaTime;
            if (_rndTime > 0) return;

            var npc = fsm.Owner.NpcEntity;
            var npcPos = npc.NpcResObject.transform.position;

            if (GameEngineUtils.IsVectorClose(npcPos, fsm.Owner.OriginPos))
            {
                ChangeState<PartolStateWalk>(fsm);
            }
            else
            {
                ChangeState<PartolStateBack>(fsm);
            }
        }
    }
}