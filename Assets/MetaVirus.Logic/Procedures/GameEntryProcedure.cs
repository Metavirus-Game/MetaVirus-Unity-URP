using System;
using System.Collections;
using FairyGUI;
using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.Config;
using GameEngine.DataNode;
using GameEngine.Event;
using GameEngine.FairyGUI;
using GameEngine.Fsm;
using GameEngine.Procedure;
using GameEngine.Sound;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Procedures.fortest;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Battle.UI;
using MetaVirus.Logic.Service.Npc;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MetaVirus.Logic.Procedures
{
    [Procedure(true, "GameMode")]
    public class GameEntryProcedure : ProcedureBase
    {
        private FairyGUIService _fairyService;
        private DataNodeService _dataService;
        private GameDataService _gameDataService;
        private SoundService _soundService;

        private string[] _loadedPkgs;

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _fairyService = GameFramework.GetService<FairyGUIService>();
            _dataService = GameFramework.GetService<DataNodeService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
            _soundService = GameFramework.GetService<SoundService>();
            GameFramework.GetService<GameDataService>();
            GameFramework.GetService<NpcRefreshService>();
            GameFramework.GetService<NpcMapUIService>();
            GameFramework.GetService<BattleService>();
            GameFramework.GetService<PositionService>();
            GameFramework.GetService<GridItemRefreshService>();
            GameFramework.GetService<StageUIService>();
            GameFramework.GetService<UIService>();
            GameFramework.GetService<PlayerService>();
            GameFramework.GetService<ArenaService>();
        }


        /// <summary>
        /// EnterProcedure处理完毕后的下一个Procedure类型
        /// </summary>
        private Type NextProcedure
        {
            get
            {
                return GameConfig.Inst.NextEnterProcedure switch
                {
                    NextEnterProcedure.BattleTestProcedure => typeof(EnterBattleTestProcedure),
                    NextEnterProcedure.UITestProcedure => typeof(UITestProcedure),
                    NextEnterProcedure.MonsterTestProcedure => typeof(EnterMonsterTestProcedure),
                    _ => typeof(MainPageProcedure)
                };
            }
        }

        public override IEnumerator OnPrepare(FsmEntity<ProcedureService> fsm)
        {
            //加载游戏数据
            var t = _gameDataService.LoadGameDataAsync();
            yield return t.AsCoroution();

            Debug.Log("GameData Loaded");

            //加载common ui资源
            var ret = _fairyService.AddPackageAsync("ui-common");
            yield return ret.AsCoroution();

            Debug.Log("Common UI Loaded");

            //加载gameitem icons资源
            ret = _fairyService.AddPackageAsync("ui-gameitem-icons");
            yield return ret.AsCoroution();

            Debug.Log("Gameitem Icons Loaded");

            //加载怪物头像
            ret = _fairyService.AddPackageAsync("ui-portrait");
            yield return ret.AsCoroution();


            Debug.Log("Portrait UI Loaded");

            var fontTask = Addressables.LoadAssetAsync<TMP_FontAsset>("UI/Fonts/JosefinSans-Bold.asset").Task;
            yield return fontTask.AsCoroution();

            //加载英文及数字字体
            var tmpFont = new TMPFont
            {
                name = Constants.EnJosefinSans,
                fontAsset = fontTask.Result
            };

            FontManager.RegisterFont(tmpFont);

            Debug.Log("Fonts Loaded");

            _loadedPkgs = ret.Result;

            if (ret.Exception != null)
            {
                Debug.LogError(ret.Exception);
            }

            //加载音频数据
            foreach (var catalog in _gameDataService.gameTable.AudioCatalogs.DataList)
            {
                if (catalog.Id == 0) continue;
                _soundService.AddCatalog(catalog.Name, catalog.Priority, catalog.DecreasePercent);
            }

            foreach (var audioConfig in _gameDataService.gameTable.AudioConfigs.DataList)
            {
                if (audioConfig.AutoLoad)
                {
                    //自动加载
                    var audioTask = _soundService.AsyncLoadSoundClip(audioConfig.Catalog_Ref.Name, audioConfig.Name,
                        audioConfig.AssetAddress,
                        audioConfig.Volume, audioConfig.Loop);
                    yield return audioTask.AsCoroution();
                }
            }

            // var st = _soundService.AsyncLoadSoundClip("BGM/场景背景音乐", "BGM/Scene/bgm_fantasy.mp3", 1, true);
            // yield return st.AsCoroution();
            // _soundService.UnloadSoundClip("BGM", "场景背景音乐");

            Debug.Log("Audios Loaded");

            //通用设置
            UIConfig.bringWindowToFrontOnClick = false;
            GRoot.inst.SetContentScaleFactor(1080, 1920, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);

            GameFramework.GetService<EventService>().Emit(GameEvents.ResourceEvent.AllResLoaded);

            ChangeProcedure(NextProcedure);
        }

        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
        }

        public override void OnLeave(FsmEntity<ProcedureService> fsm, bool isShutdown)
        {
        }

        public override void OnDestroy(FsmEntity<ProcedureService> fsm)
        {
            _fairyService.ReleasePackages(_loadedPkgs);
        }
    }
}