using System.Linq;
using cfg.attr;
using cfg.battle;
using cfg.common;
using GameEngine;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Net.Messages.Common;
using Unity.VisualScripting;

namespace MetaVirus.Logic.Service.Arena.data
{
    /// <summary>
    /// 竞技场玩家阵型 slot数据
    /// </summary>
    public class ArenaFormationDetailSlot : IMonsterDataProvider
    {
        public int SlotId { get; private set; }

        /// <summary>
        /// 数据类型，0=玩家PetData，1=怪物MonsterData
        /// </summary>
        public int DataType { get; private set; }

        /// <summary>
        /// 数据id，不同类型对应不同数据
        /// </summary>
        public int Id { get; private set; }

        public string Name { get; private set; }

        public MonsterType Type { get; private set; }

        /// <summary>
        /// 资源(NpcResourceData)id
        /// </summary>
        public int ModelResId { get; private set; }

        public int Level { get; private set; }

        public Quality Quality { get; private set; }

        public int CharacterId { get; private set; }
        public int GrowTableId { get; private set; }
        public int LevelUpTableId { get; private set; }

        public int[] Attributes { get; private set; }
        public int[] Resistances { get; private set; }

        public LevelUpTable LevelUpTable { get; private set; }
        public int CurrExp => 0;
        public int ExpToNextLevel => 0;

        public CharacterData Character { get; private set; }
        public AttrGrowTable GrowTable { get; private set; }

        public float GetBaseAttributeGrow(AttributeId attr)
        {
            var attrIdx = (int)attr - 1;
            var growAttrs = GrowTable.Attributes[1];
            return growAttrs.Attrs[attrIdx];
        }

        public int GetAttribute(AttributeId attr)
        {
            var idx = (int)attr;
            if (idx < 0 || idx > Attributes.Length)
            {
                return 0;
            }

            return Attributes[idx];
        }

        public int GetBaseAttribute(AttributeId attrId)
        {
            var idx = (int)attrId;
            if (idx < 0 || idx > Attributes.Length)
            {
                return 0;
            }

            return Attributes[idx];
        }

        public int GetResistance(ResistanceId resId)
        {
            var idx = (int)resId;
            if (idx < 0 || idx > Resistances.Length)
            {
                return 0;
            }

            return Resistances[idx];
        }

        public static ArenaFormationDetailSlot FromProtoBuf(PBBattleFormationDetailSlot slot)
        {
            var gameDataService = GameFramework.GetService<GameDataService>();

            var levelUpTable = gameDataService.gameTable.LevelUpTables.Get(slot.LevelUpTableId);
            var growTable = gameDataService.gameTable.AttrGrowTables.Get(slot.GrowTableId);
            var character = gameDataService.gameTable.CharacterDatas.Get(slot.CharacterId);

            //注意此处服务器返回的是数据id，不是资源id，需要根据ItemType区分是monsterData还是petData，再从数据中拿取npcResourceData
            var dataId = slot.ItemResId;
            var resId = 0;
            MonsterType type = null;
            if (slot.ItemType == 0)
            {
                var petData = gameDataService.gameTable.PetDatas.Get(dataId);
                resId = petData?.ResDataId ?? 0;
                type = petData?.Type_Ref;
            }
            else
            {
                var monsterData = gameDataService.gameTable.MonsterDatas.Get(dataId);
                resId = monsterData?.ResDataId ?? 0;
                type = monsterData?.Type_Ref;
            }


            var ret = new ArenaFormationDetailSlot
            {
                SlotId = slot.SlotId,
                DataType = slot.ItemType,
                Name = slot.ItemName,
                Type = type,
                Id = slot.ItemId,
                ModelResId = resId,
                Level = slot.ItemLevel,
                Quality = (Quality)slot.ItemQuality,
                CharacterId = slot.CharacterId,
                GrowTableId = slot.GrowTableId,
                LevelUpTableId = slot.LevelUpTableId,
                Attributes = slot.Attributes.ToArray(),
                Resistances = slot.Resistances.ToArray(),
                LevelUpTable = levelUpTable,
                GrowTable = growTable,
                Character = character
            };
            return ret;
        }
    }
}