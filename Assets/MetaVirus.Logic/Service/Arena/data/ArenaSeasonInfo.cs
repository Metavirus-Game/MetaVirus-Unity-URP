using MetaVirus.Net.Messages.Common;

namespace MetaVirus.Logic.Service.Arena.data
{
    /// <summary>
    /// 竞技场赛季数据
    /// </summary>
    public class ArenaSeasonInfo
    {
        public int ArenaId { get; set; }
        public int Rank { get; set; }
        public int Score { get; set; }
        public int WinCount { get; set; }
        public int LoseCount { get; set; }
        public int DrawCount { get; set; }
        public int SeasonNo { get; set; }

        public static ArenaSeasonInfo FromPbArenaInfo(PBArenaInfo pbArenaInfo)
        {
            var ret = new ArenaSeasonInfo
            {
                ArenaId = pbArenaInfo.ArenaId,
                Rank = pbArenaInfo.Rank,
                Score = pbArenaInfo.Score,
                WinCount = pbArenaInfo.WinCount,
                LoseCount = pbArenaInfo.LoseCount,
                DrawCount = pbArenaInfo.DrawCount,
                SeasonNo = pbArenaInfo.SeasonNo
            };
            return ret;
        }
    }
}