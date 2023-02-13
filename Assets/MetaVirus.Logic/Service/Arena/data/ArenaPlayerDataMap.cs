using System.Collections.Generic;
using UnityEngine;

namespace MetaVirus.Logic.Service.Arena.data
{
    public class ArenaPlayerDataMap
    {
        /// <summary>
        /// 获取竞技场数据间隔120秒
        /// </summary>
        private const float UpdateInterval = 120;

        public int ArenaId { get; }
        private Dictionary<int, ArenaPlayerData> _dataMap = new();

        public ArenaPlayerDataMap(int arenaId)
        {
            ArenaId = arenaId;
        }

        public ArenaPlayerData GetArenaPlayerData(int playerId)
        {
            _dataMap.TryGetValue(playerId, out var data);
            if (data != null && data.UpdateTime - Time.realtimeSinceStartup > UpdateInterval)
            {
                //每两分钟更新一次数据
                _dataMap[playerId] = null;
                data = null;
            }

            return data;
        }

        public void SetArenaPlayerData(ArenaPlayerData data)
        {
            _dataMap[data.PlayerId] = data;
            data.UpdateTime = Time.realtimeSinceStartup;
        }

        public void SetArenaPlayerSeasonInfo(int playerId, ArenaSeasonInfo arenaSeasonInfo)
        {
            _dataMap.TryGetValue(playerId, out var data);
            if (data == null) return;
            data.ArenaInfo = arenaSeasonInfo;
            data.UpdateTime = Time.realtimeSinceStartup;
        }
    }
}