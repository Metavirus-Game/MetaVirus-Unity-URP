using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using GameEngine.Base.Attributes;
using GameEngine.Common;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.FairyGUI;
using GameEngine.Fsm;
using GameEngine.Procedure;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Data.Events.Battle;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Battle.Scene;
using UnityEngine;
using static GameEngine.GameFramework;

namespace MetaVirus.Logic.Procedures
{
    [Procedure]
    public class NormalBattleProcedure : ProcedureBase
    {
        private DataNodeService _dataService;
        private EntityService _entityService;
        private EventService _eventService;
        private FairyGUIService _fairyService;
        private BattleService _battleService;

        private GComponent _battlePageComp;
        private GButton _btnSkip;

        private string[] _loadedPkgs;

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _dataService = GetService<DataNodeService>();
            _entityService = GetService<EntityService>();
            _eventService = GetService<EventService>();
            _fairyService = GetService<FairyGUIService>();
            _battleService = GetService<BattleService>();
        }

        // public override IEnumerator OnPrepare(FsmEntity<ProcedureService> fsm)
        // {
        //     var task = _fairyService.AddPackageAsync("ui-battle");
        //     yield return task.AsCoroution();
        //
        //     _loadedPkgs = task.Result;
        //
        //     _battlePageComp = UIPackage.CreateObject("BattlePage", "BattlePage").asCom;
        //
        //     _fairyService.AddToGRootFullscreen(_battlePageComp);
        //
        //     _btnSkip = _battlePageComp.GetChild("btnSkip").asButton;
        //     _btnSkip.text = L("BattlePage_Btn_Skip");
        // }

        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
            var player = PlayerEntity.Current?.Player;
            if (player != null)
            {
                player.SetActive(false);
            }

            // _btnSkip.onClick.Add(() =>
            // {
            //     var mapId = _dataService.GetData<int>(Constants.DataKeys.MapCurrentId);
            //     var entity = PlayerEntity.Current;
            //     ChangeMapProcedure.ChangeMap(mapId, entity.Position);
            // });
        }

        public override void OnLeave(FsmEntity<ProcedureService> fsm, bool isShutdown)
        {
            _battleService.ReleaseBattle();

            //开启地图上的场景摄像机
            var battleCamera = Object.FindObjectOfType<SceneCamera>();
            battleCamera.TurnOn();

            //关闭地图上的战斗摄像机
            var sceneCamera = Object.FindObjectOfType<BattleCamera>();
            sceneCamera.TurnOff();
        }
    }
}