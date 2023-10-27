using System.Collections;
using FairyGUI;
using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.DataNode;
using GameEngine.FairyGUI;
using GameEngine.Fsm;
using GameEngine.Procedure;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Windows;

namespace MetaVirus.Logic.Procedures.fortest
{
    [Procedure]
    public class UITestProcedure : ProcedureBase
    {
        private DataNodeService _dataService;
        private GameDataService _gameDataService;
        private FairyGUIService _fairyService;
        private UIService _uiService;

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _dataService = GameFramework.GetService<DataNodeService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
            _fairyService = GameFramework.GetService<FairyGUIService>();
            _uiService = GameFramework.GetService<UIService>();
        }


        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
            _uiService.OpenWindow<UICreateMonsterWindow>();
        }
    }
}