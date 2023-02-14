using cfg.attr;
using cfg.battle;
using cfg.common;
using GameEngine;
using MetaVirus.Logic.AttrsCalculator;
using MetaVirus.Logic.Service;
using UnityEngine;

namespace MetaVirus.Logic.Data.Provider
{
    public interface IMonsterDataProvider : IAttrsProvider
    {
        public int Id { get; }
        public string Name { get; }
        public MonsterType Type { get; }
        public AttrGrowTable GrowTable { get; }
        public int ModelResId { get; }
        public Quality Quality { get; }
        public int Level { get; }
        public LevelUpTable LevelUpTable { get; }
        public int CurrExp { get; }
        public int ExpToNextLevel { get; }

        /**
         * 返回基本属性的成长率
         */
        public float GetBaseAttributeGrow(AttributeId attr);

        /// <summary>
        /// 返回基本属性和计算属性，AttributeId.AttrCri之前(包含)为基本属性，之后为计算属性
        /// </summary>
        /// <param name="attr"></param>
        /// <returns></returns>
        public int GetAttribute(AttributeId attr);

        public CharacterData Character { get; }
        public string QualityStr => Constants.QualityToStr(Quality);

        public Color QualityClr => GameFramework.GetService<GameDataService>().QualityToColor(Quality);
    }
}