using cfg.common;

namespace MetaVirus.Logic.AttrsCalculator
{
    public interface IAttrsCalculator
    {
        public int this[AttributeId attrId] => GetAttribute(attrId);

        public int GetAttribute(AttributeId attrId)
        {
            return attrId <= AttributeId.AttrCri ? GetBaseAttribute(attrId) : GetCalcAttribute(attrId);
        }

        /// <summary>
        /// 返回属性对应的值 AttributeId.AttrHp ~ AttributeId.AttrCri
        /// </summary>
        /// <param name="attrId"></param>
        /// <returns></returns>
        public int GetBaseAttribute(AttributeId attrId);

        public int GetCalcAttribute(AttributeId attrId);

        public int GetResistance(ResistanceId resId);
    }
}