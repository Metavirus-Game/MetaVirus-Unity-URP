using System.Collections;
using System.Threading.Tasks;
using GameEngine;
using GameEngine.Config;
using GameEngine.Fsm;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MetaVirus.Logic.FsmStates.MainPage
{
    public class MainPageStateCheckUpdate : FsmState<MainPageProcedure>
    {
        private LocalizeService _localize;
        private UIWaitingWindow _wndUpdate;

        public override void OnInit(FsmEntity<MainPageProcedure> fsm)
        {
            _localize = GameFramework.GetService<LocalizeService>();
        }

        public override void OnEnter(FsmEntity<MainPageProcedure> fsm)
        {
            _wndUpdate = UIWaitingWindow.ShowWaiting(_localize.GetLanguageText("Common_Loading_Content_Updating"));
            //check update
            CheckUpdates(fsm);
        }

        private async void CheckUpdates(FsmEntity<MainPageProcedure> fsm)
        {
            var list = await Addressables.CheckForCatalogUpdates().Task;
            Debug.Log(list);

            //TODO 模拟更新延迟，改为更新逻辑
            await Task.Delay(1000);

            var hasUpdate = false;

            if (hasUpdate)
            {
                //TODO 走更新逻辑
                _wndUpdate.Hide();
            }
            else
            {
                _wndUpdate.Hide(() =>
                {
                    //连接服务器
                    //ChangeState<MainPageStateConnectServer>(fsm);
                    if (GameConfig.Inst.OfflineTest)
                    {
                        ChangeState<MainPageEnterOfflineTest>(fsm);
                    }
                    else
                    {
                        ChangeState<MainPageStateEnterGame>(fsm);
                    }
                });
            }
        }

        public override void OnLeave(FsmEntity<MainPageProcedure> fsm, bool isShutdown)
        {
        }

        public override void OnUpdate(FsmEntity<MainPageProcedure> fsm, float elapseTime, float realElapseTime)
        {
        }

        public override void OnDestroy(FsmEntity<MainPageProcedure> fsm)
        {
        }
    }
}