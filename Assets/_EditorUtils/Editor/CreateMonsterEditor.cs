using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using cfg.common;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Battle;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

public class CreateMonsterEditor : EditorWindow
{
    [MenuItem("GameEngine/Cute Series Monster Tool")]
    static void OpenWindow()
    {
        var rect = new Rect(0, 0, 800, 500);
        var wnd = GetWindow<CreateMonsterEditor>("Cute Series Monster Tool");
        wnd.Show();
    }

    private class FbxClipInfo
    {
        public FileInfo FbxFile { get; }
        public ModelImporter Importer { get; }
        public ModelImporterClipAnimation ClipAnimation { get; }

        public FbxClipInfo(FileInfo fbxFile, ModelImporter importer, ModelImporterClipAnimation clipAnimation)
        {
            Importer = importer;
            FbxFile = fbxFile;
            ClipAnimation = clipAnimation;
        }
    }

    private class ClipConfig
    {
        public string ClipName;

        public string BackupClip;
        public string[] Keywords { get; }

        public bool Strict { get; }

        public AnimationEvent Event => (string.IsNullOrEmpty(_eventString) || _eventTime == 0)
            ? null
            : new AnimationEvent
            {
                time = _eventTime,
                functionName = "OnBattleAniEvent",
                stringParameter = _eventString
            };

        private readonly string _eventString;
        private readonly float _eventTime;
        public bool IsLoop { get; }

        public ClipConfig(string clipName, string[] keywords, string eventString = null, float eventTime = 0,
            bool loop = false, bool strict = false, string backupClip = null)
        {
            Strict = strict;
            ClipName = clipName;
            BackupClip = backupClip;
            Keywords = keywords;
            _eventString = eventString;
            _eventTime = eventTime;
            IsLoop = loop;
        }
    }

    private readonly Dictionary<string, string> monsterAlias = new();

    private readonly Dictionary<string, ClipConfig> mapClipName2Keywords = new();
    private readonly Dictionary<string, ClipConfig> battleClipName2Keywords = new();

    private static string[] AttackKeywords =
    {
        "Bite Attack", "Slash Attack", "Punch Attack", "Head Attack", "Melee Attack", "Stab Attack",
        "Left Attack", "Right Attack", "PAttack"
    };

    private static string[] DieKeywords = { "Die" };
    private static string[] IdleKeywords = { "Idle Open", "Idle" };

    private static string[] RunKeywords =
    {
        "Move Forward Fast In Place", "Jump Forward Fast In Place", "Roll Forward In Place", "Forward Fast In Place",
        "Forward In Place", "Foward In Place", "Run In Place", "Open In Place", "Run Forward WO Root"
    };

    private static string[] SpawnKeywords = { "Spawn", "Idle" };
    private static string[] TakeDamageKeywords = { "Take Damage" };
    private static string[] UndergroundKeywords = { "Underground", "Idle" };

    private static string[] WalkKeywords =
    {
        "Move Forward Slow In Place", "Roll Forward In Place", "Forward Slow In Place", "Forward In Place",
        "Foward In Place", "Walk In Place", "Open In Place", "Walk Forward WO Root"
    };

    private static string[] CastSpellKeywords = { "Cast Spell", "Roar", "Fly and Spin" };

    private static string[] ProjectileKeywords =
    {
        "Projectile Attack", "Shoot Attack", "Sting Attack", "Machine Gun Attack"
    };

    private string _pathBattleController = "Assets/MetaVirus.Res/Models/NpcPrefabs/MonsterBattleController.controller";
    private string _pathMapController = "Assets/MetaVirus.Res/Models/NpcPrefabs/MonsterMapController.controller";
    private string _cuteSeriesDir = "D:/Projects/UnityProjects/AssetsRepository/Assets/Cute Series Pack";
    private string _toResDir = "Assets/MetaVirus.Res/Models/";
    private string _cuteSeriesPack = "All";
    private string _cuteSeriesId = "";
    private string _prefabDir = "Assets/MetaVirus.Res/Models/NpcPrefabs";

