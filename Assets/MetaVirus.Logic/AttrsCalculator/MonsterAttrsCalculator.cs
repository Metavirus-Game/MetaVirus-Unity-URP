using cfg.common;
using MetaVirus.Logic.Data.Player;

namespace MetaVirus.Logic.AttrsCalculator
{
    public class MonsterAttrsCalculator : IAttrsCalculator
    {
        private readonly IAttrsProvider _attrsProvider;

        public MonsterAttrsCalculator(IAttrsProvider provider)
        {
            _attrsProvider = provider;
        }

        public int GetBaseAttribute(AttributeId attrId)
        {
            return _attrsProvider.GetBaseAttribute(attrId);
        }


        public int GetResistance(ResistanceId resId)
        {
            return _attrsProvider.GetResistance(resId);
        }

        public int GetCalcAttribute(AttributeId attrId)
        {
            return attrId switch
            {
                AttributeId.CalcHpMax => CalcHpMax(),
                AttributeId.CalcMpMax => CalcMpMax(),
                AttributeId.CalcAtk => CalcAtk(),
                AttributeId.CalcMAtk => CalcMAtk(),
                AttributeId.CalcDef => CalcDef(),
                AttributeId.CalcMDef => CalcMDef(),
                _ => 0
            };
        }

        //物攻=力量
        private int CalcAtk()
        {
            return GetBaseAttribute(AttributeId.AttrStr);
        }

        //魔攻=智力
        private int CalcMAtk()
        {
            return GetBaseAttribute(AttributeId.AttrInt);
        }

        //物防=体质
        private int CalcDef()
        {
            return GetBaseAttribute(AttributeId.AttrPhy);
        }

        //魔防=精神
        private int CalcMDef()
        {
            return GetBaseAttribute(AttributeId.AttrSpr);
        }

        //HpMax = hp * 10
        private int CalcHpMax()
        {
            var attrHp = GetBaseAttribute(AttributeId.AttrHp);
            return attrHp * 10;
        }

        //MpMax = mp * 10
        private int CalcMpMax()
        {
            var attrMp = GetBaseAttribute(AttributeId.AttrMp);
            return attrMp * 10;
        }
    }
}