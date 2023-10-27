using System.Collections.Generic;
using GameEngine.Config;
using UnityEditor;
using UnityEngine;

public class BuildWindow : EditorWindow
{
    [MenuItem("Tools/Build Tool")]
    static void OpenWindow()
    {
        var wnd = GetWindow<BuildWindow>("Lottery Build Tool");
        wnd.Show();
    }

    private Vector2 _scroll = Vector2.zero;

    private readonly Dictionary<MetaVirusBuildSetting, bool>
        _foldoutMap = new Dictionary<MetaVirusBuildSetting, bool>();

    private void OnGUI()
    {
        var setting = MetaVirusBuildSettings.Inst;
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("新建", "ButtonLeft"))
            {
                var bs = new MetaVirusBuildSetting
                {
                    name = "New Building",
                    gameConfig = new MetaVirusGameConfig(),
                };
                setting.buildSettings.Add(bs);
                _foldoutMap[bs] = true;
            }

            GUI.color = Color.cyan;
            if (GUILayout.Button("Build All", "ButtonRight"))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要打包所有产品吗？", "确定", "取消"))
                {
                    //TODO 打包all
                }
            }

            GUI.color = Color.white;
        }
        GUILayout.EndHorizontal();

        _scroll = GUILayout.BeginScrollView(_scroll);
        {
            foreach (var bs in setting.buildSettings)
            {
                if (!_foldoutMap.ContainsKey(bs))
                {
                    _foldoutMap[bs] = true;
                }

                GUILayout.BeginHorizontal("Badge");
                GUILayout.Space(12);
                _foldoutMap[bs] = EditorGUILayout.Foldout(_foldoutMap[bs], bs.name, true);
                GUILayout.Label(string.Empty);

                if (GUILayout.Button(EditorGUIUtility.IconContent("BuildSettings.SelectedIcon"), "IconButton",
                        GUILayout.Width(20)))
                {
                    Build(bs);
                }

                GUILayout.Space(20);
                
                if (GUILayout.Button(EditorGUIUtility.IconContent("TreeEditor.Trash"), "IconButton",
                        GUILayout.Width(20)))
                {
                    setting.buildSettings.Remove(bs);
                    break;
                }

                GUILayout.EndHorizontal();

                if (_foldoutMap[bs])
                {
                    GUILayout.BeginVertical("Box");

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("产品名称", GUILayout.Width(70));
                    bs.name = GUILayout.TextField(bs.name);
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Label("场景列表", GUILayout.Width(70));

                    if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Plus More"), GUILayout.Width(28)))
                    {
                        bs.buildScenes.Add(null);
                    }

                    GUILayout.EndHorizontal();

                    if (bs.buildScenes.Count > 0)
                    {
                        GUILayout.BeginHorizontal();
                        GUILayout.Space(75);
                        GUILayout.BeginVertical("Badge");
                        for (var s = 0; s < bs.buildScenes.Count; s++)
                        {
                            var scene = bs.buildScenes[s];
                            GUILayout.BeginHorizontal();
                            GUILayout.Label($"Scene-{s + 1}", GUILayout.Width(55));
                            bs.buildScenes[s] =
                                EditorGUILayout.ObjectField(scene, typeof(SceneAsset), false) as SceneAsset;
                            if (GUILayout.Button(EditorGUIUtility.IconContent("Toolbar Minus"), "MiniButtonLeft",
                                    GUILayout
                                        .Width(20)))
                            {
                                bs.buildScenes.Remove(scene);
                                break;
                            }

                            GUILayout.EndHorizontal();
                        }

                        GUILayout.EndVertical();
                        GUILayout.EndHorizontal();
                    }


                    GUILayout.BeginHorizontal();
                    GUILayout.Label("游戏配置", GUILayout.Width(70));

                    GUILayout.BeginVertical("Badge");
                    var config = bs.gameConfig;

                    config.server = EditorGUILayout.TextField("Server Ip", config.server);
                    config.port = EditorGUILayout.IntField("Server Port", config.port);
                    config.battleServerIp = EditorGUILayout.TextField("Battle Server Ip", config.battleServerIp);
                    config.battleServerPort = EditorGUILayout.IntField("Battle Server Port", config.battleServerPort);

                    config.moduleId = EditorGUILayout.TextField("Module Id", config.moduleId);
                    config.instId = EditorGUILayout.TextField("Inst Id", config.instId);

                    config.channelName = EditorGUILayout.TextField("Channel", config.channelName);

                    config.worldServerId = EditorGUILayout.TextField("World Server Id", config.worldServerId);

                    config.nextProcedure =
                        (NextEnterProcedure)EditorGUILayout.EnumPopup("Start Procedure", config.nextProcedure);

                    GUILayout.EndVertical();

                    GUILayout.EndHorizontal();


                    GUI.color = Color.cyan;
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(75);
                    if (GUILayout.Button($"Build {bs.name}"))
                    {
                        Build(bs);
                    }

                    GUILayout.EndHorizontal();
                    GUI.color = Color.white;

                    GUILayout.EndVertical();
                }
            }
        }
        GUILayout.EndScrollView();

        if (GUI.changed)
        {
            EditorUtility.SetDirty(setting);
        }
    }

    private void Build(MetaVirusBuildSetting bs)
    {
        var sceneList = new List<EditorBuildSettingsScene>();
        foreach (var scene in bs.buildScenes)
        {
            var path = AssetDatabase.GetAssetPath(scene);
            if (!string.IsNullOrEmpty(path))
            {
                sceneList.Add(new EditorBuildSettingsScene(path, true));
            }
        }

        var bsCfg = bs.gameConfig;
        bsCfg.ApplyToGameConfig();

        PlayerSettings.productName = bs.name;

        var report = BuildPipeline.BuildPlayer(sceneList.ToArray(), $"Build/{bs.name}/{bs.name}.exe",
            BuildTarget.StandaloneWindows64,
            BuildOptions.None);

        Debug.Log(report.summary.result);
    }
}