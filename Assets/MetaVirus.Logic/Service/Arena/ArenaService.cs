﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameEngine;
using GameEngine.Base;
using GameEngine.Network;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Network;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Protocols.Arena;
using MetaVirus.Logic.Service.Arena.data;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.UI;
using MetaVirus.Net.Messages.Arena;
using UnityEngine.Events;

namespace MetaVirus.Logic.Service.Arena
{
    public class ArenaService : BaseService
    {
        private NetworkService _networkService;
        private PlayerService _playerService;
        private GameDataService _gameDataService;

        /**
         * arenaId ↔ ArenaPlayerDataMap
         */
        private readonly Dictionary<int, ArenaPlayerDataMap> _playerDataMap = new();

        public override void PostConstruct()
        {
        }

        private void OnPlayerLogin(PlayerInfo playerInfo)
        {
            foreach (var arenaData in _gameDataService.gameTable.ArenaDatas.DataList)
            {
                GetPlayerArenaData(arenaData.Id, playerInfo.PlayerId);
            }
        }

        /// <summary>
        /// 获取指定玩家的指定竞技场的数据
        /// </summary>
        /// <param name="arenaId">竞技场id</param>
        /// <param name="playerId">玩家id</param>
        /// <returns></returns>
        public async Task<NetworkResult<ArenaPlayerData>> GetPlayerArenaData(int arenaId, int playerId)
        {
            _playerDataMap.TryGetValue(arenaId, out var map);
            map ??= _playerDataMap[arenaId] = new ArenaPlayerDataMap(arenaId);
            var result = map.GetArenaPlayerData(playerId) == null
                ? await _GetPlayerArenaData(arenaId, playerId)
                : new NetworkResult<ArenaPlayerData>(map.GetArenaPlayerData(playerId));
            if (result.IsSuccess)
            {
                map.SetArenaPlayerData(result.Result);
            }

            return result;
        }

        /// <summary>
        /// 获取指定玩家在指定竞技场的防守阵型
        /// </summary>
        /// <param name="arenaId"></param>
        /// <param name="playerId"></param>
        /// <returns></returns>
        public async Task<NetworkResult<ArenaFormationDetail>> GetPlayerArenaFormation(int arenaId, int playerId)
        {
            var pb = new ArenaPlayerFormationRequestCsPb
            {
                ArenaId = arenaId,
                PlayerId = playerId
            };

            var req = new ArenaPlayerFormationRequestCs(pb);

            var playerInfo = _playerService.CurrentPlayerInfo;
            var resp = await _networkService.SendPacketToAsync(req, playerInfo.sceneServerId);

            if (!resp.IsSuccess)
            {
                var r = new NetworkResult<ArenaFormationDetail>(resp.ErrorMessageCode, resp.IsTimeout);
                return r;
            }

            var packet = resp.GetPacket<ArenaPlayerFormationResponseSc>();
            var ret = ArenaFormationDetail.FromProtoBuf(packet.ProtoBufMsg.Formation);

            return new NetworkResult<ArenaFormationDetail>(ret);
        }

        /// <summary>
        /// 获取指定竞技场的排行榜
        /// </summary>
        /// <param name="arenaId">竞技场id</param>
        /// <returns></returns>
        public async Task<NetworkResult<List<ArenaPlayerData>>> GetArenaTopRankList(int arenaId)
        {
            var pb = new ArenaTopRankListRequestCsPb
            {
                ArenaId = arenaId
            };

            var req = new ArenaTopRankListRequestCs(pb);

            var playerInfo = _playerService.CurrentPlayerInfo;
            var resp = await _networkService.SendPacketToAsync(req, playerInfo.sceneServerId);

            if (!resp.IsSuccess)
            {
                return new NetworkResult<List<ArenaPlayerData>>(resp.ErrorMessageCode, resp.IsTimeout);
            }

            var packet = resp.GetPacket<ArenaTopRankListResponseSc>();
            var list = packet.ProtoBufMsg.PlayerInfo.Select(ArenaPlayerData.FromPbArenaPlayerInfo).ToList();

            return new NetworkResult<List<ArenaPlayerData>>(list);
        }

