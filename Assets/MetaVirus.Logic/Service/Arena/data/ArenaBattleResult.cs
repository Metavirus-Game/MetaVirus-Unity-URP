﻿using MetaVirus.Logic.Service.Battle;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Service.Arena.data
{
    public enum BattleResult
    {
        Win = 0,
        Draw,
        Lose
    }

    public class ArenaBattleResult
    {
        /// <summary>
        /// 战斗结果，胜平负
        /// </summary>
        public BattleResult Result { get; private set; }

        /// <summary>
        /// 战斗录像
        /// </summary>
        public BattleRecord BattleRecord { get; private set; }

        /// <summary>
        /// 玩家新的竞技场排名积分数据
        /// </summary>
        public ArenaSeasonInfo ArenaInfo { get; private set; }

        private ArenaBattleResult()
        {
        }

        public static ArenaBattleResult FromProtoBuf(ArenaMatchBattleResponseScPb pb)
        {
            var ret = new ArenaBattleResult
            {
                Result = (BattleResult)pb.Result,
                BattleRecord = BattleRecord.FromGZipData(pb.BattleRecord.ToByteArray()),
                ArenaInfo = ArenaSeasonInfo.FromPbArenaInfo(pb.ArenaInfo)
            };


            return ret;
        }
    }
}