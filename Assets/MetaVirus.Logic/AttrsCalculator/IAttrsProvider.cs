using cfg.attr;
using cfg.common;

namespace MetaVirus.Logic.AttrsCalculator
{
    public interface IAttrsProvider
    {
        /// <summary>
        /// 返回12个基本属性 AttrHp ~ AttrCri
        /// 返回值为计算成长后的结果
        /// </summary>
        /// <param name="attrId"></param>
        /// <returns></returns>
        public int GetBaseAttribute(AttributeId attrId);

        /// <summary>
        /// 返回抗性最终值
        /// </summary>
        /// <param name="resId"></param>
        /// <returns></returns>
        public int GetResistance(ResistanceId resId);
    }
}