using GameEngine.Fsm;
using MetaVirus.Logic.Player;
using MetaVirus.Logic.Service;

namespace MetaVirus.Logic.FsmStates.NetPlayerFsm
{
    /// <summary>
    /// 网络玩家waypoint走完后，等待一个reportInterval的间隔
    /// 防止连续在行走和idle动作之间切换
    /// </summary>
    public class NetPlayerStateWaiting : FsmState<NetPlayerController>
    {
        private float _countDown = 0;

        public override void OnEnter(FsmEntity<NetPlayerController> fsm)
        {
            _countDown = PositionService.ReportInterval;
        }

        public override void OnUpdate(FsmEntity<NetPlayerController> fsm, float elapseTime, float realElapseTime)
        {
            if (_countDown > 0)
            {
                _countDown -= elapseTime;
            }

            if (fsm.Owner.HasWayPoint)
            {
                ChangeState<NetPlayerStateMoving>(fsm);
            }
            else if (_countDown <= 0)
            {
                ChangeState<NetPlayerStateIdle>(fsm);
            }
        }
    }
}