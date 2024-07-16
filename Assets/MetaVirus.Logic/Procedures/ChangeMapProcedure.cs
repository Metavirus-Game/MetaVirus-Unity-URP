using System;
using System.Collections;
using cfg.map;
using FairyGUI;
using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.DataNode;
using GameEngine.Event;
using GameEngine.FairyGUI;
using GameEngine.Fsm;
using GameEngine.Procedure;
using GameEngine.Resource;
using GameEngine.Sound;
using GameEngine.Utils;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Exception;
using MetaVirus.Logic.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Procedures
{
    /**
     * 负责切换地图使用
     */
    [Procedure]
    public class ChangeMapProcedure : ProcedureBase
    {
        private const float LoadingFadeDuration = 0.5f;

        private FairyGUIService _fairyService;
        private DataNodeService _dataService;
        private GameDataService _gameDataService;
        private SoundService _soundService;
        private EventService _eventService;
        private YooAssetsService _yooAssetsService;

        private LoadingPage _loadingPage;

        // private AsyncOperationHandle<SceneInstance> _handle;
        private SceneHandle _handle;

        private int _toMapId;
        private Type _toProcedure;
        private Type _fromProcedure;
        private Vector3 _toPosition;

        private string[] _loadedPkgs;

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _fairyService = GameFramework.GetService<FairyGUIService>();
            _dataService = GameFramework.GetService<DataNodeService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
            _eventService = GameFramework.GetService<EventService>();
            _soundService = GameFramework.GetService<SoundService>();
            _yooAssetsService = GameFramework.GetService<YooAssetsService>();
        }

        public void SetChangeMapInfo(int toMapId, Type fromProcedure, Type toProcedure, Vector3 toPosition)
        {
            _toProcedure = toProcedure;
            _fromProcedure = fromProcedure;
            _toMapId = toMapId;
            _toPosition = toPosition;
        }

        public override IEnumerator OnPrepare(FsmEntity<ProcedureService> fsm)
        {
            var ret = _fairyService.AddPackageAsync("LoadingPage");
            yield return ret.AsCoroution();
            _loadedPkgs = ret.Result;

            _loadingPage = LoadingPage.Create();

            _loadingPage.ProgressBar.value = 0;

            _fairyService.AddToGRootFullscreen(_loadingPage.LoadingPageCom);
            yield return null;

            var proceed = false;
            _loadingPage.LoadingPageCom.TweenFade(1, LoadingFadeDuration).OnComplete(() => { proceed = true; });

            yield return new WaitUntil(() => proceed);
        }

        private IEnumerator LoadNext()
        {
            var package = _yooAssetsService.GetPackage();
            var info = _dataService.GetData<PlayerInfo>(DataKeys.PlayerInfo);

            //AsyncOperation op = null;
            var progress = _loadingPage.ProgressBar;
            MapData currMapData = null;
            MapData toMapData = null;
            if (info.CurrentMapId != _toMapId)
            {
                try
                {
                    currMapData = _gameDataService.GetMapData(info.CurrentMapId);
                }
                catch (MapDataNotFoundException)
                {
                }

                try
                {
                    toMapData = _gameDataService.GetMapData(_toMapId);
                }
                catch (MapDataNotFoundException)
                {
                    //加载地图错误，没有数据
                    UIDialog.ShowErrorMessage("错误", "地图数据没找到！", (id, btn, dialog) =>
                    {
                        dialog.Hide();
                        ChangeProcedure(_fromProcedure);
                    });
                    yield break;
                }

                var toSceneAddress = ResAddress.MapRes(_toMapId);

                // if (_handle.IsValid() && _handle.Status == AsyncOperationStatus.Succeeded)
                // {
                //     //卸载前一个handle
                //     yield return Addressables.UnloadSceneAsync(_handle).Task.AsCoroution();
                // }

                if (_handle != null)
                {
                    var unloadOp = _handle.UnloadAsync();
                    yield return unloadOp;
                    _handle = null;
                }


                //加载地图资源
                _handle = package.LoadSceneAsync(toSceneAddress, LoadSceneMode.Single, true);
                progress.value = _handle.Progress * 100;

                // _handle = Addressables.LoadSceneAsync(toSceneAddress, LoadSceneMode.Single, false);
                // progress.value = _handle.PercentComplete * 100;

                while (_handle.Progress < 0.899f)
                {
                    progress.value = _handle.Progress * 100;
                    yield return null;
                }

                //op = _handle.Result.ActivateAsync();
                //op.allowSceneActivation = false;

                if (_handle.Status == EOperationStatus.Failed)
                {
                    //加载失败了
                    // if (_handle.OperationException is InvalidKeyException)
                    // {
                    //加载地图错误，没有资源
                    UIDialog.ShowErrorMessage("错误", "地图没找到!", (id, btn, dialog) =>
                    {
                        dialog.Hide();
                        ChangeProcedure(_fromProcedure);
                    });
                    yield break;
                    // }
                }

                yield return new WaitUntil(() => _handle.Progress >= 0.899f);
            }

            progress.value = 100;

            //加载地图上的刷新点

            //加载地图上的资源点

            //加载地图上的其他玩家


            //设置背景音乐

            //加载地图音乐资源

            var sceneBgm = "";

            if (toMapData is { Bgm: > 0 })
            {
                sceneBgm = SoundService.ToFullPath(toMapData.Bgm_Ref.Catalog_Ref.Name, toMapData.Bgm_Ref.Name);
                // var task = _soundService.AsyncLoadSoundClip(sceneBgm, toMapData.Bgm_Ref.AssetAddress,
                //     toMapData.Bgm_Ref.Volume,
                //     toMapData.Bgm_Ref.Loop);
                // yield return task.AsCoroution();

                yield return _soundService.LoadSoundClipCor(toMapData.Bgm_Ref.Catalog_Ref.Name, toMapData.Bgm_Ref.Name,
                    toMapData.Bgm_Ref.AssetAddress,
                    toMapData.Bgm_Ref.Volume,
                    toMapData.Bgm_Ref.Loop);
            }

            //Move to BaseBattleInstance
            // if (toMapData is { BattleBgm: > 0 })
            // {
            //     var battleBgm = SoundService.ToFullPath(toMapData.BattleBgm_Ref.Catalog_Ref.Name,
            //         toMapData.BattleBgm_Ref.Name);
            //     var task = _soundService.AsyncLoadSoundClip(battleBgm, toMapData.BattleBgm_Ref.AssetAddress,
            //         toMapData.BattleBgm_Ref.Volume,
            //         toMapData.BattleBgm_Ref.Loop);
            //     yield return task.AsCoroution();
            // }


            if (currMapData != null)
            {
                _soundService.StopBGM(currMapData.Bgm_Ref.Catalog_Ref.Name, currMapData.Bgm_Ref.Name, unload: true);
            }

            if (!string.IsNullOrEmpty(sceneBgm))
            {
                _soundService.PlayBGM(sceneBgm);
            }


            //设置玩家位置
            info.CurrentMapId = _toMapId;
            info.Position = _toPosition;

            //if (op != null) op.allowSceneActivation = true;
            _handle.UnSuspend();
            yield return null;

            //切换进入下一个Procedure
            ChangeProcedure(_toProcedure);
        }

        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
            GameFramework.Inst.StartCoroutine(LoadNext());
        }

        public override void OnUpdate(FsmEntity<ProcedureService> fsm, float elapseTime, float realElapseTime)
        {
        }

        public override void OnLeave(FsmEntity<ProcedureService> fsm, bool isShutdown)
        {
            _loadingPage.LoadingPageCom.TweenFade(0, LoadingFadeDuration).OnComplete(() =>
            {
                GRoot.inst.RemoveChild(_loadingPage.LoadingPageCom, true);
                _loadingPage.LoadingPageCom.Dispose();
                _fairyService.ReleasePackages(_loadedPkgs);
            });
        }

        public static void BackToCurrentMap()
        {
            var mapId = GameFramework.GetService<DataNodeService>().GetData<int>(DataKeys.MapCurrentId);
            var entity = PlayerEntity.Current;
            ChangeMap(mapId, entity.Position);
        }

        public static void ChangeMap(int toMapId, Vector3 position)
        {
            ChangeMap(toMapId, position, typeof(NormalMapProcedure));
        }


        public static void ChangeMap(int toMapId, Vector3 position, Type toProcedure)
        {
            var ps = GameFramework.GetService<ProcedureService>();
            var ds = GameFramework.GetService<DataNodeService>();

            var info = ds.GetData<PlayerInfo>(DataKeys.PlayerInfo);
            info.NextMapId = toMapId;

            var fsm = ps.ProcedureFsm;

            var state = fsm.GetState<ChangeMapProcedure>();
            state.SetChangeMapInfo(toMapId, fsm.CurrState.GetType(), toProcedure, position);
            fsm.ChangeState<ChangeMapProcedure>();
        }
    }
}