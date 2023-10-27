using System.Collections.Generic;

namespace MetaVirus.Logic.Data.Provider
{
    public interface IMonsterListProvider
    {
        public IMonsterDataProvider GetMonsterData(int id);
        public IMonsterDataProvider GetMonsterDataAt(int index);

        /**
         * 返回指定id的数据在列表中的索引，未找到返回-1
         */
        public int GetMonsterDataIndex(int id);

        public int Count { get; }
    }
}