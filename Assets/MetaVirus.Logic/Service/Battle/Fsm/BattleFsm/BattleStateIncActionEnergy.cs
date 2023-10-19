using GameEngine.Fsm;

namespace MetaVirus.Logic.Service.Battle.Fsm.BattleFsm
{
    /// <summary>
    /// 各自增长行动能量，还没有可行动的单位
    /// </summary>
    public class BattleStateIncActionEnergy : FsmState<BaseBattleInstance>
    {
        public override void OnUpdate(FsmEntity<BaseBattleInstance> fsm, float elapseTime, float realElapseTime)
        {
            var bi = fsm.Owner;
            if (bi.CurrentActionFrame() != null)
            {
                //进行行动
                ChangeState<BattleStateAction>(fsm);
                return;
            }

            bi.IncTimeline(elapseTime);
            if (bi.CurrentActionFrame() != null)
            {
                //进行行动
                ChangeState<BattleStateAction>(fsm);
            }
            else if (bi.BattleCompleted)
            {
                //战斗完毕
                ChangeState<BattleStateCompleted>(fsm);
            }
        }
    }
}