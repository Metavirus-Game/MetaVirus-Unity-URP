using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
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
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Battle.Scene;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI;
using MetaVirus.Logic.UI.Component.Common;
using MetaVirus.Logic.UI.Windows;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MetaVirus.Logic.Procedures.BattleTest
{
    [Procedure]
    public class MonsterTestProcedure : ProcedureBase
    {
        private DataNodeService _dataNodeService;
        private FairyGUIService _fairyService;
        private GameDataService _gameDataService;
        private EntityService _entityService;
        private BattleService _battleService;
        private UIService _uiService;

        private string[] _loadedPkgs;
        private GComponent _monsterTestPageComp;

        private Dictionary<int, int[]> _upFormation = new();
        private Dictionary<int, int[]> _downFormation = new();

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _fairyService = GameFramework.GetService<FairyGUIService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
            _entityService = GameFramework.GetService<EntityService>();
            _battleService = GameFramework.GetService<BattleService>();
            _uiService = GameFramework.GetService<UIService>();
        }

        public override IEnumerator OnPrepare(FsmEntity<ProcedureService> fsm)
        {
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
            MakeMonsterTestComposite();
        }

        private void ClearMonsterSelection()
        {
            _upFormation.Clear();
            _downFormation.Clear();
            for (var i = 1; i <= 5; i++)
            {
                var comp = _monsterTestPageComp.GetChild("monster_up_" + i).asCom;
                var header = comp.GetChild("header").asCom;
                var txtName = comp.GetChild("txtName").asTextField;
                var ctrl = header.GetController("quality");
                ctrl.SetSelectedIndex(0);
                header.onClick.Clear();
                txtName.text = "";

                comp = _monsterTestPageComp.GetChild("monster_down_" + i).asCom;
                header = comp.GetChild("header").asCom;
                txtName = comp.GetChild("txtName").asTextField;
                ctrl = header.GetController("quality");
                ctrl.SetSelectedIndex(0);
                header.onClick.Clear();
                txtName.text = "";
            }
        }

        private void MakeMonsterTestComposite()
        {
            _monsterTestPageComp = UIPackage.CreateObject("BattlePage", "MonsterTestPage").asCom;
            _fairyService.AddToGRootFullscreen(_monsterTestPageComp);

            for (var i = 1; i <= 5; i++)
            {
                var comp = _monsterTestPageComp.GetChild("monster_up_" + i).asCom;
                var btn = comp.GetChild("btnLoad").asButton;
                var inId = comp.GetChild("txMonsterId").asTextInput;
                var inLv = comp.GetChild("txtLevel").asTextInput;
                var txtName = comp.GetChild("txtName").asTextField;
                var header = comp.GetChild("header").asCom;
                MakeMonsterInput(i, inId, inLv, txtName, btn, header);
            }

            for (var i = 1; i <= 5; i++)
            {
                var comp = _monsterTestPageComp.GetChild("monster_down_" + i).asCom;
                var btn = comp.GetChild("btnLoad").asButton;
                var inId = comp.GetChild("txMonsterId").asTextInput;
                var inLv = comp.GetChild("txtLevel").asTextInput;
                var txtName = comp.GetChild("txtName").asTextField;
                var header = comp.GetChild("header").asCom;
                MakeMonsterInput(i, inId, inLv, txtName, btn, header, false);
            }

            var btnGo = _monsterTestPageComp.GetChild("btnGo").asButton;
            btnGo.onClick.Set(() =>
            {
                if (_upFormation.Count == 0 || _downFormation.Count == 0)
                {
                    UIDialog.ShowErrorMessage("Error", "上下阵型都至少要上阵一个怪物才能进入战斗！", null);
                    return;
                }

                GameFramework.Inst.StartCoroutine(LoadBattle());
            });
        }

        private string MakeFormationStr(Dictionary<int, int[]> formation)
        {
            var ret = "";
            foreach (var idx in formation.Keys)
            {
                formation.TryGetValue(idx, out var arr);
                if (arr != null)
                {
                    var slot = idx switch
                    {
                        1 => 1,
                        2 => 2,
                        3 => 4,
                        4 => 5,
                        5 => 6,
                        _ => 1
                    };
                    if (!string.IsNullOrEmpty(ret))
                    {
                        ret += ",";
                    }

                    ret += slot + "," + arr[0] + "," + arr[1];
                }
            }

            return ret;
        }

        private IEnumerator LoadBattle()
        {
            var wndRunBattle = UIWaitingWindow.ShowWaiting("Loading...");

            var http = new HttpClient();

            var upSide = MakeFormationStr(_upFormation);
            var downSide = MakeFormationStr(_downFormation);

            var task = http.GetByteArrayAsync(
                $"http://{GameConfig.Inst.BattleServerIp}:{GameConfig.Inst.BattleServerPort}/battle/v1/mvm?upSide={upSide}&downSide={downSide}");
            yield return task.AsCoroution();

            var bytes = task.Result;
            //var str = Encoding.UTF8.GetString(bytes);
            //bytes = Convert.FromBase64String(str);

            // var str =
            //     "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";
            //
            // var bytes = Convert.FromBase64String(str);
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

            var battleTask = _battleService.AsyncRunBattle(br, null, OnExitBattle,
                () => GameFramework.Inst.StartCoroutine(OnReplayBattle(bytes)));
            yield return battleTask.AsCoroution();

            wndRunBattle.Hide();
            _monsterTestPageComp.visible = false;
        }

        private void OnExitBattle()
        {
            _battleService.ReleaseBattle();
            ClearMonsterSelection();
            _monsterTestPageComp.visible = true;
        }

        private IEnumerator OnReplayBattle(byte[] brBytes)
        {
            var wndRunBattle = UIWaitingWindow.ShowWaiting("Loading...");
            _battleService.ReleaseBattle();
            var br = BattleRecord.FromGZipData(brBytes);
            var battleTask = _battleService.AsyncRunBattle(br, null, OnExitBattle,
                () => GameFramework.Inst.StartCoroutine(OnReplayBattle(brBytes)));
            yield return battleTask.AsCoroution();
            wndRunBattle.Hide();
        }

        private void MakeMonsterInput(int slot, GTextInput inId, GTextInput inLv, GTextField txtName, GButton btnLoad,
            GComponent header, bool up = true)
        {
            btnLoad.onClick.Set(() =>
            {
                if (string.IsNullOrEmpty(inId.text) || !int.TryParse(inId.text, out var id) || id <= 0)
                {
                    UIDialog.ShowErrorMessage("Error", "怪物Id输入错误", null);
                    return;
                }

                if (string.IsNullOrEmpty(inLv.text) || !int.TryParse(inLv.text, out var lv) || lv <= 0)
                {
                    UIDialog.ShowErrorMessage("Error", "怪物等级输入错误", null);
                    return;
                }

                var monsterData = _gameDataService.GetMonsterData(id);
                if (monsterData == null)
                {
                    UIDialog.ShowErrorMessage("Error", "怪物Id输入错误, MonsterData数据未找到.", null);
                    return;
                }

                var provider = new MonsterDataProvider(monsterData, lv);
                MonsterHeaderGridButton.RenderHeaderCompNew(provider, header);
                txtName.text = monsterData.Name;

                if (up)
                {
                    _upFormation[slot] = new[] { id, lv };
                }
                else
                {
                    _downFormation[slot] = new[] { id, lv };
                }

                header.onClick.Set(() =>
                {
                    //弹出属性窗口
                    _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailData, provider);
                    _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailDataList,
                        new MonsterDataListProvider(provider));
                    _uiService.OpenWindow<UIMonsterDetail>();
                });
            });
        }
    }

    class MonsterDataListProvider : IMonsterListProvider
    {
        private IMonsterDataProvider[] _providers;

        public MonsterDataListProvider(params IMonsterDataProvider[] provider)
        {
            _providers = provider;
        }

        public IMonsterDataProvider GetMonsterData(int id)
        {
            return _providers.First(p => p.Id == id);
        }

        public IMonsterDataProvider GetMonsterDataAt(int index)
        {
            if (index >= 0 && index < _providers.Length)
            {
                return _providers[index];
            }

            return null;
        }

        public int GetMonsterDataIndex(int id)
        {
            for (var i = 0; i < _providers.Length; i++)
            {
                if (_providers[i].Id == id)
                {
                    return i;
                }
            }

            return -1;
        }

        public int Count => _providers.Length;
    }
}