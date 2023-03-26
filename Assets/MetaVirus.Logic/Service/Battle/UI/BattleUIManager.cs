using System.Collections.Generic;
using System.Threading.Tasks;
using FairyGUI;
using GameEngine;
using GameEngine.Common;
using GameEngine.DataNode;
using GameEngine.Event;
using GameEngine.FairyGUI;
using GameEngine.ObjectPool;
using GameEngine.Procedure;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events.Battle;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.Service.Battle.UI.Pages;
using MetaVirus.Logic.Service.Battle.UI.Pages.BattleActionBar;
using MetaVirus.Logic.Service.Battle.UI.Pages.BattleHeaderUI;
using MetaVirus.Logic.Service.Battle.UI.Pages.BattleRecordTextUI;
using MetaVirus.Logic.Service.Battle.UI.Pages.FloatingText;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;

namespace MetaVirus.Logic.Service.Battle.UI
{
    public class BattleUIManager
    {
        private BaseBattleInstance _battle;
        private DataNodeService _dataService;
        private readonly FairyGUIService _fairyService;
        private BattleService _battleService;

        private readonly List<BattleUIComponent> _battleUIComponents;

        private TMPFont _font;

        public GComponent UIBattlePage { get; private set; }

        /// <summary>
        /// 退出战斗时的回调
        /// </summary>
        public EventCallback0 ExitCallback { get; set; }

        /// <summary>
        /// 重播的回调
        /// </summary>
        public EventCallback0 ReplayCallback { get; set; }

        public BattleUIManager(BaseBattleInstance battle)
        {
            _battle = battle;
            _battleService = GameFramework.GetService<BattleService>();
            _dataService = GameFramework.GetService<DataNodeService>();
            _fairyService = GameFramework.GetService<FairyGUIService>();

            _battleUIComponents = new List<BattleUIComponent>
            {
                new DamageFloatingUIComponent(this, battle),
                new BattleActionBarUIComponent(this, battle),
                new BattleHeaderUIComponent(this, battle),
                new BattleRecordTextUIComponent(this, battle)
            };
        }


        public void Clear()
        {
            foreach (var comp in _battleUIComponents)
            {
                comp.Release();
            }

            GRoot.inst.RemoveChild(UIBattlePage, true);
        }


        public void OnUpdate(float elapseTime, float realElapseTime)
        {
            foreach (var comp in _battleUIComponents)
            {
                comp.OnUpdate(elapseTime, realElapseTime);
            }
        }

        public async Task AsyncLoadBattleUI()
        {
            try
            {
                UIBattlePage = UIPackage.CreateObject("BattlePage", "BattlePage").asCom;
                var btnSkip = UIBattlePage.GetChild("btnSkip").asButton;
                var btnSpeed = UIBattlePage.GetChild("btnSpeed").asButton;
                btnSkip.text = "Skip";
                //btnSkip.onClick.Add(ExitCallback ?? ChangeMapProcedure.BackToCurrentMap);
                btnSkip.onClick.Set(() => { _battle.Skip(); });

                btnSpeed.text = $"[size=60]×[/size]{_battleService.TimeSpeedRate}";
                btnSpeed.onClick.Set(() =>
                {
                    btnSpeed.text = $"[size=60]×[/size]{_battleService.NextTimeSpeedOption()}";
                });

                _fairyService.AddToGRootFullscreen(UIBattlePage);

                foreach (var comp in _battleUIComponents)
                {
                    comp.Load();
                }
            }
            catch (System.Exception e)
            {
                Debug.Log("Load UI Error: ");
                Debug.Log(e);
            }
        }
    }
}