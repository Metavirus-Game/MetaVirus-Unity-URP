using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameEngine.Config
{
    public enum NextEnterProcedure
    {
        MainPageProcedure = 1,
        BattleTestProcedure,
        MonsterTestProcedure,
        UITestProcedure
    }

    public enum GameVersion
    {
        Local = 0,
        Preview = 1,
        Production
    }

    [Serializable]
    public class GameServer
    {
        public GameVersion version;
        public string address;
        public int port;
        public string worldServerId;
        public string accountServer;
    }


    [CreateAssetMenu(menuName = "GameEngine/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        private static GameConfig _inst;

        public static GameConfig Inst
        {
            get
            {
                if (_inst == null)
                {
                    _inst = Resources.FindObjectsOfTypeAll<GameConfig>().FirstOrDefault();
                    if (_inst == null)
                    {
                        _inst = Resources.Load<GameConfig>("Configs/GameConfig");
                    }

#if !UNITY_EDITOR
                    //_inst.offlineTest = false;
#endif
                }

                return _inst;
            }
        }


        [SerializeField] private GameVersion gameVersion;

        [Header("Network Config")] [SerializeField]
        private GameServer[] servers;

//        [SerializeField] private string server;
        //       [SerializeField] private int port;
        [SerializeField] private string battleServerIp;
        [SerializeField] private int battleServerPort;
        [SerializeField] private string moduleId;
        [SerializeField] private string instId;

        [SerializeField] private string channelName;

        // [SerializeField] private string worldServerId;
        [SerializeField] private int targetFps = 60;
        [SerializeField] private NextEnterProcedure nextProcedure = NextEnterProcedure.MainPageProcedure;
        [SerializeField] private bool offlineTest = true;
        [SerializeField] private bool savePlayerId = false;
        [SerializeField] private int[] timeScaleOptions = new[] { 1, 2, 3 };

        public bool SavePlayerId
        {
            get => savePlayerId;
            set => savePlayerId = value;
        }

        public GameVersion GameVersion => gameVersion;

        public NextEnterProcedure NextEnterProcedure
        {
            get => nextProcedure;
            set => nextProcedure = value;
        }

        public string AccountServer => servers.First(s => s.version == gameVersion)?.accountServer ?? "";

        public string Server
        {
            get
            {
                var sc = servers.First(s => s.version == gameVersion);
                return sc?.address ?? "";
            }
            set
            {
                //TODO 
            }
        }

        public int Port
        {
            get
            {
                var sc = servers.First(s => s.version == gameVersion);
                return sc?.port ?? 0;
            }
            set
            {
                //TODO
            }
        }

        /// <summary>
        /// 战斗服务器端口，临时测试用
        /// </summary>
        public int BattleServerPort
        {
            get => battleServerPort;
            set => battleServerPort = value;
        }

        public string BattleServerIp
        {
            get => battleServerIp;
            set => battleServerIp = value;
        }

        public int TargetFps => targetFps;

        public int[] TimeScaleOptions => timeScaleOptions;

        public short ModuleId
        {
            get =>
                moduleId.StartsWith("0x")
                    ? Convert.ToInt16(moduleId.Substring(2), 16)
                    : Convert.ToInt16(moduleId);
            set => moduleId = $"0x{value:X}";
        }

        public short InstId
        {
            get =>
                instId.StartsWith("0x")
                    ? Convert.ToInt16(instId[2..], 16)
                    : Convert.ToInt16(instId);
            set => instId = $"0x{value:X}";
        }

        public int WorldServerId
        {
            get
            {
                var sc = servers.First(s => s.version == gameVersion);
                var worldServerId = sc?.worldServerId ?? "";
                return worldServerId.StartsWith("0x")
                    ? Convert.ToInt32(worldServerId[2..], 16)
                    : Convert.ToInt32(worldServerId);
            }
            set
            {
                //worldServerId = $"0x{value:X}";
            }
        }

        public int ClientGlobalId => (ModuleId << 8) | (InstId & 0xff);

        public string ChannelName
        {
            get => channelName;
            set => channelName = value;
        }

        public bool OfflineTest
        {
            get => offlineTest;
            set => offlineTest = value;
        }

#if UNITY_EDITOR

        [MenuItem("GameEngine/GameConfig")]
        public static void GetGameConfig()
        {
            if (Inst == null)
            {
                EditorUtility.DisplayDialog("错误", "没有找到GameConfig，请在菜单Assets/Create/GameEngine中创建", "OK");
            }
            else
            {
                UnityEditor.Selection.activeObject = Inst;
            }
        }

#endif
    }
}