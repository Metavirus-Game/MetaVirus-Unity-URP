using System.Collections.Generic;
using System.Linq;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Net.Messages.Common;

namespace MetaVirus.Logic.Service.Arena.data
{
    /**
     * 竞技场阵型详细数据
     */
    public class ArenaFormationDetail : IMonsterListProvider
    {
        public int PlayerId { get; set; }
        private readonly Dictionary<int, IMonsterDataProvider> _slots = new();

        public int Count => _slots.Count;


        /// <summary>
        /// 返回指定位置的阵型数据，如果指定位置没有单位，返回null
        /// </summary>
        /// <param name="slotId">阵型位置id，0-4</param>
        /// <returns></returns>
        public IMonsterDataProvider GetSlot(int slotId)
        {
            _slots.TryGetValue(slotId, out var ret);
            return ret;
        }

        public IMonsterDataProvider GetMonsterData(int id)
        {
            return _slots.Values.FirstOrDefault(md => md.Id == id);
        }

        public IMonsterDataProvider GetMonsterDataAt(int index)
        {
            if (index >= 0 && index < Count)
            {
                var keys = _slots.Keys.ToArray();
                var k = keys[index];
                return _slots[k];
            }

            return null;
        }

        public int GetMonsterDataIndex(int id)
        {
            var mds = _slots.Values.ToArray();
            for (var i = 0; i < mds.Length; i++)
            {
                if (mds[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
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