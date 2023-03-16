using MetaVirus.Logic.Data;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Net.Messages.Arena;

namespace MetaVirus.Logic.Service.Arena.data
{
    public class ArenaBattleResult
    {
        /// <summary>
        /// 战斗结果，胜平负
        /// </summary>
        public Constants.BattleResult Result { get; private set; }

        /// <summary>
        /// 战斗录像
        /// </summary>
        public BattleRecord BattleRecord { get; private set; }

        /// <summary>
        /// 玩家新的竞技场排名积分数据
        /// </summary>
        public ArenaSeasonInfo ArenaInfo { get; private set; }

        public int RankChanged { get; private set; }
        public int ScoreChanged { get; private set; }

        private ArenaBattleResult()
        {
        }

        public static ArenaBattleResult FromProtoBuf(ArenaMatchBattleResponseScPb pb, ArenaPlayerData currentData)
        {
            var ret = new ArenaBattleResult
            {
                Result = (Constants.BattleResult)pb.Result,
                BattleRecord = BattleRecord.FromGZipData(pb.BattleRecord.ToByteArray()),
                ArenaInfo = ArenaSeasonInfo.FromPbArenaInfo(pb.ArenaInfo),
                ScoreChanged = pb.ArenaInfo.Score - currentData?.ArenaInfo.Score ?? pb.ArenaInfo.Score,
                RankChanged = pb.ArenaInfo.Rank - currentData?.ArenaInfo.Rank ?? pb.ArenaInfo.Rank
            };

            return ret;
        }
    }
}