    private int _maxTextureSize = 512;
    private int _maxEmissionSize = 128;

    private string[] _processPacks;
    private List<int> _processIds = new();

    private void Awake()
    {
        GameDataService.EditorInst.LoadGameDataFromDisk();

        mapClipName2Keywords["Die"] = new ClipConfig("Die", DieKeywords);
        mapClipName2Keywords["MeleeAttack1"] = new ClipConfig("Melee Attack", AttackKeywords, "MeleeAttack", 0.4f,
            backupClip: "Projectile Attack");
        mapClipName2Keywords["Spawn"] = new ClipConfig("Spawn", SpawnKeywords);
        mapClipName2Keywords["Take Damage"] = new ClipConfig("TakeDamage", TakeDamageKeywords);
        mapClipName2Keywords["Underground"] = new ClipConfig("Underground", UndergroundKeywords, strict: true);
        mapClipName2Keywords["Walk Forward In Place"] = new ClipConfig("Walk", WalkKeywords, loop: true);
        mapClipName2Keywords["Run Forward In Place"] = new ClipConfig("Run", RunKeywords, loop: true);
        mapClipName2Keywords["Idle"] = new ClipConfig("Idle", IdleKeywords, loop: true, strict: true);
        mapClipName2Keywords["Projectile Attack"] =
            new ClipConfig("Projectile Attack", ProjectileKeywords, "SpawnProjectile", 0.5f,
                backupClip: "MeleeAttack1");

        battleClipName2Keywords["Die"] = new ClipConfig("Die", DieKeywords);
        battleClipName2Keywords["MeleeAttack1"] = new ClipConfig("Melee Attack", AttackKeywords, "MeleeAttack", 0.4f,
            backupClip: "Projectile Attack");
        battleClipName2Keywords["Spawn"] = new ClipConfig("Spawn", SpawnKeywords);
        battleClipName2Keywords["Take Damage"] = new ClipConfig("TakeDamage", TakeDamageKeywords);
        battleClipName2Keywords["Underground"] = new ClipConfig("Underground", UndergroundKeywords, strict: true);
        battleClipName2Keywords["CastSpell1"] = new ClipConfig("CastSpell", CastSpellKeywords, "CastSpell", 0.4f);
        battleClipName2Keywords["Projectile Attack"] =
            new ClipConfig("Projectile Attack", ProjectileKeywords, "SpawnProjectile", 0.5f,
                backupClip: "MeleeAttack1");
        battleClipName2Keywords["Walk Forward In Place"] = new ClipConfig("Walk", WalkKeywords, loop: true);
        battleClipName2Keywords["Run Forward In Place"] = new ClipConfig("Run", RunKeywords, loop: true);
        battleClipName2Keywords["Idle"] = new ClipConfig("Idle", IdleKeywords, loop: true, strict: true);

        monsterAlias["Eyeball Bat"] = "Eyeball Bat Red";
        monsterAlias["Eyeball Creep"] = "Eyeball Creep Red";
        monsterAlias["Eyeball Mage"] = "Eyeball Mage Red";

        monsterAlias["Treant Minion"] = "Treant Minion Evergreen";
        monsterAlias["Treant Tree"] = "Treant Tree Evergreen";
        monsterAlias["Treant Forest"] = "Treant Forest Evergreen";

        monsterAlias["Goblin Trooper Modular Pack"] = "Goblin Trooper Soldier";
        monsterAlias["Goblin Giant Modular Pack"] = "Goblin Giant Devil";

        monsterAlias["Ball Robot"] = "Ball Robot Blue";
        monsterAlias["Hermit Robot"] = "Hermit Robot Blue";
        monsterAlias["Blast Robot"] = "Blast Robot Blue";

        monsterAlias["Dragon Bot Robot"] = "Dragon Bot";
    }

