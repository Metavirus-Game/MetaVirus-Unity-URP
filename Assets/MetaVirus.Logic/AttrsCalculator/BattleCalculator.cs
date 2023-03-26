using cfg.common;
using cfg.skill;
using MetaVirus.Logic.Data.Provider;

namespace MetaVirus.Logic.AttrsCalculator
{
    /// <summary>
    /// 战斗相关计算<br/>
    /// 只为显示用的计算器，需和服务器算法同步
    /// </summary>
    public class BattleCalculator
    {
        /// <summary>
        /// 使用skillOwner的属性，计算技能伤害
        /// </summary>
        /// <param name="atkAttribute"></param>
        /// <param name="atkValue"></param>
        /// <param name="skillOwner"></param>
        /// <returns></returns>
        public static int CalcSkillDamage(AtkAttribute atkAttribute, AtkValue atkValue, IMonsterDataProvider skillOwner)
        {
            var calcAttr = atkAttribute == AtkAttribute.Physical
                ? AttributeId.CalcAtk
                : AttributeId.CalcMAtk;

            return CalcDamage(calcAttr, atkValue, skillOwner);
        }

        public static int CalcDamage(AttributeId calcAttr, AtkValue atkValue, IMonsterDataProvider skillOwner)
        {
            if (atkValue.CalcType == AtkCalcType.Value)
            {
                return (int)atkValue.Value;
            }

            var atk = skillOwner.GetAttribute(calcAttr);

            return atkValue.CalcType switch
            {
                AtkCalcType.Additional => atk + (int)atkValue.Value,
                AtkCalcType.Percent => (int)(atk * atkValue.Value),
                _ => 0
            };
        }
    }
}