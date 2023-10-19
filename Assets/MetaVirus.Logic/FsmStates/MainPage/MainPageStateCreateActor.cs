using GameEngine;
using GameEngine.DataNode;
using GameEngine.Fsm;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Windows;

namespace MetaVirus.Logic.FsmStates.MainPage
{
    public class MainPageStateCreateActor : FsmState<MainPageProcedure>
    {
        private UIService _uiService;
        private DataNodeService _dataNodeService;

        public override void OnInit(FsmEntity<MainPageProcedure> fsm)
        {
            _uiService = GameFramework.GetService<UIService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();
        }

        public override void OnEnter(FsmEntity<MainPageProcedure> fsm)
        {
            var wnd = _uiService.OpenWindow<UICreateActor>();
            wnd.OnClosed = () => { ChangeState<MainPageStateEnterGame>(fsm); };

            wnd.OnActorCreated = playerId =>
            {
                wnd.Hide();
                _dataNodeService.SetData(Constants.DataKeys.LoginPlayerId, playerId);
                ChangeState<MainPageStatePlayerLogin>(Fsm);
            };
        }
    }
}