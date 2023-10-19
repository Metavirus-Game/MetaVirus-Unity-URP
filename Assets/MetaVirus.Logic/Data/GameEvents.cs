namespace MetaVirus.Logic.Data
{
    public static class GameEvents
    {
        public static class GameEvent
        {
            public const string ServerConnected = "ServerConnected";

            public const string GameDataUpdated = "GameEvent_GameDataUpdated";
        }


        public static class ControllerEvent
        {
            public static string JoystickEvent = "ControllerEvent_JoystickEvent";
        }

        public static class ResourceEvent
        {
            public const string AllResLoaded = "ResourceEvent_AllResLoaded";
        }

        public static class MapEvent
        {
            //准备切换到指定地图
            public const string MapStartChanging = "MapEvent_MapStartChanging";

            //地图切换完成
            public const string MapChanged = "MapEvent_MapChanged";

            public const string NpcEvent = "MapEvent_NpcEvent";

            public const string GridItemEvent = "MapEvent_GridItemEvent";
        }

        public static class PlayerEvent
        {
            /**
             * 玩家登陆成功后发送
             */
            public const string PlayerLoginSuccessful = "PlayerEvent_PlayerLoginSuccessful";

            public const string InteractiveNpcListChanged = "PlayerEvent_InteractiveNpcListChanged";
            public const string InteractingWithNpc = "PlayerEvent_InteractingWithNpc";
        }

        public static class UIEvent
        {
            public const string TopLayerFullscreenUIChanged = "UIEvent_TopLayerFullscreenUIChanged";
        }

        public static class BattleEvent
        {
            public const string Battle = "BattleEvent_Battle";

            public const string OnSkillDamage = "BattleEvent_OnSkillDamage";

            public const string OnBuffEffectDamage = "BattleEvent_OnBuffEffectDamage";

            public const string OnUnitAction = "BattleEvent_OnUnitAction";

            public const string OnUnitPropertiesChanged = "BattleEvent_OnUnitPropertiesChanged";
        }

        public static class ArenaEvent
        {
            public const string NewRecordNotifition = "ArenaEvent_NewRecordNotifition";
        }
    }
}