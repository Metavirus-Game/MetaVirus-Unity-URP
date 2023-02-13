using System.Linq;
using cfg.common;
using MetaVirus.Net.Messages.Common;

namespace MetaVirus.Logic.Service.Arena.data
{
    /// <summary>
    /// 竞技场玩家阵型 slot数据
    /// </summary>
    public class ArenaFormationDetailSlot
    {
        public int SlotId { get; private set; }

        /// <summary>
        /// 数据类型，0=玩家PetData，1=怪物MonsterData
        /// </summary>
        public int Type { get; private set; }

        /// <summary>
        /// 数据id，不同类型对应不同数据
        /// </summary>
        public int ItemId { get; private set; }

        /// <summary>
        /// 资源(NpcResourceData)id
        /// </summary>
        public int ItemResId { get; private set; }

        public int ItemLevel { get; private set; }

        public Quality ItemQuality { get; private set; }

        public int CharacterId { get; private set; }
        public int GrowTableId { get; private set; }
        public int LevelUpTableId { get; private set; }

        public int[] Attributes { get; private set; }
        public int[] Resistances { get; private set; }

        public static ArenaFormationDetailSlot FromProtoBuf(PBBattleFormationDetailSlot slot)
        {
            var ret = new ArenaFormationDetailSlot
            {
                SlotId = slot.SlotId,
                Type = slot.ItemType,
                ItemId = slot.ItemId,
                ItemResId = slot.ItemResId,
                ItemLevel = slot.ItemLevel,
                ItemQuality = (Quality)slot.ItemQuality,
                CharacterId = slot.CharacterId,
                GrowTableId = slot.GrowTableId,
                LevelUpTableId = slot.LevelUpTableId,
                Attributes = slot.Attributes.ToArray(),
                Resistances = slot.Resistances.ToArray()
            };
            return ret;
        }
    }
}