    private void OnGUI()
    {
        GUILayout.Space(20);

        GUILayout.Label("本工具会将Cute Series资源，自动同步到指定的游戏资源文件夹中，并根据数据表中的id进行重命名");

        GUILayout.Space(20);

        _cuteSeriesDir = EditorGUILayout.TextField("Cute Series模型源文件夹:", _cuteSeriesDir);
        EditorGUILayout.LabelField("要处理的Pack Number，All表示全部处理，多个PackNumber用,分开");
        _cuteSeriesPack = EditorGUILayout.TextField("", _cuteSeriesPack);

        EditorGUILayout.LabelField("要处理的怪物Id，不填表示全部处理，多个怪物Id用,分开");
        _cuteSeriesId = EditorGUILayout.TextField("", _cuteSeriesId);

        _toResDir = EditorGUILayout.TextField("游戏资源文件夹:", _toResDir);
        _prefabDir = EditorGUILayout.TextField("Npc Prefabs文件夹:", _prefabDir);
        GUILayout.Space(20);
        _pathMapController = EditorGUILayout.TextField("地图用状态机:", _pathMapController);
        _pathBattleController = EditorGUILayout.TextField("战斗用状态机:", _pathBattleController);
        GUILayout.Space(10);

        _maxTextureSize = EditorGUILayout.IntField("贴图最高分辨率", _maxTextureSize);
        _maxEmissionSize = EditorGUILayout.IntField("发光贴图最高分辨率", _maxEmissionSize);
        GUILayout.Space(10);

        if (GUILayout.Button("同步模型到游戏资源文件夹", GUILayout.Width(200)))
        {
            _processIds.Clear();

            _processPacks = _cuteSeriesPack.Split(",");
            for (var i = 0; i < _processPacks.Length; i++)
            {
                _processPacks[i] = _processPacks[i].Trim();
            }

            var strs = _cuteSeriesId.Split(",");
            foreach (var t in strs)
            {
                if (int.TryParse(t.Trim(), out var id))
                {
                    _processIds.Add(id);
                }
            }

            if (_cuteSeriesPack.ToLower() != "all" ||
                EditorUtility.DisplayDialog("all", "生成全部数据比较耗时，确定要同步全部数据吗？", "确定", "取消"))
            {
                var info = GetAssetsDir(_cuteSeriesDir);
                if (!info.Exists)
                {
                    GUILayout.Label("Can't find the resource dir: " + info.FullName);
                }
                else
                {
                    var dirs = info.GetDirectories();
                    foreach (var dir in dirs)
                    {
                        ProcessCuteSeriesDir(dir);
                    }
                }
            }
        }
    }

    private void ProcessCuteSeriesDir(DirectoryInfo dir)
    {
        var names = dir.Name.Split(' ');
        var packNo = names[3];

        if (_cuteSeriesPack.ToLower() != "all" && !_processPacks.Contains(packNo))
        {
            return;
        }

        Debug.Log("processing dir: " + dir.Name);
        var resDir = GetAssetsDir(Path.Combine(_toResDir, $"CuteRes-{packNo}"));
        if (!resDir.Exists)
        {
            resDir.Create();
        }

        foreach (var modelDir in dir.GetDirectories())
        {
            ProcessModel(modelDir, resDir);
        }
    }

