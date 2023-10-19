using GameEngine.Fsm;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Player;
using UnityEngine;

namespace MetaVirus.Logic.FsmStates.NetPlayerFsm
{
    public class NetPlayerStateIdle : FsmState<NetPlayerController>
    {
        public override void OnEnter(FsmEntity<NetPlayerController> fsm)
        {
            fsm.Owner.CurrentSpeed = 0;
            if (fsm.Owner.Animator != null)
            {
                //fsm.Owner.Animator.SetFloat(Constants.AniParamName.Velocity, 0);
                fsm.Owner.Animator.SetInteger(Constants.AniParamName.State, Constants.NpcAniState.Idle);
                fsm.Owner.Animator.speed = 1;
                fsm.Owner.Animator.transform.localPosition = Vector3.zero;
            }
        }

        public override void OnUpdate(FsmEntity<NetPlayerController> fsm, float elapseTime, float realElapseTime)
        {
            if (fsm.Owner.HasWayPoint)
            {
                ChangeState<NetPlayerStateMoving>(fsm);
            }
        }
    }
}