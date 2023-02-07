using System.Collections.Generic;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Data.Provider;

namespace MetaVirus.Logic.Service.Player
{
    public class PlayerPetListProvider : IMonsterListProvider
    {
        private readonly List<PlayerPetData> _list;

        public int Count => _list.Count;

        public PlayerPetListProvider(List<PlayerPetData> list)
        {
            _list = list;
        }

        public IMonsterDataProvider GetMonsterData(int id)
        {
            var pet = _list.Find(p => p.Id == id);
            return pet;
        }

        public IMonsterDataProvider GetMonsterDataAt(int index)
        {
            if (index >= Count)
            {
                index = Count - 1;
            }

            return index < 0 ? null : _list[index];
        }

        public int GetMonsterDataIndex(int id)
        {
            for (var idx = 0; idx < Count; idx++)
            {
                if (_list[idx].Id == id)
                {
                    return idx;
                }
            }

            return -1;
        }
    }
}