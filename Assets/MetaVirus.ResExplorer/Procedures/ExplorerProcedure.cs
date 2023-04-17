using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using FairyGUI;
using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.DataNode;
using GameEngine.Event;
using GameEngine.FairyGUI;
using GameEngine.Fsm;
using GameEngine.Procedure;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Battle.UI;
using MetaVirus.Logic.UI;
using MetaVirus.ResExplorer.UI;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MetaVirus.ResExplorer.Procedures
{
    [Procedure(true, "ExplorerMode")]
    public class ExplorerProcedure : ProcedureBase
    {
        private FairyGUIService _fairyService;
        private DataNodeService _dataService;
        private GameDataService _gameDataService;

        private string[] _loadedPkgs;

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _fairyService = GameFramework.GetService<FairyGUIService>();
            _dataService = GameFramework.GetService<DataNodeService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
            GameFramework.GetService<ExplorerUIService>();
        }

        public override IEnumerator OnPrepare(FsmEntity<ProcedureService> fsm)
        {
            //通用设置
            UIConfig.bringWindowToFrontOnClick = false;
            GRoot.inst.SetContentScaleFactor(1920, 1080, UIContentScaler.ScreenMatchMode.MatchWidthOrHeight);

            var tasks = new List<Task>();

            //加载common ui资源
            var ret = _fairyService.AddPackageAsync("ui-common");
            yield return ret.AsCoroution();
            
            yield return GameFramework.GetService<UpdateService>().CheckUpdate();
            
            //yield return ret.AsCoroution();
            //Debug.Log("Common UI Loaded");
            
            //加载游戏数据
            var t = _gameDataService.LoadGameDataAsync();
            tasks.Add(t);
            // yield return t.AsCoroution();
            // Debug.Log("GameData Loaded");

            //加载gameitem icons资源
            ret = _fairyService.AddPackageAsync("ui-gameitem-icons");
            tasks.Add(ret);
            // yield return ret.AsCoroution();
            // Debug.Log("Gameitem Icons Loaded");

            //加载怪物头像
            ret = _fairyService.AddPackageAsync("ui-portrait");
            tasks.Add(ret);
            // yield return ret.AsCoroution();
            // Debug.Log("Portrait UI Loaded");

            //加载编辑器用资源
            ret = _fairyService.AddPackageAsync("ui-res-explorer");
            tasks.Add(ret);
            // yield return ret.AsCoroution();
            // Debug.Log("res explorer UI loaded");

            var fontTask = Addressables.LoadAssetAsync<TMP_FontAsset>("UI/Fonts/JosefinSans-Bold.asset").Task;
            yield return fontTask.AsCoroution();
            
            //加载英文及数字字体
            var tmpFont = new FloatingTextFont
            {
                name = Constants.EnJosefinSans,
                fontAsset = fontTask.Result
            };

            FontManager.RegisterFont(tmpFont);
            Debug.Log("Fonts Loaded");

            foreach (var task in tasks)
            {
                yield return task.AsCoroution();
            }
            
            //_loadedPkgs = ret.Result;

            GameFramework.GetService<EventService>().Emit(GameEvents.ResourceEvent.AllResLoaded);
        }

        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
        }

        public override void OnDestroy(FsmEntity<ProcedureService> fsm)
        {
            _fairyService.ReleasePackages(_loadedPkgs);
        }
    }
}