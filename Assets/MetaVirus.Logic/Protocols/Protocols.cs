namespace MetaVirus.Logic.Protocols
{
    public static class Protocols
    {
        public static class User
        {
            public const byte Main = 0x30;

            //账号登陆
            public const byte AccountLoginRequest = 0x01;
            public const byte AccountLoginResponse = 0x02;

            public const byte GameLoginRequest = 0x03;
            public const byte GameLoginResponse = 0x04;

            public const byte PlayerLoginRequest = 0x05;
            public const byte PlayerLoginResponse = 0x06;

            public const byte CreateActorRequestCs = 0x07;
            public const byte CreateActorResponseSc = 0x08;
        }

        public static class Test
        {
            public const byte Main = 0x71;
            public const byte TestCreateMonsterRequest = 0x01;
            public const byte TestCreateMonsterResponse = 0x02;
            public const byte TestBattleRequestCs = 0x03;
            public const byte TestBattleResponseSc = 0x04;
        }

        public static class Scene
        {
            public const byte Main = 0x31;

            /// <summary>
            /// 客户端向SceneServer上报当前坐标
            /// </summary>
            public const byte PlayerReportPosition = 0x01;

            public const byte ScNotifyRefreshNpc = 0x02;

            /// <summary>
            /// 客户端向SceneServer上报进入地图
            /// 服务器会下发当前地图npc，怪物等一系列数据
            /// </summary>
            public const byte CsPlayerReporteEnterMap = 0x03;


            /// <summary>
            /// 服务器通知客户端有GridItem进入
            /// </summary>
            public const byte ScNotifyGridItemsEnter = 0x04;

            /// <summary>
            /// 服务器通知客户端有GridItem移动
            /// </summary>
            public const byte ScNotifyGridItemsMove = 0x05;

            /// <summary>
            /// 服务器通知客户端有GridItem离开
            /// </summary>
            public const byte ScNotifyGridItemsLeave = 0x06;

            public const byte TouchNpcRequestCS = 0x07;

            public const byte TouchNpcResponseSC = 0x08;
        }

        public static class Player
        {
            public const byte Main = 0x32;
            public const byte UpdateFormationRequestCs = 0x01;
            public const byte UpdateFormationResponseSc = 0x02;
            public const byte MapNpcBattleRequestCs = 0x03;
            public const byte MapNpcBattleResponseSc = 0x04;
        }

        public static class Arena
        {
            public const byte Main = 0x33;


            /// <summary>
            /// 获取竞技场玩家信息
            /// </summary>
            public const byte ArenaPlayerInfoRequestCs = 0x01;

            public const byte ArenaPlayerInfoResponseSc = 0x02;

            /// <summary>
            /// 获取竞技场玩家阵型信息
            /// </summary>
            public const byte ArenaPlayerFormationRequestCs = 0x03;

            public const byte ArenaPlayerFormationResponseSc = 0x04;

            /// <summary>
            /// 获取竞技场排行榜
            /// </summary>
            public const byte ArenaTopRankListRequestCs = 0x05;

            public const byte ArenaTopRankListResponseSc = 0x06;

            /// <summary>
            /// 获取玩家匹配列表
            /// </summary>
            public const byte ArenaMatchListRequestCs = 0x07;

            public const byte ArenaMatchListResponseSc = 0x08;

            /// <summary>
            /// 竞技场玩家匹配战斗
            /// </summary>
            public const byte ArenaMatchBattleRequestCs = 0x09;

            public const byte ArenaMatchBattleResponseSc = 0x0A;

            /// <summary>
            /// 获取玩家战斗记录列表
            /// </summary>
            public const byte ArenaRecordListRequestCs = 0x0B;
            public const byte ArenaRecordListResponseSc = 0x0C;

            /// <summary>
            /// 获取指定战斗记录
            /// </summary>
            public const byte ArenaPlayerRecordRequestCs = 0x0D;
            public const byte ArenaPlayerRecordResponseSc = 0x0E;

            /// <summary>
            /// 服务器通知客户端有新的战斗记录
            /// </summary>
            public const byte ArenaNewRecordNotification = 0x70;
        }
    }
}