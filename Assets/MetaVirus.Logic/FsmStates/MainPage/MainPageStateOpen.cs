using System.Collections;
using FairyGUI;
using GameEngine.Fsm;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.UI;
using UnityEngine;

namespace MetaVirus.Logic.FsmStates.MainPage
{
    public class MainPageStateOpen : FsmState<MainPageProcedure>
    {
        private GObject _btnMain;
        private GComponent _btnTapToStar;
        private GObject _txtTapToStart;


        public override void OnInit(FsmEntity<MainPageProcedure> fsm)
        {
            _btnMain = fsm.Owner.MainPageCom.GetChild("btnMain");
            _btnTapToStar = fsm.Owner.MainPageCom.GetChild("btnTapToStart").asCom;
            _txtTapToStart = fsm.Owner.MainPageCom.GetChild("txtTapToStart");
        }

        public override void OnEnter(FsmEntity<MainPageProcedure> fsm)
        {
            _btnTapToStar.alpha = _txtTapToStart.alpha = 0;

            _btnTapToStar.TweenFade(1, 0.5f).OnComplete(() =>
            {
                var trans = _btnTapToStar.GetTransition("t1");
                trans?.Play(-1, 0, null);
                _btnMain.onClick.Add(OnMainPageClicked);
            });

            _txtTapToStart.TweenFade(1, 0.5f);

        }

        private void ToCheckUpdate()
        {
            var trans = _btnTapToStar.GetTransition("t1");
            trans?.Stop();
            _btnTapToStar.TweenFade(0, 0.5f);
            _txtTapToStart.TweenFade(0, 0.5f);
            ChangeState<MainPageStateCheckUpdate>(Fsm);
        }

        public override void OnLeave(FsmEntity<MainPageProcedure> fsm, bool isShutdown)
        {
            _btnMain.onClick.Remove(OnMainPageClicked);
        }


        private void OnMainPageClicked()
        {
            ToCheckUpdate();
        }
    }
}