    /// <summary>
    /// 将modelDir中需要的模型、贴图、材质，拷贝到指定的资源文件夹下，将prefab拷贝到_prefabDir中
    /// </summary>
    /// <param name="modelDir"></param>
    /// <param name="gameResDir"></param>
    private void ProcessModel(DirectoryInfo modelDir, DirectoryInfo gameResDir)
    {
        var npcResTable = GameDataService.EditorInst.gameTable.NpcResourceDatas.DataList;

        //find model name
        var names = modelDir.Name.Split(' ');
        var name = "";
        for (var i = 0; i < names.Length - 2; i++)
        {
            name += names[i] + " ";
        }

        name = name.Trim();

        if (monsterAlias.ContainsKey(name))
        {
            name = monsterAlias[name];
        }

        var modelNpcData = npcResTable.FirstOrDefault(data => data.Name.ToLower().Equals(name.ToLower()));
        if (modelNpcData == null)
        {
            Debug.LogError($"can't find npc resource data for model {modelDir.FullName}");
            return;
        }

        if (_processIds.Count > 0 && !_processIds.Contains(modelNpcData.Id))
        {
            return;
        }

        var resModelDirName = $"{modelNpcData.Id}-{modelNpcData.Name}";
        var modelToResDir = Path.Combine(gameResDir.FullName, resModelDirName);
        if (!Directory.Exists(modelToResDir))
        {
            Directory.CreateDirectory(modelToResDir);
        }

        //move FBX dir
        MoveFbxes(modelDir, modelToResDir);

        //Move Materials dir
        MoveResources(modelDir, modelToResDir, "Materials");
        //Move Textures Dir
        MoveTextures(modelDir, modelToResDir);

        //Move Prefab
        var preDir = Path.Combine(modelDir.FullName, "Prefabs");
        MovePrefab(preDir, modelToResDir, modelNpcData);


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private void MoveTextures(DirectoryInfo modelDir, string modelToResDir)
    {
        MoveResources(modelDir, modelToResDir, "Textures");
        var texDir = Path.Combine(modelToResDir, "Textures");
        var dir = new DirectoryInfo(texDir);
        var textures = dir.GetFiles("*.psd");

        foreach (var texFile in textures)
        {
            var assetPath = FullPathToAssetPath(texFile.FullName);
            var importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            var maxTexSize = _maxTextureSize;
            if (texFile.Name.IndexOf("Emission", StringComparison.Ordinal) != -1)
            {
                maxTexSize = _maxEmissionSize;
            }

            if (importer.maxTextureSize > maxTexSize)
            {
                importer.maxTextureSize = maxTexSize;
            }
        }
    }

    private void MoveFbxes(DirectoryInfo modelDir, string modelToResDir)
    {
        var resName = "FBX";
        var fromDir = Path.Combine(modelDir.FullName, resName);
        var toDir = Path.Combine(modelToResDir, resName);
        if (!Directory.Exists(fromDir))
        {
            Debug.Log($"{fromDir} Not Found, Skip...");
            return;
        }

        if (!Directory.Exists(toDir))
        {
            Directory.CreateDirectory(toDir);
        }

        var fromDirInfo = new DirectoryInfo(fromDir);
        foreach (var fileInfo in fromDirInfo.GetFiles())
        {
            if (fileInfo.Extension == ".controller")
            {
                //不拷贝controller
                continue;
            }

            if (fileInfo.Extension.ToUpper() == ".FBX")
            {
                //fbx文件，拷贝
                var targetFile = new FileInfo(Path.Combine(toDir, fileInfo.Name));
                if (targetFile.Exists)
                {
                    FileUtil.DeleteFileOrDirectory(targetFile.FullName);
                }

                var metaFrom = fileInfo.FullName + ".meta";
                var metaTo = targetFile.FullName + ".meta";
                var animName = targetFile.FullName[..^4] + ".anim";

                if (File.Exists(metaTo))
                {
                    File.Delete(metaTo);
                }

                //copy fbx
                FileUtil.CopyFileOrDirectory(fileInfo.FullName, targetFile.FullName);
                //copy .meta
                FileUtil.CopyFileOrDirectory(metaFrom, metaTo);

                AssetDatabase.Refresh();

                if (fileInfo.Name.IndexOf("@", StringComparison.Ordinal) != -1)
                {
                    //动作文件，解析出animationClip保存，然后删除fbx源文件
                    var assets = AssetDatabase.LoadAllAssetsAtPath(FullPathToAssetPath(targetFile.FullName));
                    List<AnimationClip> clips = new();
                    foreach (var asset in assets)
                    {
                        if (asset is AnimationClip clip)
                        {
                            if (clip.name.StartsWith("__"))
                                continue;
                            var newClip = new AnimationClip();
                            EditorUtility.CopySerialized(clip, newClip);
                            clips.Add(newClip);
                            break;
                        }
                    }

                    FileUtil.DeleteFileOrDirectory(targetFile.FullName);
                    FileUtil.DeleteFileOrDirectory(metaTo);
                    AssetDatabase.Refresh();
                    foreach (var clip in clips)
                    {
                        if (clip.name.Equals("Attack"))
                        {
                            clip.name = clip.name.Replace("Attack", "PAttack");
                            animName = animName.Replace("@Attack", "@PAttack");
                        }

                        AssetDatabase.CreateAsset(clip, FullPathToAssetPath(animName));
                    }
                }
            }
        }
    }

    private void MoveResources(DirectoryInfo modelDir, string modelToResDir, string resName)
    {
        var fromDir = Path.Combine(modelDir.FullName, resName);
        var toToDir = Path.Combine(modelToResDir, resName);
        if (!Directory.Exists(fromDir))
        {
            Debug.Log($"{fromDir} Not Found, Skip...");
            return;
        }

        if (Directory.Exists(toToDir))
        {
            Debug.Log($"{toToDir} Existed, Skip...");
            return;
        }

        FileUtil.CopyFileOrDirectory(fromDir, toToDir);
        AssetDatabase.Refresh();
    }

    private void MovePrefab(string prefabsDir, string modelToResDir, NpcResourceData npcResourceData)
    {
        var preToDir = GetAssetsDir(Path.Combine(_prefabDir, npcResourceData.Id.ToString()));
        if (!preToDir.Exists)
        {
            preToDir.Create();
        }

        var aniDir = GetAssetsDir(Path.Combine(preToDir.FullName, "Animator"));
        if (!aniDir.Exists)
        {
            aniDir.Create();
        }

        var preSrcDir = new DirectoryInfo(prefabsDir);
        if (!preSrcDir.Exists)
        {
            Debug.LogError($"Prefab Dir {preSrcDir.FullName} Not Found");
            return;
        }

        foreach (var fileInfo in preSrcDir.GetFiles("*.prefab"))
        {
            var toName = fileInfo.Name;
            var isModelPrefab = false;
            if (fileInfo.Name.StartsWith(npcResourceData.Name))
            {
                isModelPrefab = true;
                toName = $"{npcResourceData.Id}.prefab";
            }

            var target = Path.Combine(preToDir.FullName, toName);
            if (File.Exists(target))
            {
                File.Delete(target);
            }

            FileUtil.CopyFileOrDirectory(fileInfo.FullName, target);
            AssetDatabase.Refresh();

            if (isModelPrefab)
            {
                var ap = FullPathToAssetPath(target);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(ap);

                var navMeshAgent = prefab.AddComponent<NavMeshAgent>();
                if (navMeshAgent != null)
                {
                    navMeshAgent.speed = 3.5f;
                    navMeshAgent.angularSpeed = 200;
                    navMeshAgent.acceleration = 8;
                    navMeshAgent.radius = 0.5f;
                }

                var inst = PrefabUtility.InstantiatePrefab(prefab) as GameObject;

                //增加HUDPos
                var skin = inst.GetComponentInChildren<SkinnedMeshRenderer>();
                var bounds = skin.sharedMesh.bounds;
                var center = bounds.center;
                var ext = bounds.extents;
                var up = inst.transform.up;

                //以renderer的坐标系，向上移动到边界
                up = skin.transform.InverseTransformDirection(up);
                up.Scale(ext);
                center += up;

                center = skin.transform.TransformPoint(center);

                var localTop = inst.transform.InverseTransformPoint(center);

                center.y += 0.2f;

                var hudPos = new GameObject("HUDPos");
                hudPos.transform.SetParent(inst.transform, false);
                hudPos.transform.position = center;
                hudPos.transform.forward = inst.transform.forward;

                //添加HitVfxPos
                var hitVfxPos = new GameObject("HitVfxPos");
                hitVfxPos.transform.SetParent(inst.transform, false);
                localTop.y /= 2;
                hitVfxPos.transform.position = localTop;
                hitVfxPos.transform.forward = inst.transform.forward;

                PrefabUtility.SaveAsPrefabAsset(inst, target);

                DestroyImmediate(hudPos);
                DestroyImmediate(hitVfxPos);

                inst.name = $"{npcResourceData.Id}-battle";
                var targetBattle = Path.Combine(preToDir.FullName, $"{npcResourceData.Id}-battle.prefab");

                //删掉战斗prefab上的navMeshAgent，增加BattleUnitAni
                navMeshAgent = inst.GetComponent<NavMeshAgent>();
                DestroyImmediate(navMeshAgent);

                //增加BattleUnitAni对象
                inst.AddComponent<BattleUnitAni>();

                //生成AnimatorController
                var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(_pathMapController);
                var overCon = new AnimatorOverrideController(controller);

                BindAniToController(overCon, mapClipName2Keywords, Path.Combine(modelToResDir, "FBX"),
                    npcResourceData.Name);

                var prefabAni = prefab.GetComponent<Animator>();
                prefabAni.runtimeAnimatorController = overCon;

                var ctrlFile = Path.Combine(aniDir.FullName,
                    $"{npcResourceData.Name.Replace(" ", "")}AniController.overrideController");

                if (File.Exists(ctrlFile))
                {
                    File.Delete(ctrlFile);
                    AssetDatabase.Refresh();
                }

                AssetDatabase.CreateAsset(overCon,
                    Path.Combine(FullPathToAssetPath(aniDir.FullName),
                        $"{npcResourceData.Name.Replace(" ", "")}AniController.overrideController"));

                controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(_pathBattleController);
                overCon = new AnimatorOverrideController(controller);

                BindAniToController(overCon, battleClipName2Keywords, Path.Combine(modelToResDir, "FBX"),
                    npcResourceData.Name);

                var instAni = inst.GetComponent<Animator>();
                instAni.runtimeAnimatorController = overCon;

                ctrlFile = Path.Combine(aniDir.FullName,
                    $"{npcResourceData.Name.Replace(" ", "")}AniBattleController.overrideController");

                if (File.Exists(ctrlFile))
                {
                    File.Delete(ctrlFile);
                    AssetDatabase.Refresh();
                }

                AssetDatabase.CreateAsset(overCon,
                    Path.Combine(FullPathToAssetPath(aniDir.FullName),
                        $"{npcResourceData.Name.Replace(" ", "")}AniBattleController.overrideController"));


                PrefabUtility.SaveAsPrefabAsset(inst, targetBattle);
                DestroyImmediate(inst);
            }
        }
    }

    private void BindAniToController(AnimatorOverrideController controller,
        Dictionary<string, ClipConfig> clipConfigMap, string modelDir, string modelName)
    {
        var dir = new DirectoryInfo(modelDir);
        var fbxFiles = dir.GetFiles("*.anim");

        foreach (var key in clipConfigMap.Keys)
        {
            var clipConfig = clipConfigMap[key];

            // var clipInfo = GetClipInfoFromFbxFiles(fbxFiles, clipConfig.Keywords);
            // if (clipInfo == null)
            // {
            //     Debug.LogError($"Bind Ani Error: ani clip {key} not found");
            //     continue;
            // }

            var clipAni =
                GetClipInfoFromAnimFiles(fbxFiles, clipConfig.Keywords, clipConfig.Strict); // clipInfo.ClipAnimation;

            if (clipAni == null)
            {
                if (clipConfig.BackupClip != null)
                {
                    var backClipConfig = clipConfigMap[clipConfig.BackupClip];
                    clipAni = GetClipInfoFromAnimFiles(fbxFiles, backClipConfig.Keywords, backClipConfig.Strict);
                }

                if (clipAni == null)
                {
                    Debug.LogError($"Bind Ani Error: model[{modelDir}] ani clip to {key} not found");
                    continue;
                }
                else
                {
                    //copy clipAni to new file
                    var clipName = $"{modelName}@{clipConfig.ClipName}";
                    var p = Path.Combine(FullPathToAssetPath(modelDir), clipName + ".anim");
                    clipAni = Instantiate(clipAni);
                    clipAni.name = clipName;
                    AssetDatabase.CreateAsset(clipAni, p);
                    clipAni = AssetDatabase.LoadAssetAtPath<AnimationClip>(p);
                }
            }

            var clipSetting = AnimationUtility.GetAnimationClipSettings(clipAni);
            clipSetting.loopTime = clipConfig.IsLoop;
            AnimationUtility.SetAnimationClipSettings(clipAni, clipSetting);

            var evt = clipConfig.Event;
            if (evt != null)
            {
                AnimationUtility.SetAnimationEvents(clipAni, new[] { clipConfig.Event });
                // clipAni.events = new[] { clipConfig.Event };
                // var import = clipInfo.Importer;
                // import.clipAnimations = new[] { clipAni };
                // import.SaveAndReimport();
            }

            // var fbx = FullPathToAssetPath(clipInfo.FbxFile.FullName);
            // var assets = AssetDatabase.LoadAllAssetsAtPath(fbx);
            // foreach (var asset in assets)
            // {
            //     if (asset is AnimationClip clip && clip.name == clipInfo.ClipAnimation.name)
            //     {
            //         controller[key] = clip;
            //     }
            // }

            // if (clipAni.name == clipInfo.ClipAnimation.name)
            // {
            controller[key] = clipAni;
            // }
        }
    }

    private AnimationClip GetClipInfoFromAnimFiles(FileInfo[] animFiles, string[] keywords, bool strict)
    {
        foreach (var keyword in keywords)
        {
            foreach (var fileInfo in animFiles)
            {
                var assetPath = FullPathToAssetPath(fileInfo.FullName);

                var clipInfo = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
                //var importer = (ModelImporter)AssetImporter.GetAtPath(assetPath);

                // foreach (var clipInfo in importer.defaultClipAnimations)
                // {
                var idx = clipInfo.name.IndexOf("@", StringComparison.Ordinal);
                var clipInfoName = idx > 0 ? clipInfo.name[(idx + 1)..] : clipInfo.name;
                if (strict)
                {
                    if (clipInfoName.Equals(keyword))
                    {
                        return clipInfo;
                    }
                }
                else
                {
                    if (clipInfoName.Contains(keyword))
                    {
                        return clipInfo;
                    }
                }
                // }
            }
        }

        return null;
    }

    private FbxClipInfo GetClipInfoFromFbxFiles(FileInfo[] fbxFiles, string[] keywords)
    {
        foreach (var fileInfo in fbxFiles)
        {
            var assetPath = FullPathToAssetPath(fileInfo.FullName);
            var importer = (ModelImporter)AssetImporter.GetAtPath(assetPath);

            foreach (var keyword in keywords)
            {
                foreach (var clipInfo in importer.defaultClipAnimations)
                {
                    var idx = clipInfo.name.IndexOf("@", StringComparison.Ordinal);
                    var clipInfoName = idx > 0 ? clipInfo.name[idx..] : clipInfo.name;
                    if (clipInfoName.Contains(keyword))
                    {
                        return new FbxClipInfo(fileInfo, importer, clipInfo);
                    }
                }
            }
        }

        return null;
    }

    private string FullPathToAssetPath(string fullpath)
    {
        var p = fullpath;
        var idx = fullpath.IndexOf("Assets", StringComparison.Ordinal);
        if (idx > 0)
        {
            p = p.Substring(idx);
        }

        return p;
    }

    private DirectoryInfo GetAssetsDir(string dir)
    {
        //return dir.StartsWith("Assets") ? new DirectoryInfo(dir) : new DirectoryInfo(Path.Combine("Assets", dir));
        return new DirectoryInfo(dir);
    }
}