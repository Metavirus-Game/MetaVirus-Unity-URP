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

            /**
             * 客户端向SceneServer上报当前坐标
             */
            public const byte PlayerReportPosition = 0x01;

            public const byte ScNotifyRefreshNpc = 0x02;

            /*
             * 客户端向SceneServer上报进入地图
             * 服务器会下发当前地图npc，怪物等一系列数据
             */
            public const byte CsPlayerReporteEnterMap = 0x03;


            /**
             * 服务器通知客户端有GridItem进入
             */
            public const byte ScNotifyGridItemsEnter = 0x04;

            /**
             * 服务器通知客户端有GridItem移动
             */
            public const byte ScNotifyGridItemsMove = 0x05;

            /**
             * 服务器通知客户端有GridItem离开
             */
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
    }
}