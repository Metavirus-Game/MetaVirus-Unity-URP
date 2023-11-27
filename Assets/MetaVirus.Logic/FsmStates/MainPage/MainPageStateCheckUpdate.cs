﻿using System.Collections;
using System.Threading.Tasks;
using GameEngine;
using GameEngine.Config;
using GameEngine.Fsm;
using GameEngine.Procedure;
using GameEngine.Utils;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.Service;
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
            //_wndUpdate = UIWaitingWindow.ShowWaiting(_localize.GetLanguageText("Common_Loading_Content_Updating"));
            //check update
            GameFramework.Inst.StartCoroutine(CheckUpdates(fsm));
        }

        private IEnumerator CheckUpdates(FsmEntity<MainPageProcedure> fsm)
        {
            var updateTask = GameFramework.GetService<UpdateService>().AsyncUpdate();
            yield return updateTask.AsCoroution();

            if (updateTask.Result > 0)
            {
                //数据有更新，重新加载游戏
                fsm.Owner.ChangeProcedure<ReloadAfterUpdateProcedure>();
                yield break;
            }

            // 连接服务器
            //ChangeState<MainPageStateConnectServer>(fsm);
            if (GameConfig.Inst.OfflineTest)
            {
                ChangeState<MainPageEnterOfflineTest>(fsm);
            }
            else
            {
                ChangeState<MainPageStateEnterGame>(fsm);
            }

            // var hasUpdate = false;
            //
            // if (hasUpdate)
            // {
            //     //TODO 走更新逻辑
            //     _wndUpdate.Hide();
            // }
            // else
            // {
            //     _wndUpdate.Hide(() =>
            //     {
            //         //连接服务器
            //         //ChangeState<MainPageStateConnectServer>(fsm);
            //         if (GameConfig.Inst.OfflineTest)
            //         {
            //             ChangeState<MainPageEnterOfflineTest>(fsm);
            //         }
            //         else
            //         {
            //             ChangeState<MainPageStateEnterGame>(fsm);
            //         }
            //     });
            // }
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