        /// <summary>
        /// 获取指定竞技场的匹配列表
        /// </summary>
        /// <param name="arenaId">竞技场id</param>
        /// <returns></returns>
        public async Task<NetworkResult<List<ArenaPlayerData>>> GetArenaMatchList(int arenaId)
        {
            var pb = new ArenaMatchListRequestCsPb
            {
                ArenaId = arenaId
            };

            var req = new ArenaMatchListRequestCs(pb);

            var playerInfo = _playerService.CurrentPlayerInfo;
            var resp = await _networkService.SendPacketToAsync(req, playerInfo.sceneServerId);

            if (!resp.IsSuccess)
            {
                return new NetworkResult<List<ArenaPlayerData>>(resp.ErrorMessageCode, resp.IsTimeout);
            }

            var packet = resp.GetPacket<ArenaMatchListResponseSc>();
            var list = packet.ProtoBufMsg.PlayerInfo.Select(ArenaPlayerData.FromPbArenaPlayerInfo).ToList();

            return new NetworkResult<List<ArenaPlayerData>>(list);
        }

        /// <summary>
        /// 向服务请求竞技场战斗
        /// </summary>
        /// <param name="arenaId">竞技场id</param>
        /// <param name="attackPartyId">进攻阵型id</param>
        /// <param name="defenceId">防守方玩家id</param>
        /// <returns></returns>
        public async Task<NetworkResult<ArenaBattleResult>> RunMatchBattle(int arenaId, int attackPartyId,
            int defenceId)
        {
            var pb = new ArenaMatchBattleRequestCsPb
            {
                ArenaId = arenaId,
                AttackPartyId = attackPartyId,
                DefencePlayerId = defenceId
            };

            var req = new ArenaMatchBattleRequestCs(pb);
            var playerInfo = _playerService.CurrentPlayerInfo;
            var resp = await _networkService.SendPacketToAsync(req, playerInfo.sceneServerId);

            if (!resp.IsSuccess)
            {
                return new NetworkResult<ArenaBattleResult>(resp.ErrorMessageCode, resp.IsTimeout);
            }

            var packet = resp.GetPacket<ArenaMatchBattleResponseSc>();
            var data = ArenaBattleResult.FromProtoBuf(packet.ProtoBufMsg);

            //更新玩家的竞技场排名数据
            _playerDataMap.TryGetValue(arenaId, out var map);
            map?.SetArenaPlayerSeasonInfo(playerInfo.PlayerId, data.ArenaInfo);

            return new NetworkResult<ArenaBattleResult>(data);
        }


        private async Task<NetworkResult<ArenaPlayerData>> _GetPlayerArenaData(int arenaId, int playerId,
            UnityAction<NetworkResult<ArenaPlayerData>> onData = null)
        {
            var pb = new ArenaPlayerInfoRequestCsPb
            {
                ArenaId = arenaId,
                PlayerId = playerId
            };

            var req = new ArenaPlayerInfoRequestCs(pb);

            var playerInfo = _playerService.CurrentPlayerInfo;
            var resp = await _networkService.SendPacketToAsync(req, playerInfo.sceneServerId);

            if (!resp.IsSuccess)
            {
                var r = new NetworkResult<ArenaPlayerData>(resp.ErrorMessageCode, resp.IsTimeout);
                onData?.Invoke(r);
                return r;
            }

            var packet = resp.GetPacket<ArenaPlayerInfoResponseSc>();
            var data = ArenaPlayerData.FromPbArenaPlayerInfo(packet.ProtoBufMsg.PlayerInfo);
            var ret = new NetworkResult<ArenaPlayerData>(data);
            onData?.Invoke(ret);
            return ret;
        }

        public override void ServiceReady()
        {
            Event.On<PlayerInfo>(GameEvents.PlayerEvent.PlayerLoginSuccessful, OnPlayerLogin);
            _networkService = GameFramework.GetService<NetworkService>();
            _playerService = GameFramework.GetService<PlayerService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
        }

        public override void PreDestroy()
        {
            Event.Remove<PlayerInfo>(GameEvents.PlayerEvent.PlayerLoginSuccessful, OnPlayerLogin);
        }
    }
}