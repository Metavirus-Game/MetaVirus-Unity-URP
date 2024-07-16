using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using FairyGUI;
using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.Config;
using GameEngine.DataNode;
using GameEngine.Event;
using GameEngine.FairyGUI;
using GameEngine.Fsm;
using GameEngine.Procedure;
using GameEngine.Resource;
using GameEngine.Sound;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Procedures.fortest;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Npc;
using MetaVirus.Logic.Service.NpcHeader;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using TMPro;
using UnityEngine;

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

        public GComponent EntryPageCom { get; private set; }
        private GProgressBar _loadingProgress;

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _fairyService = GameFramework.GetService<FairyGUIService>();
            _dataService = GameFramework.GetService<DataNodeService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
            _soundService = GameFramework.GetService<SoundService>();
            GameFramework.GetService<YooAssetsService>();
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
            GameFramework.GetService<NpcHeaderService>();
            GameFramework.GetService<UpdateService>();
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
            GameFramework.GetService<EventService>().Emit(GameEvents.GameEvent.OpenGame);
            var yooAssetService = GameFramework.GetService<YooAssetsService>();

            var task = yooAssetService.InitializeAsync();
            yield return task.ToCoroutine();
            var package = yooAssetService.GetPackage();

            var assetHandle =
                package.LoadAssetAsync<TMP_FontAsset>("Assets/MetaVirus.Res/Fonts/JosefinSans-Bold.asset");
            yield return assetHandle;

            var asset = assetHandle.GetAssetObject<TMP_FontAsset>();

            // var fontTask = Addressables.LoadAssetAsync<TMP_FontAsset>("UI/Fonts/JosefinSans-Bold.asset").Task;
            // yield return fontTask.AsCoroution();

            //加载英文及数字字体
            var tmpFont = new TMPFont
            {
                name = Constants.EnJosefinSans,
                fontAsset = asset
            };

            FontManager.RegisterFont(tmpFont);

            assetHandle = package.LoadAssetAsync<TMP_FontAsset>("Assets/MetaVirus.Res/Fonts/LilitaOne-Regular.asset");
            yield return assetHandle;

            asset = assetHandle.AssetObject as TMP_FontAsset;
            tmpFont = new TMPFont
            {
                name = Constants.EnLilitaOne,
                fontAsset = asset
            };

            FontManager.RegisterFont(tmpFont);
            Debug.Log("Fonts Loaded");

            //mainpage资源在Entry时加载，在离开MainPageProcedure时释放
            var mt = _fairyService.AddPackageAsync("MainPage");
            yield return mt.AsCoroution();

            EntryPageCom = UIPackage.CreateObject("MainPage", "EntryPage").asCom;
            _fairyService.AddToGRootFullscreen(EntryPageCom);

            _loadingProgress = EntryPageCom.GetChild("loadingProgress").asProgress;
            _loadingProgress.value = 10;

            //加载common ui资源
            var ret = _fairyService.AddPackageAsync("Common");
            yield return ret.AsCoroution();
            Debug.Log("Common UI Loaded");
            _loadingProgress.value = 20;

            //加载游戏数据
            var t = _gameDataService.LoadGameDataAsync();

            while (!t.IsCompleted)
            {
                if (_loadingProgress.value < 65)
                {
                    _loadingProgress.value += 10 * Time.deltaTime;
                }

                yield return null;
            }

            _loadingProgress.value = 65;
            Debug.Log("GameData Loaded");

            //加载gameitem icons资源
            ret = _fairyService.AddPackageAsync("GameItem");
            yield return ret.AsCoroution();
            _loadingProgress.value = 70;

            Debug.Log("Gameitem Icons Loaded");

            //加载怪物头像
            ret = _fairyService.AddPackageAsync("UnitPortraits");
            yield return ret.AsCoroution();

            _loadingProgress.value = 80;
            Debug.Log("Portrait UI Loaded");

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
                    yield return audioTask.ToCoroutine();
                }
            }


            _loadingProgress.value = 95;

            // var st = _soundService.AsyncLoadSoundClip("BGM/场景背景音乐", "BGM/Scene/bgm_fantasy.mp3", 1, true);
            // yield return st.AsCoroution();
            // _soundService.UnloadSoundClip("BGM", "场景背景音乐");

            Debug.Log("Audios Loaded");

            //通用设置
            UIConfig.bringWindowToFrontOnClick = false;
            GRoot.inst.SetContentScaleFactor(1080, 1920, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);

            GameFramework.GetService<EventService>().Emit(GameEvents.ResourceEvent.AllResLoaded);

            _loadingProgress.value = 100;
        }

        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
            ChangeProcedure(NextProcedure);
        }

        public override void OnLeave(FsmEntity<ProcedureService> fsm, bool isShutdown)
        {
            GRoot.inst.RemoveChild(EntryPageCom, true);
        }

        public override void OnDestroy(FsmEntity<ProcedureService> fsm)
        {
            _fairyService.ReleasePackages(_loadedPkgs);
        }
    }
}