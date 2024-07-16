using System;
using System.Collections.Generic;
using System.Linq;
using GameEngine.Base;
using GameEngine.Base.Attributes;
using GameEngine.Common;
using UnityEngine;

namespace GameEngine
{
    [ServicePriority(EngineConsts.ServicePriorityValue.LocalizeService)]
    public class LocalizeService : BaseService
    {
        public enum Language
        {
            Cn,
            En
        }

        public Language CurrLangauge => _currLanguage;

        private Language _currLanguage = Language.En;

        //public TextAsset[] localizations;

        private readonly Dictionary<string, Sprite[]> _spriteMap = new Dictionary<string, Sprite[]>();

        private class LanguageTable
        {
            private readonly Dictionary<string, string> _table = new Dictionary<string, string>();

            public string Get(string key, string defaultValue = "")
            {
                return _table.ContainsKey(key) ? _table[key] : defaultValue;
            }

            public void Add(string key, string value)
            {
                if (_table.ContainsKey(key))
                {
                    Debug.LogWarning($"language key[{key}] already exist");
                }
                else
                {
                    _table.Add(key, value);
                }
            }
        }

        private Dictionary<Language, LanguageTable> langTables = new Dictionary<Language, LanguageTable>();

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        public void ChangeLanguage(Language language)
        {
            _currLanguage = language;
            Event.Emit(Events.Localize.LanguageChanged, language);
        }

        private void LoadLangSprites(string abName)
        {
            // ResService.LoadAsset<Sprite>(abName, null, objs =>
            // {
            //     List<Sprite> sprites = new List<Sprite>();
            //     foreach (var obj in objs)
            //     {
            //         sprites.Add(obj as Sprite);
            //     }
            //
            //     _spriteMap.Add(abName, sprites.ToArray());
            // });
        }

        /// <summary>
        /// TODO 换到GameData中
        /// </summary>
        /// <param name="language"></param>
        /// <param name="assetAddress"></param>
        private void LoadLangText(Language language, string assetAddress)
        {
            // var handle = Addressables.LoadAssetAsync<TextAsset>(assetAddress);
            // handle.Completed += operationHandle =>
            // {
            //     var txt = operationHandle.Result.text;
            //     var lt = new LanguageTable();
            //     var lines = txt.Split('\n');
            //     foreach (var line in lines)
            //     {
            //         var l = line;
            //         if (line.Trim().StartsWith("#")) continue;
            //         if (l.EndsWith("\r"))
            //         {
            //             l = l.Substring(0, l.Length - 1);
            //         }
            //
            //         var kv = l.Split('=');
            //         if (kv.Length != 2) continue;
            //         lt.Add(kv[0].Trim(), kv[1].Trim());
            //     }
            //
            //     langTables.Add(language, lt);
            //     Addressables.Release(handle);
            // };
        }

        private void LoadLangTexts()
        {
            LoadLangText(Language.Cn, "Assets/MetaVirus.Res/Localized/local_cn.txt");
            LoadLangText(Language.En, "Assets/MetaVirus.Res/Localized/local_en.txt");

            // var handle = Addressables.LoadAssetAsync<TextAsset>("Assets/MetaVirus.Res/Localized/local_cn.txt");
            // handle.Completed += operationHandle =>
            // {
            //     var txt = operationHandle.Result.text;
            //     var lt = new LanguageTable();
            //     string[] lines = txt.Split('\n');
            //     foreach (var line in lines)
            //     {
            //         string l = line;
            //         if (line.Trim().StartsWith("#")) continue;
            //         if (l.EndsWith("\r"))
            //         {
            //             l = l.Substring(0, l.Length - 1);
            //         }
            //
            //         var kv = l.Split('=');
            //         if (kv.Length != 2) continue;
            //         lt.Add(kv[0].Trim(), kv[1].Trim());
            //     }
            //
            //     langTables.Add(_currLanguage, lt);
            //     Addressables.Release(handle);
            // };

            // ResService.LoadAsset<TextAsset>("localize", new[] { "local_cn", "local_en" }, objs =>
            // {
            //     for (int i = 0; i < objs.Length; i++)
            //     {
            //         LanguageTable lt = new LanguageTable();
            //         var asset = objs[i] as TextAsset;
            //         if (asset == null) continue;
            //         var txt = asset.text;
            //         string[] lines = txt.Split('\n');
            //         foreach (var line in lines)
            //         {
            //             string l = line;
            //             if (line.Trim().StartsWith("#")) continue;
            //             if (l.EndsWith("\r"))
            //             {
            //                 l = l.Substring(0, l.Length - 1);
            //             }
            //
            //             string[] kv = l.Split('=');
            //             if (kv.Length != 2) continue;
            //             lt.Add(kv[0].Trim(), kv[1].Trim());
            //         }
            //
            //         Language lang = (Language)i;
            //         langTables.Add(lang, lt);
            //     }
            // });
        }

        public override void PostConstruct()
        {
            LoadLangSprites("ui_lang_1");
            LoadLangTexts();
        }

        public override void ServiceReady()
        {
            ChangeLanguage(_currLanguage);
        }

        /// <summary>
        /// 获取一张对应语言的图片字，lang = "cn" or "en"
        /// 图片字sprite的命名规则为 spriteName + "_" + currLanguage
        /// </summary>
        /// <param name="abName"></param>
        /// <param name="spriteName"></param>
        /// <returns></returns>
        public Sprite GetLanguageSprite(string abName, string spriteName)
        {
            if (_spriteMap.ContainsKey(abName))
            {
                var sprites = _spriteMap[abName];
                var spriteFullname = spriteName + "_" + _currLanguage;
                return sprites.FirstOrDefault(sprite => sprite.name == spriteFullname);
            }

            return null;
        }

        [Obsolete("不要用这个方法了，以后会去掉，本地化都改到GameDataService中了 ")]
        public string GetLanguageText(string key, string defaultValue = "")
        {
            var lt = langTables[_currLanguage];
            return lt.Get(key, defaultValue);
        }
    }
}