using System;
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
        UITestProcedure
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


        [Header("Network Config")] [SerializeField]
        private string server;

        [SerializeField] private int port;
        [SerializeField] private int battleServerPort;
        [SerializeField] private string moduleId;
        [SerializeField] private string instId;
        [SerializeField] private string channelName;
        [SerializeField] private string worldServerId;
        [SerializeField] private int targetFps = 60;
        [SerializeField] private NextEnterProcedure nextProcedure = NextEnterProcedure.MainPageProcedure;
        [SerializeField] private bool offlineTest = true;
        [SerializeField] private bool savePlayerId = false;

        public bool SavePlayerId => savePlayerId;
        public NextEnterProcedure NextEnterProcedure => nextProcedure;
        public string Server => server;
        public int Port => port;

        /// <summary>
        /// 战斗服务器端口，临时测试用
        /// </summary>
        public int BattleServerPort => battleServerPort;

        public int TargetFps => targetFps;

        public short ModuleId => moduleId.StartsWith("0x")
            ? Convert.ToInt16(moduleId.Substring(2), 16)
            : Convert.ToInt16(moduleId);

        public short InstId => instId.StartsWith("0x")
            ? Convert.ToInt16(instId[2..], 16)
            : Convert.ToInt16(instId);

        public int WorldServerId => worldServerId.StartsWith("0x")
            ? Convert.ToInt32(worldServerId[2..], 16)
            : Convert.ToInt32(worldServerId);

        public int ClientGlobalId => (ModuleId << 8) | (InstId & 0xff);
        public string ChannelName => channelName;

        public bool OfflineTest => offlineTest;

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