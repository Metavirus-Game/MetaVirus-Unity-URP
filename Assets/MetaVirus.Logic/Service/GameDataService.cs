﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bright.Serialization;
using cfg;
using cfg.attr;
using cfg.avatar;
using cfg.battle;
using cfg.common;
using cfg.map;
using cfg.skill;
using GameEngine;
using GameEngine.Base;
using GameEngine.Base.Exception;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Exception;
using SimpleJSON;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service
{
    [CreateAssetMenu(menuName = "GameEngine/LoadGameData")]
    public class GameDataService : BaseService
    {
#if UNITY_EDITOR
        private static GameDataService _inst;

        public static GameDataService EditorInst
        {
            get
            {
                if (_inst == null)
                {
                    var go = new GameObject
                    {
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    _inst = go.AddComponent<GameDataService>();
                    _inst.LoadGameDataFromDisk();
                }

                return _inst;
            }
        }

        [MenuItem("GameEngine/LoadGameData")]
        public static void LoadGameData()
        {
            EditorInst.LoadGameDataFromDisk();
        }
#endif

        public Tables gameTable { get; private set; }

        /// <summary>
        /// 仅用于技能演示时候，自定义技能使用
        /// </summary>
        private Dictionary<int, BattleSkillData> _customSkillMap = new();

        public override void ServiceReady()
        {
            Event.On<string>(GameEvents.GameEvent.GameDataUpdated, OnGameDataTableUpdated);
        }

        private void OnGameDataTableUpdated(string args)
        {
            LoadGameDataAsync();
        }

#if UNITY_EDITOR
        public void LoadGameDataFromDisk()
        {
            var gameDatas = new Dictionary<string, byte[]>();
            var dir = new DirectoryInfo("Assets/MetaVirus.Res/GameDatas");
            if (dir.Exists)
            {
                var fs = dir.GetFiles("*.bytes");
                foreach (var file in fs)
                {
                    var bytes = File.ReadAllBytes(file.FullName);
                    gameDatas[file.Name] = bytes;
                }

                //加载所有数据
                gameTable = new Tables(file =>
                {
                    var f = $"{file}.bytes";
                    gameDatas.TryGetValue(f, out var data);
                    if (data != null) return ByteBuf.Wrap(data);
                    Debug.LogError($"GameData File [{file}] Not Found!");
                    return null;
                });

                //EditorUtility.DisplayDialog("Message", "Data Load Completed", "ok");
            }
        }
#endif

        public async Task<Tables> LoadGameDataAsync()
        {
            var loadPath = ""; 
#if UNITY_STANDALONE_WIN
            var args = Environment.GetCommandLineArgs();
            if (args.Length >= 3)
            {
                if (args[1].ToLower() == "-p")
                {
                    if (Directory.Exists(args[2]))
                    {
                        loadPath = args[2];
                    }
                }
            }
#endif

            if (string.IsNullOrEmpty(loadPath))
            {
                //从addressables资源中读取
                var locations = await Addressables
                    .LoadResourceLocationsAsync(ResAddress.GameDatas).Task;

                var gameDatas = new Dictionary<string, TextAsset>();

                foreach (var l in locations)
                {
                    var asset = await Addressables.LoadAssetAsync<TextAsset>(l).Task;
                    var fileName = l.PrimaryKey;
                    fileName = fileName.Substring(fileName.LastIndexOf("/", StringComparison.Ordinal) + 1);
                    gameDatas[fileName] = asset;
                }


                //加载所有数据
                gameTable = new Tables(file =>
                {
                    var f = $"{file}.bytes";
                    gameDatas.TryGetValue(f, out var data);
                    if (data != null) return ByteBuf.Wrap(data.bytes);
                    Debug.LogError($"GameData File [{file}] Not Found!");
                    return null;
                });
            }
            else
            {
                LoadDataFromLocalPath(loadPath);
                StartCoroutine(MonitorLoadPath(loadPath));
            }

            // var str =
            //     "H4sIAAAAAAAAAL2WQUgUYRTH9/u+cfv8SHdcTEUxJgla5jAOnkSIdI2wg7IglEEdZnfHdWl1l90V7OaMs0NKUgkFHTp18BBFR/HQKQgiCjpZp04eokMduon1ZmbXxonVlWZ2YC7v7f7e4/9/7/uGIn6p9wbVljl+lfZgYUgc3CEfOBOltmnqEfqMP2IWavlx/p5kvZ7ws66t7tAQCx08I1incUQmQ6HlSwk8s0oBrQN6nfcBvQJoXEOv84A2AL3pR9drgEY19KbVdQXQuxEf0Pfd6N0IoE1AG34IornRBj+gQNeUxz1EYOLga7SHLzzF29wefouTDxBj7Hv7Tr/1esI/Oy1y52P0bf+386CDGog4modGE2hmY0pMsqvlOwVVyuTzmZyqFLIlKZWfH5xUy8q1bHGxJCWVcjmnSkU1lS+mpUQxX1CL5axaGp9TFjLqWKqczS9cKSrzaiIZPUM5fqkHlB5ooX38GzTA0X4eiTf9rBGxa5h2jXb+riTOsvhJ6NO3s7ncuFIqH8YKFDlYQaMxNNTBIrb2CFxti73nZByEUmYTlNKCUQqwgk5jpElKaV6lAqhhNMENPRg3ACus0Bhukhu6V6lbftbgocZXAscItaXa6Ren2cWT4OOLs7OHkVFKLCQSNqZiwJXJMBMfcr4Y8I6AA067AgEHroMD1g3pOPAFyzhaCxAIPEcQ6Kh6FIbAEySjYdQr0DD/ySTwJ0jF4PcQxaNkoi2RniMFvIRscMULrjQAPuUCV+qArSuM2OA9B2w2AKYusFkHrHk71hoAt7rAWh2w4QUbDYCZC2z8C/Z/V6pzzMGyjAW1K69aq0eXfazM/O+udMOuAJIJIJUtEgcitU2kxV/Il4V5iWBhnJ6FNeuuPctO/x0TGUXbq1ZazqVlJ28ck9ePyVeOyZv182LKT8O63Ib18S+wPRU4kKmoXTaGf1MRtqdCd01FxpehOOfMxFH3WBA26M2wwfTfBmrbYAZlw1GfwUHYYHpskLe6R8KX09YTJ5Ik/QGEnPEb1A4AAA==";
            // var bytes = Convert.FromBase64String(str);
            //
            // var br = BattleRecord.FromGZipData(bytes);
            // Debug.Log(br);

            return gameTable;
        }

        private void LoadDataFromLocalPath(string loadPath)
        {
            //本地数据中读取，仅限测试使用
            gameTable = new Tables(file =>
            {
                var f = $"{file}.bytes";
                f = Path.Combine(loadPath, f);
                var data = File.ReadAllBytes(f);
                return ByteBuf.Wrap(data);
            });
        }

        private IEnumerator MonitorLoadPath(string loadPath)
        {
            var reloadFile = Path.Combine(loadPath, "reload");
            while (true)
            {
                if (File.Exists(reloadFile))
                {
                    LoadDataFromLocalPath(loadPath);
                    File.Delete(reloadFile);
                }

                yield return new WaitForSeconds(1);
            }
        }

        public CommonConfig CommonConfig => gameTable.CommonConfigs.DataList[0];

        public float BattleProjectileSpeed(float speedFactor)
        {
            return CommonConfig.BattleProjectileSpeed * speedFactor;
        }

        public static Color IntArrayToColor(int[] color)
        {
            return color.Length switch
            {
                3 => new Color32((byte)color[0], (byte)color[1], (byte)color[2], 255),
                4 => new Color32((byte)color[0], (byte)color[1], (byte)color[2], (byte)color[3]),
                _ => Color.white
            };
        }

        public Color BattleUIHpBarColor => IntArrayToColor(CommonConfig.BattleUiHpBarCololr);
        public Color BattleUIMpBarColor => IntArrayToColor(CommonConfig.BattleUiMpBarCololr);

        public Color BattleColorHpInc()
        {
            return IntArrayToColor(CommonConfig.BattleColorHpInc);
        }

        public Color BattleColorHpDec()
        {
            return IntArrayToColor(CommonConfig.BattleColorHpDec);
        }

        public string GetSkillAttackTargetDesc(SkillScope scope, AtkTarget target)
        {
            var keyAll = "battle.skill.desc.ally.all";
            var keyOne = "battle.skill.desc.ally.one";

            if (target == AtkTarget.Enemy)
            {
                keyAll = "battle.skill.desc.enemy.all";
                keyOne = "battle.skill.desc.enemy.one";
            }


            var tarKey = scope == SkillScope.All
                ? keyAll
                : keyOne;

            var str = GetLocalizeStr(tarKey);
            return str;
        }

        public string GetAtkAttributeName(AtkAttribute atkAttribute)
        {
            return GetLocalizeStr("battle.atkAttribute.name." + (int)atkAttribute);
        }

        public string GetSkillLevelName(string name, int level, bool bold = true, string clrString = "")
        {
            var map = new Dictionary<string, string>
            {
                [LocalizeVarNames.Battle.Level] = level.ToString(),
                [LocalizeVarNames.Battle.Name] = name
            };
            var skillName = GetLocalizeStr("battle.skill.levelname", map);
            if (bold)
            {
                skillName = "[b]" + skillName + "[/b]";
            }

            if (!string.IsNullOrEmpty(clrString))
            {
                skillName = $"[color={clrString}]{skillName}[/color]";
            }

            return skillName;
        }

        public string GetGameMesasge(int messageId)
        {
            var data = gameTable.GameMessages.GetOrDefault(messageId);

            var str = Localize.CurrLangauge switch
            {
                LocalizeService.Language.En => data?.TextEn ?? "",
                LocalizeService.Language.Cn => data?.TextCn ?? "",
                _ => ""
            };

            return str;
        }

        public string GetLocalizeStr(string key, Dictionary<string, string> replaceMap = null)
        {
            var data = gameTable.LocalizationDatas.GetOrDefault(key);
            string str;
            if (data == null)
            {
                str = "";
            }
            else
            {
                switch (Localize.CurrLangauge)
                {
                    case LocalizeService.Language.En:
                        str = data.TextEn;
                        break;
                    case LocalizeService.Language.Cn:
                    default:
                        str = data.TextCn;
                        break;
                }
            }

            if (replaceMap != null)
            {
                foreach (var replaceKey in replaceMap.Keys)
                {
                    str = str.Replace(replaceKey, replaceMap[replaceKey]);
                }
            }

            return str;
        }

        public LevelUpTable QualityToLevelUpTable(Quality quality)
        {
            var q = (int)quality;

            var tableId = 2;

            if (q < CommonConfig.MonsterLevelupTable.Length)
            {
                tableId = CommonConfig.MonsterLevelupTable[q];
            }

            return gameTable.LevelUpTables.Get(tableId);
        }


        public Color QualityToColor(Quality quality)
        {
            int[] clr;
            switch (quality)
            {
                case Quality.GREEN:
                    clr = CommonConfig.ItemQualityColorGreen;
                    break;
                case Quality.BLUE:
                    clr = CommonConfig.ItemQualityColorBlue;
                    break;
                case Quality.PURPLE:
                    clr = CommonConfig.ItemQualityColorPurple;
                    break;
                case Quality.Orange:
                    clr = CommonConfig.ItemQualityColorOrange;
                    break;
                case Quality.Red:
                    clr = CommonConfig.ItemQualityColorRed;
                    break;
                case Quality.WHITE:
                default:
                    clr = CommonConfig.ItemQualityColorWhite;
                    break;
            }

            return IntArrayToColor(clr);
        }

        /// <summary>
        /// 返回所有通用特效的id
        /// </summary>
        public int[] ConfigCommonVfxIds
        {
            get
            {
                var vfxIds = new SortedSet<int>();
                var commonVfxIds = CommonConfig.VfxCommon;
                for (var c = 0; c < commonVfxIds.Length; c += 2)
                {
                    if (c + 1 >= commonVfxIds.Length)
                    {
                        break;
                    }

                    var s = commonVfxIds[c];
                    var e = commonVfxIds[c + 1];

                    for (var vfxId = s; vfxId <= e; vfxId++)
                    {
                        vfxIds.Add(vfxId);
                    }
                }

                return vfxIds.ToArray();
            }
        }


        /// <summary>
        /// 返回战斗单位所有技能引用到的特效id
        /// </summary>
        /// <returns></returns>
        public int[] GetAllSkillVfxIds(BattleUnit unit)
        {
            var idSet = new SortedSet<int>();

            var ids = GetSkillVfxIds(unit.AtkSkills);
            idSet.AddRange(ids);

            return idSet.ToArray();
        }

        private static IEnumerable<int> GetSkillVfxIds(BattleSkillData[] skillDatas)
        {
            var idSet = new HashSet<int>();

            foreach (var skillData in skillDatas)
            {
                idSet.Add(skillData.StartAction.AttachVfx);
                idSet.Add(skillData.MoveAction.AttachVfx);
                idSet.Add(skillData.CastAction.AttachVfx);
                idSet.Add(skillData.CastAction.HitVfx);
                idSet.Add(skillData.CastAction.GlobalVfx);
                idSet.Add(skillData.CastAction.Projectile_Ref.MuzzleVfx);
                idSet.Add(skillData.CastAction.Projectile_Ref.ProjectileVfx);
                idSet.Add(skillData.CastAction.Projectile_Ref.HitVfx);

                foreach (var levelInfo in skillData.LevelInfo)
                {
                    foreach (var eff in levelInfo.AttachEffects)
                    {
                        idSet.Add(eff.HitVfx);
                    }

                    foreach (var buff in levelInfo.AttachBuffs)
                    {
                        idSet.Add(buff.BuffId_Ref.AttachVfx);
                        idSet.Add(buff.BuffId_Ref.EffectVfx);
                    }
                }
            }

            return idSet.ToArray();
        }

        public MapData GetMapData(int mapId)
        {
            gameTable.MapDatas.DataMap.TryGetValue(mapId, out var mapData);
            if (mapData == null)
            {
                throw new MapDataNotFoundException(mapId);
            }

            return mapData;
        }

        public NpcResourceData GetResourceData(int resId)
        {
            gameTable.NpcResourceDatas.DataMap.TryGetValue(resId, out var res);
            return res;
        }

        public MonsterData GetMonsterData(int monsterId)
        {
            gameTable.MonsterDatas.DataMap.TryGetValue(monsterId, out var md);
            return md;
        }

        public MonsterGroupData GetMonsterGroupData(int monsterGroupId)
        {
            gameTable.MonsterGroupDatas.DataMap.TryGetValue(monsterGroupId, out var mg);
            return mg;
        }

        public MonsterGroupData[] GetAllMonsterGroups()
        {
            return gameTable.MonsterGroupDatas.DataList.ToArray();
        }

        public void AddCustomSkillData(int skillId)
        {
        }

        public BattleSkillData GetSkillData(int skillId)
        {
            gameTable.SkillDatas.DataMap.TryGetValue(skillId, out var skill);
            return skill;
        }

        public SkillInfo GetSkillData(int skillId, int level)
        {
            return new SkillInfo(GetSkillData(skillId), level);
        }

        public BuffInfo GetBuffData(int buffId, int level)
        {
            gameTable.BuffDatas.DataMap.TryGetValue(buffId, out var buffData);
            if (buffData == null)
            {
                return null;
            }

            return new BuffInfo(buffData, level);
        }

        public VFXData GetVfxData(int vfxId)
        {
            gameTable.VFXDatas.DataMap.TryGetValue(vfxId, out var vfxData);
            return vfxData;
        }

        public List<PetData> GetPetsByType(MonsterType type)
        {
            var list = gameTable.PetDatas.DataList.FindAll(p => p.Type == type.Id);
            return list;
        }


        public AvatarSenseData GetAvatarSenseData(int partId, HeadWearType part)
        {
            return gameTable.AvatarSenseDatas.DataList.FirstOrDefault(
                data => data.PartId == partId && data.Part == part);
        }

        /// <summary>
        /// 从本地化数据 Localization.xlsx 中读取对应key的值
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <param name="replaceMap"></param>
        /// <returns></returns>
        public static string LT(string key, Dictionary<string, string> replaceMap = null)
        {
            return GameFramework.GetService<GameDataService>().GetLocalizeStr(key, replaceMap);
        }

        /// <summary>
        /// 从本地化消息 GameMessages.xlsx 中读取对应messageId的值
        /// </summary>
        /// <param name="messageId"></param>
        /// <returns></returns>
        public static string LM(int messageId)
        {
            return GameFramework.GetService<GameDataService>().GetGameMesasge(messageId);
        }
    }
}