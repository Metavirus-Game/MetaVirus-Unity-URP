using System;
using System.Collections.Generic;

namespace MetaVirus.Logic.Data.Player
{
    public class PlayerPetDataGroup<T>
    {
        private readonly List<T> _keys = new();
        private readonly Dictionary<T, List<PlayerPetData>> _petDatas = new();

        public int Count => _keys.Count;

        public List<PlayerPetData> this[T index] => GetPetDataList(index);

        private Comparison<PlayerPetData> _sorter;

        public PlayerPetDataGroup(Comparison<PlayerPetData> sorter = null)
        {
            _sorter = sorter;
        }

        public void AddPetData(T key, PlayerPetData petData)
        {
            var list = GetPetDataList(key);
            list.Add(petData);
            if (_sorter != null)
            {
                list.Sort(_sorter);
            }
        }

        public T GetKeyAt(int index)
        {
            if (index >= _keys.Count)
            {
                return default;
            }

            return _keys[index];
        }

        private List<PlayerPetData> GetPetDataList(T key)
        {
            if (!_keys.Contains(key))
            {
                _keys.Add(key);
                _petDatas[key] = new List<PlayerPetData>();
            }

            return _petDatas[key];
        }
    }
}