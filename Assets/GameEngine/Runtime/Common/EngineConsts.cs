namespace GameEngine.Common
{
    public static class EngineConsts
    {
        /**
         * 各个Service的优先级定义，未定义的为默认值
         * 值越小的服务，init和update的顺序越靠前，release的位置越靠后
        */
        public static class ServicePriorityValue
        {
            public const int OrderDefault = 1000;
            public const int LocalizeService = -100;
            public const int DataNodeService = -20;
            public const int ObjectPool = -20;
            public const int NetworkService = -10;
            public const int FsmService = -2;
            public const int ProcedureService = -1;
        }

        public enum SocketDisType
        {
            Exception,
            Disconnect
        }

        public enum SocketEvent
        {
            NotConnect,
            Connected,
            ConnectFailed,
            Disconnected,
            Reconnected,
            ClientLoginFailed,
            Exception
        }
    }

    public static class Events
    {
        public static class Localize
        {
            public const string LanguageChanged = "Localize_LanguageChanged";
        }

        public static class Engine
        {
            public const string EngineStarted = "Event_Engine_EngineStart";

            public const string ProcedureChanged = "Event_Engine_ProcedureChanged";
        }
        
        public static class SoundEvent
        {
            public const string SoundPlayerEvent = "Event_SoundPlayerEvent";
        }
    }

    public enum UILifeCycle
    {
        OnCreated, //after create,
        OnShowing, //before show
        OnShowed, //after shown
        OnClosing, //before close
        OnClosed, //after close
    }
}