using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameEngine.Config;
using UnityEditor;
using UnityEngine;

[Serializable]
public class MetaVirusBuildSetting
{
    public string name;
    public MetaVirusGameConfig gameConfig;
    public List<SceneAsset> buildScenes = new List<SceneAsset>();
}

[Serializable]
public class MetaVirusGameConfig
{
    public MetaVirusGameConfig()
    {
        var gc = GameConfig.Inst;
        server = gc.Server;
        port = gc.Port;
        battleServerIp = gc.BattleServerIp;
        battleServerPort = gc.BattleServerPort;
        moduleId = $"0x{gc.ModuleId:X}";
        instId = $"0x{gc.InstId:X}";
        channelName = gc.ChannelName;
        worldServerId = $"0x{gc.WorldServerId:X}";
        nextProcedure = gc.NextEnterProcedure;
        offlineTest = gc.OfflineTest;
        savePlayerId = gc.SavePlayerId;
    }

    public void ApplyToGameConfig()
    {
        var gc = GameConfig.Inst;
        gc.Server = server;
        gc.Port = port;
        gc.BattleServerIp = battleServerIp;
        gc.BattleServerPort = battleServerPort;
        gc.ModuleId = Convert.ToInt16(moduleId[2..], 16);
        gc.InstId = Convert.ToInt16(instId[2..], 16);
        gc.ChannelName = channelName;
        gc.WorldServerId = Convert.ToInt16(worldServerId[2..], 16);
        gc.NextEnterProcedure = nextProcedure;
        gc.OfflineTest = offlineTest;
        gc.SavePlayerId = savePlayerId;
    }

    public string server;
    public int port;
    public string battleServerIp;
    public int battleServerPort;
    public string moduleId;
    public string instId;
    public string channelName;
    public string worldServerId;
    public NextEnterProcedure nextProcedure = NextEnterProcedure.MainPageProcedure;
    public bool offlineTest = true;
    public bool savePlayerId = false;
}

[CreateAssetMenu(fileName = "MetaVirusBuildSetting", menuName = "MetaVirus Build Setting")]
public class MetaVirusBuildSettings : ScriptableObject
{
    private static MetaVirusBuildSettings _inst;

    public static MetaVirusBuildSettings Inst
    {
        get
        {
            if (_inst == null)
            {
                _inst = Resources.FindObjectsOfTypeAll<MetaVirusBuildSettings>().FirstOrDefault();
#if UNITY_EDITOR
                if (_inst == null)
                {
                    _inst = AssetDatabase.LoadAssetAtPath<MetaVirusBuildSettings>(
                        "Assets/_EditorUtils/BuildTool/BuildSetting/MetaVirusBuildSetting.asset");
                }
#endif
            }

            return _inst;
        }
    }

    public List<MetaVirusBuildSetting> buildSettings = new();
}