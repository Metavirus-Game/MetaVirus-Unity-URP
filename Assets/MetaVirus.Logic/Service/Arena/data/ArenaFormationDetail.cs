using System.Collections.Generic;
using MetaVirus.Net.Messages.Common;

namespace MetaVirus.Logic.Service.Arena.data
{
    /**
     * 竞技场阵型详细数据
     */
    public class ArenaFormationDetail
    {
        public int PlayerId { get; set; }
        private Dictionary<int, ArenaFormationDetailSlot> _slots = new();

        /// <summary>
        /// 返回指定位置的阵型数据，如果指定位置没有单位，返回null
        /// </summary>
        /// <param name="slotId">阵型位置id，1 - 9</param>
        /// <returns></returns>
        public ArenaFormationDetailSlot GetSlot(int slotId)
        {
            _slots.TryGetValue(slotId, out var ret);
            return ret;
        }

        public static ArenaFormationDetail FromProtoBuf(PBBattleFormationDetail detail)
        {
            var ret = new ArenaFormationDetail
            {
                PlayerId = detail.PlayerId,
            };

            foreach (var t in detail.Slots)
            {
                var slot = ArenaFormationDetailSlot.FromProtoBuf(t);
                ret._slots[slot.SlotId] = slot;
            }

            return ret;
        }
    }
}