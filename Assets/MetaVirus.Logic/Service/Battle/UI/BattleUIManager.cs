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

namespace MetaVirus.Logic.Service.Battle.UI
{
    public class BattleUIManager
    {
        private BaseBattleInstance _battle;
        private DataNodeService _dataService;
        private readonly FairyGUIService _fairyService;

        private readonly List<BattleUIComponent> _battleUIComponents;

        private TMPFont _font;

        public GComponent UIBattlePage { get; private set; }

        public BattleUIManager(BaseBattleInstance battle)
        {
            _battle = battle;
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
                btnSkip.text = "Skip";
                btnSkip.onClick.Add(ChangeMapProcedure.BackToCurrentMap);

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