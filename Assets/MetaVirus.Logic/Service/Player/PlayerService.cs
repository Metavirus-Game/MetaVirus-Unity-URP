using System.Collections.Generic;
using System.Threading.Tasks;
using cfg.common;
using GameEngine;
using GameEngine.Base;
using GameEngine.DataNode;
using GameEngine.Entity;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Logic.Utils;

namespace MetaVirus.Logic.Service.Player
{
    public class PlayerService : BaseService
    {
        private EntityService _entityService;
        private DataNodeService _dataService;
        private PlayerInfo _currPlayerInfo;

        public PlayerEntity CurrentPlayer => GameFramework.GetService<EntityService>()
            .GetEntity<PlayerEntity>(Constants.EntityGroupName.Player, _currPlayerInfo?.PlayerId ?? -1);

        public PlayerInfo CurrentPlayerInfo => _currPlayerInfo;

        private readonly List<PlayerPetData> _petDatas = new();

        private readonly Dictionary<int, PlayerParty> _parties = new();

        public int PetCount => _petDatas.Count;

        public override void ServiceReady()
        {
            _entityService = GameFramework.GetService<EntityService>();
            _dataService = GameFramework.GetService<DataNodeService>();
        }

        public async Task LoadPlayer(PlayerInfo p)
        {
            _dataService.SetData(Constants.DataKeys.PlayerInfo, p);
            _currPlayerInfo = p;

            foreach (var pp in p.PlayerParties)
            {
                _parties[pp.PartyId] = pp;
            }

            var pe = new PlayerEntity(p);
            await _entityService.AddEntity(Constants.EntityGroupName.Player, pe);
        }

        public void SetPartyInfo(int partyId, int formationDataId, string formationName, int[] slots)
        {
            _parties.TryGetValue(partyId, out var pp);
            if (pp == null)
            {
                pp = new PlayerParty(partyId, formationDataId, formationName);
                _parties.Add(partyId, pp);
            }

            for (var i = 0; i < slots.Length; i++)
            {
                pp[i] = slots[i];
            }
        }

        public int PartySize => _parties.Count;

        public PlayerParty GetPlayerParty(int partyId)
        {
            _parties.TryGetValue(partyId, out var pp);
            return pp;
        }


        /// <summary>
        ///
        /// 根据种族分类所有的宠物
        /// </summary>
        /// <returns></returns>
        public PlayerPetDataGroup<MonsterType> GetAllPetsBySpecies()
        {
            var ret = new PlayerPetDataGroup<MonsterType>(GameUtil.PetDataSortBySpecies);
            foreach (var petData in _petDatas)
            {
                ret.AddPetData(petData.Type, petData);
            }

            return ret;
        }

        /// <summary>
        ///
        /// 根据稀有度分类所有的宠物
        /// </summary>
        /// <returns></returns>
        public PlayerPetDataGroup<Quality> GetAllPetsByQuality()
        {
            var ret = new PlayerPetDataGroup<Quality>(GameUtil.PetDataSortByQuality);
            foreach (var petData in _petDatas)
            {
                ret.AddPetData(petData.Quality, petData);
            }

            return ret;
        }

        public IMonsterListProvider GetPetListProvider()
        {
            return new PlayerPetListProvider(_petDatas);
        }

        public PlayerPetData GetPetData(int id)
        {
            var pet = _petDatas.Find(p => p.Id == id);
            return pet;
        }

        public bool HasPetData(int id)
        {
            return GetPetIndexById(id) != -1;
        }

        public PlayerPetData GetPetDataBySourceDataId(int id)
        {
            var pet = _petDatas.Find(p => p.PetDataId == id);
            return pet;
        }


        public int GetPetIndex(PlayerPetData petData)
        {
            var idx = _petDatas.IndexOf(petData);
            if (idx == -1)
            {
                idx = GetPetIndexById(petData.Id);
            }

            return idx;
        }

        public int GetPetIndexById(int id)
        {
            for (var idx = 0; idx < _petDatas.Count; idx++)
            {
                if (_petDatas[idx].Id == id)
                {
                    return idx;
                }
            }

            return -1;
        }

        public PlayerPetData GetPetDataAt(int index)
        {
            if (index >= _petDatas.Count)
            {
                index = _petDatas.Count - 1;
            }

            return index < 0 ? null : _petDatas[index];
        }

        public void AddPetData(PlayerPetInfo petInfo)
        {
            var petData = new PlayerPetData(petInfo);
            AddPetData(petData);
        }

        public void AddPetData(PlayerPetData petData)
        {
            var pet = GetPetData(petData.Id);

            if (pet != null)
            {
                _petDatas.Remove(pet);
            }

            _petDatas.Add(petData);
            _petDatas.Sort((p1, p2) =>
            {
                if (p1.Type != p2.Type)
                {
                    return p1.Type.Id - p2.Type.Id;
                }

                if (p1.Quality != p2.Quality)
                {
                    return p2.Quality - p1.Quality;
                }

                return p2.Id - p1.Id;
            });
        }
    }
}