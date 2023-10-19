using GameEngine.Fsm;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.FsmStates.NpcFsm.RandomFsm
{
    public class RandomStateIdle : FsmState<NpcStateRandomWalk>
    {
        private float _rndTime = 0;

        public override void OnEnter(FsmEntity<NpcStateRandomWalk> fsm)
        {
            _rndTime = Random.Range(3, 10);
            fsm.Owner.NpcEntity.Animator.SetInteger(AniParamName.State, NpcAniState.Idle);
        }

        public override void OnUpdate(FsmEntity<NpcStateRandomWalk> fsm, float elapseTime, float realElapseTime)
        {
            _rndTime -= Time.deltaTime;
            if (_rndTime > 0) return;

            ChangeState<RandomStateWalk>(fsm);
        }
    }
}