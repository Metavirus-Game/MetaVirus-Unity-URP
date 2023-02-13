using MetaVirus.Net.Messages.Common;

namespace MetaVirus.Logic.Service.Arena.data
{
    public enum ArenaPlayerType
    {
        Player = 0,
        Bot = 1,
    }

    /// <summary>
    /// 竞技场玩家数据
    /// </summary>
    public class ArenaPlayerData
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public int PlayerLevel { get; set; }
        public ArenaPlayerType Type { get; set; }

        public float UpdateTime { get; set; }

        public ArenaSeasonInfo ArenaInfo { get; set; }

        public static ArenaPlayerData FromPbArenaPlayerInfo(PBArenaPlayerInfo playerInfo)
        {
            var data = new ArenaPlayerData
            {
                PlayerId = playerInfo.PlayerId,
                PlayerName = playerInfo.PlayerName,
                PlayerLevel = playerInfo.PlayerLevel,
                Type = (ArenaPlayerType)playerInfo.Type,
                ArenaInfo = ArenaSeasonInfo.FromPbArenaInfo(playerInfo.ArenaInfo)
            };
            return data;
        }
    }
}