using GameEngine.Fsm;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Player;
using UnityEngine;

namespace MetaVirus.Logic.FsmStates.NetPlayerFsm
{
    public class NetPlayerStateMoving : FsmState<NetPlayerController>
    {
        public override void OnEnter(FsmEntity<NetPlayerController> fsm)
        {
            fsm.Owner.CurrentSpeed = 1;
            if (fsm.Owner.Animator != null)
            {
                fsm.Owner.Animator.SetInteger(Constants.AniParamName.State, Constants.NpcAniState.Walk);
                fsm.Owner.Animator.speed = Constants.PlayerWalkAniSpeed;
                fsm.Owner.Animator.transform.localPosition = Vector3.zero;
            }
        }

        public override void OnUpdate(FsmEntity<NetPlayerController> fsm, float elapseTime, float realElapseTime)
        {
            var ctl = fsm.Owner;
            if (ctl.HasWayPoint)
            {
                var dest = ctl.WayPoints[0];
                var position = ctl.transform.position;
                if (position == dest.Position)
                {
                    ctl.WayPoints.RemoveAt(0);
                    if (ctl.WayPoints.Count == 0)
                    {
                        ctl.SetRotation(dest.Rotation);
                        return;
                    }

                    dest = ctl.WayPoints[0];
                }

                var nextPosition = Vector3.MoveTowards(position, dest.Position,
                    ctl.moveSpeed * elapseTime);
                var dir = nextPosition - position;
                ctl.transform.position = nextPosition;
                dir.y = 0;
                ctl.ControlObject.transform.forward = dir;
            }
            else
            {
                ChangeState<NetPlayerStateWaiting>(fsm);
            }
        }
    }
}