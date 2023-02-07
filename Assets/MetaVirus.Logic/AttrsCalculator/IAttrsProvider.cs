using cfg.attr;
using cfg.common;

namespace MetaVirus.Logic.AttrsCalculator
{
    public interface IAttrsProvider
    {
        /// <summary>
        /// 返回12个基本属性 AttrHp ~ AttrCri
        /// </summary>
        /// <param name="attrId"></param>
        /// <returns></returns>
        public int GetBaseAttribute(AttributeId attrId);

        public int GetResistance(ResistanceId resId);
    }
}