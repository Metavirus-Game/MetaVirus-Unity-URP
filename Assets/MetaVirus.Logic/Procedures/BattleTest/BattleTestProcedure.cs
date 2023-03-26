using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text;
using cfg.battle;
using FairyGUI;
using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.Config;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.FairyGUI;
using GameEngine.Fsm;
using GameEngine.Procedure;
using GameEngine.Utils;
using Google.Protobuf;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Battle.Scene;
using MetaVirus.Logic.UI;
using UnityEngine;
using UnityEngine.Android;
using Object = UnityEngine.Object;

namespace MetaVirus.Logic.Procedures
{
    [Procedure]
    public class BattleTestProcedure : ProcedureBase
    {
        private DataNodeService _dataService;
        private FairyGUIService _fairyService;
        private GameDataService _gameDataService;
        private EntityService _entityService;
        private BattleService _battleService;

        private GComponent _battleTestPageComp;
        // private GComponent _battleSkipComp;

        private string[] _loadedPkgs;

        private int _topSideMgId;
        private int _botSideMgId;

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _dataService = GameFramework.GetService<DataNodeService>();
            _fairyService = GameFramework.GetService<FairyGUIService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
            _entityService = GameFramework.GetService<EntityService>();
            _battleService = GameFramework.GetService<BattleService>();
        }

        public override IEnumerator OnPrepare(FsmEntity<ProcedureService> fsm)
        {
            //加载battle相关ui

            var task = _fairyService.AddPackageAsync("ui-battle");
            yield return task.AsCoroution();
            _loadedPkgs = task.Result;

            //开启BattleCamera
            var battleCamera = Object.FindObjectOfType<BattleCamera>();
            while (battleCamera == null)
            {
                battleCamera = Object.FindObjectOfType<BattleCamera>();
                yield return null;
            }

            battleCamera.TurnOn();

            yield return null;
        }

        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
            _battleTestPageComp = UIPackage.CreateObject("BattlePage", "BattleTestSetupPage").asCom;
            _fairyService.AddToGRootFullscreen(_battleTestPageComp);

            var botSideInput = _battleTestPageComp.GetChild("n5").asTextInput;
            var topSideInput = _battleTestPageComp.GetChild("n8").asTextInput;

            var txtMsg = _battleTestPageComp.GetChild("n49").asTextField;

            var botSideText = new GTextField[9];
            var topSideText = new GTextField[9];

            for (var i = 0; i < 9; i++)
            {
                botSideText[i] = _battleTestPageComp.GetChild("bot_" + (i + 1)).asTextField;
                topSideText[i] = _battleTestPageComp.GetChild("top_" + (i + 1)).asTextField;
            }

            var btnLoad = _battleTestPageComp.GetChild("n9").asButton;
            var btnBattle = _battleTestPageComp.GetChild("n48").asButton;

            btnBattle.enabled = false;

            btnBattle.onClick.Add(() => { GameFramework.Inst.StartCoroutine(RunBattle()); });

            btnLoad.onClick.Add(() =>
            {
                var strTopSide = topSideInput.text;
                var strBotSide = botSideInput.text;
                txtMsg.text = "";
                btnBattle.enabled = false;
                if (!int.TryParse(strBotSide, out _botSideMgId))
                {
                    txtMsg.text = "玩家位置(下侧) 怪物组ID输入有误，请检查!";
                    return;
                }

                if (!int.TryParse(strTopSide, out _topSideMgId))
                {
                    txtMsg.text = "怪物位置(上侧) 怪物组ID输入有误，请检查!";
                    return;
                }

                var botMg = _gameDataService.GetMonsterGroupData(_botSideMgId);
                if (botMg == null || _botSideMgId == 0)
                {
                    txtMsg.text = "玩家位置(下侧) 怪物组ID不存在，请检查!";
                    return;
                }

                var topMg = _gameDataService.GetMonsterGroupData(_topSideMgId);
                if (topMg == null || _topSideMgId == 0)
                {
                    txtMsg.text = "怪物位置(上侧) 怪物组ID不存在，请检查!";
                    return;
                }

                LoadMonsterGroups(botMg, botSideText);
                LoadMonsterGroups(topMg, topSideText);

                btnBattle.enabled = true;
            });
        }

        private IEnumerator RunBattle()
        {
#if UNITY_ANDROID
            Permission.RequestUserPermission("android.permission.INTERNET");
#endif

            var wndRunBattle = UIWaitingWindow.ShowWaiting("Loading...");

            var http = new HttpClient();

            var task = http.GetByteArrayAsync(
                $"http://{GameConfig.Inst.BattleServerIp}:{GameConfig.Inst.BattleServerPort}/battle/test/run?srcMgId={_botSideMgId}&tarMgId={_topSideMgId}");
            yield return task.AsCoroution();
            
            var bytes = task.Result;
            var str = Encoding.UTF8.GetString(bytes);
            bytes = Convert.FromBase64String(str);

            // var str =
            //     "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";

            //var bytes = Convert.FromBase64String(str);
            // using (var ms = new MemoryStream(bytes))
            // {
            //     var gzip = new GZipStream(ms, CompressionMode.Decompress);
            //     using (var resultMs = new MemoryStream())
            //     {
            //         gzip.CopyTo(resultMs);
            //         bytes = resultMs.ToArray();
            //     }
            // }

            // var record = BattleRecordPb.Parser.ParseFrom(bytes);
            var br = BattleRecord.FromGZipData(bytes);
            // var bi = new NormalBattleInstance(br, Object.FindObjectOfType<BattleField>());
            // yield return bi.LoadBattleCoro();
            // bi.OnEnter();
            //
            // _battleInstance = bi;

            var battleTask = _battleService.AsyncRunBattle(br, null);
            yield return battleTask.AsCoroution();

            wndRunBattle.Hide();
            _battleTestPageComp.visible = false;
            // _battleSkipComp.visible = true;
        }

        public override void OnUpdate(FsmEntity<ProcedureService> fsm, float elapseTime, float realElapseTime)
        {
            //_battleInstance?.OnUpdate(elapseTime, realElapseTime);
        }

        private void LoadMonsterGroups(MonsterGroupData mgData, GTextField[] mgTexts)
        {
            for (var i = 0; i < 9; i++)
            {
                var slot = mgData.Slot[i];
                if (slot.MonsterId == 0)
                {
                    mgTexts[i].text = "";
                }
                else
                {
                    mgTexts[i].text =
                        $"ID.{slot.MonsterId}\nLv.{(slot.Level == 0 ? slot.MonsterId_Ref.Level : slot.Level)}\n{slot.MonsterId_Ref.Name}";
                }
            }
        }

        public override void OnLeave(FsmEntity<ProcedureService> fsm, bool isShutdown)
        {
            GRoot.inst.RemoveChild(_battleTestPageComp, true);
            _fairyService.ReleasePackages(_loadedPkgs);
            _battleService.ReleaseBattle();
        }
    }
}