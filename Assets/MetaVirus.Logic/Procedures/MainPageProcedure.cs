﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FairyGUI;
using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.FairyGUI;
using GameEngine.Fsm;
using GameEngine.Procedure;
using GameEngine.Utils;
using MetaVirus.Logic.FsmStates.MainPage;
using MetaVirus.Logic.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using static GameEngine.GameFramework;

namespace MetaVirus.Logic.Procedures
{
    [Procedure]
    public class MainPageProcedure : ProcedureBase
    {
        private FairyGUIService _fairyService;
        private LocalizeService _localize;

        private readonly List<GameObject> _loadedObjes = new();

        private string[] _loadedPkgs;

        public GComponent MainPageCom { get; private set; }

        public GComponent BtnEnter { get; private set; }

        private FsmEntity<MainPageProcedure> _fsmMainPage;

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _fairyService = GetService<FairyGUIService>();
            _localize = GetService<LocalizeService>();
        }

        public override IEnumerator OnPrepare(FsmEntity<ProcedureService> fsm)
        {
            var ret = _fairyService.AddPackageAsync("ui-mainpage");
            yield return ret.AsCoroution();

            _loadedPkgs = ret.Result;

            CreateMainPage();

            _fsmMainPage = GetService<FsmService>().CreateFsm("FsmMainPage", this,
                new MainPageStateOpen(),
                new MainPageStateCheckUpdate(),
                new MainPageStateConnectServer(),
                new MainPageStateEnterGame(),
                new MainPageEnterOfflineTest(),
                new MainPageStateCreateActor(),
                new MainPageStatePlayerLogin()
            );

            _fsmMainPage.Start<MainPageStateOpen>();
        }

        private async void CreateMainPage()
        {
            MainPageCom = UIPackage.CreateObject("MainPage", "MainPage").asCom;

            _fairyService.AddToGRootFullscreen(MainPageCom);

            var go = await Addressables.InstantiateAsync(
                "Assets/MetaVirus.Res/Scenes/2.MainScene/Res/Particle_Tonado.prefab").Task;
            _loadedObjes.Add(go);

            go.transform.localScale = new Vector3(70, 70, 70);
            MainPageCom.GetChild("effect").asGraph.SetNativeObject(new GoWrapper(go));

            //EnterGameButton
            var btnEnterGame = MainPageCom.GetChild("BtnEnter").asCom;
            //btnEnterGame.text = L("Common_Button_EnterGame");
            btnEnterGame.visible = false;

            BtnEnter = btnEnterGame;
            
            //加载创建角色的资源
            var scene =await Addressables.LoadSceneAsync("map-createactor/CreateActor.unity", LoadSceneMode.Additive).Task;
            SceneManager.SetActiveScene(scene.Scene);
        }

        public override void OnUpdate(FsmEntity<ProcedureService> fsm, float elapseTime, float realElapseTime)
        {
            MainPageCom.visible = _fsmMainPage.CurrState.GetType() != typeof(MainPageStateCreateActor);
        }

        public override void OnLeave(FsmEntity<ProcedureService> fsm, bool isShutdown)
        {
            GetService<FsmService>().DestroyFsm<MainPageProcedure>(_fsmMainPage.Name);
            _fairyService.ReleasePackages(_loadedPkgs);

            foreach (var obj in _loadedObjes)
            {
                Addressables.ReleaseInstance(obj);
            }

            GRoot.inst.RemoveChild(MainPageCom, true);
        }

        public override void OnDestroy(FsmEntity<ProcedureService> fsm)
        {
            _fsmMainPage.Shutdown();
        }
    }
}