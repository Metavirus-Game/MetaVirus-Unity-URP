using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Battle.data;

namespace MetaVirus.Logic.Data.Events.Battle
{
    public class BattleOnUnitAction
    {
        public enum Action
        {
            Starting, //技能起始动作
            Moving, //技能移动动作
            Casting, //技能施放动作
            AtkKeyFrame, //技能攻击关键帧
            Hitting, //技能命中
            Backing, //回退动作
            Finished, //技能释放完毕
            Dead, //单位死亡
        }

        public BattleUnitEntity UnitEntity { get; }
        public Action ActionState { get; }
        public SkillCastInfo SkillCastInfo { get; }

        public BattleOnUnitAction(BattleUnitEntity unitEntity, Action action = Action.Casting,
            SkillCastInfo skillCastInfo = null)
        {
            UnitEntity = unitEntity;
            ActionState = action;
            SkillCastInfo = skillCastInfo;
        }
    }
}