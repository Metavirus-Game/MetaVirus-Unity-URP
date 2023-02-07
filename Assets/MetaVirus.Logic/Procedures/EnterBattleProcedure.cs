using System.Collections;
using cfg.map;
using FairyGUI;
using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.DataNode;
using GameEngine.FairyGUI;
using GameEngine.Fsm;
using GameEngine.Network;
using GameEngine.Procedure;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.FsmStates.EnterBattleProcedureFsm;
using MetaVirus.Logic.Protocols.Player;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Battle.Scene;
using MetaVirus.Logic.UI;
using MetaVirus.Logic.Utils;
using MetaVirus.Net.Messages.Player;
using UnityEngine;

namespace MetaVirus.Logic.Procedures
{
    [Procedure]
    public class EnterBattleProcedure : ProcedureBase
    {
        private const string FsmName = "EnterBattleProcedureFsm";

        public int NpcId { get; private set; }
        public NpcRefreshInfo NpcInfo { get; private set; }

        public BattleRecord BattleRecord { get; set; }

        private FairyGUIService _fairyService;
        private FsmService _fsmService;
        private FsmEntity<EnterBattleProcedure> _fsmEntity;

        public LoadingPage LoadingPage;
        private string[] _loadedPkgs;


        /// <summary>
        /// 设置触发战斗的npc信息
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="npcInfo"></param>
        private void SetBattleInfo(int npcId, NpcRefreshInfo npcInfo)
        {
            NpcId = npcId;
            NpcInfo = npcInfo;
            BattleRecord = null;
        }

        private void SetBattleRecord(BattleRecord record)
        {
            NpcId = -1;
            BattleRecord = record;
        }

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _fairyService = GameFramework.GetService<FairyGUIService>();
            _fsmService = GameFramework.GetService<FsmService>();
        }

        public override IEnumerator OnPrepare(FsmEntity<ProcedureService> fsm)
        {
            //准备进入战斗
            //先关闭玩家控制，并且避免玩家再次进入战斗
            var player = PlayerEntity.Current;
            if (player != null)
            {
                player.AvoidBattleTimer = 999;
                player.PlayerController.enabled = false;
            }

            //加载Loading和battle ui
            var ret = _fairyService.AddPackageAsync("ui-loading");
            yield return ret.AsCoroution();
            _loadedPkgs = ret.Result;

            LoadingPage = LoadingPage.Create();
            _fairyService.AddToGRootFullscreen(LoadingPage.LoadingPageCom);
            yield return null;

            var proceed = false;
            LoadingPage.LoadingPageCom.TweenFade(1, 0.3f).OnComplete(() => { proceed = true; });
            yield return new WaitUntil(() => proceed);
        }

        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
            //创建状态机
            _fsmEntity = _fsmService.CreateFsm(FsmName, this, new EnterBattleRequestBattleState(),
                new EnterBattleState());
            if (BattleRecord == null)
            {
                //未传入战斗结果，使用NpcId请求战斗
                _fsmEntity.Start<EnterBattleRequestBattleState>();
            }
            else
            {
                //已经传入了战斗结果，直接进入战斗
                _fsmEntity.Start<EnterBattleState>();
            }
        }

        public override void OnLeave(FsmEntity<ProcedureService> fsm, bool isShutdown)
        {
            LoadingPage.LoadingPageCom.TweenFade(0, 0.3f).OnComplete(() =>
            {
                //离开战斗
                //先关闭玩家控制，并且避免玩家再次进入战斗
                var player = PlayerEntity.Current;
                if (player != null)
                {
                    //让玩家暂时避免战斗2秒钟
                    player.AvoidBattle = true;
                    //恢复玩家控制
                    player.PlayerController.enabled = true;
                }

                GRoot.inst.RemoveChild(LoadingPage.LoadingPageCom);
                _fairyService.ReleasePackages(_loadedPkgs);
            });

            //移除状态机
            _fsmService.DestroyFsm<EnterBattleProcedure>(FsmName);
            //清除战斗记录
            BattleRecord = null;
        }

        /// <summary>
        /// 直接播放一段战斗记录
        /// </summary>
        /// <param name="result"></param>
        public static void EnterBattle(BattleRecord result)
        {
            var procedureService = GameFramework.GetService<ProcedureService>();
            var currProcedure = procedureService.CurrProcedure;
            var procedureEntity = procedureService.ProcedureFsm;
            var enterBattle = procedureEntity.GetState<EnterBattleProcedure>();
            enterBattle.SetBattleRecord(result);

            GameFramework.GetService<DataNodeService>()
                .SetData(Constants.DataKeys.BattleBackProcedure, currProcedure.GetType());

            procedureEntity.ChangeState<EnterBattleProcedure>();
        }

        /// <summary>
        /// 与指定npc发生战斗交互
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="npcInfo"></param>
        public static void EnterBattle(int npcId, NpcRefreshInfo npcInfo)
        {
            var procedureService = GameFramework.GetService<ProcedureService>();
            var currProcedure = procedureService.CurrProcedure;
            var procedureEntity = procedureService.ProcedureFsm;
            var enterBattle = procedureEntity.GetState<EnterBattleProcedure>();
            enterBattle.SetBattleInfo(npcId, npcInfo);

            GameFramework.GetService<DataNodeService>()
                .SetData(Constants.DataKeys.BattleBackProcedure, currProcedure.GetType());

            procedureEntity.ChangeState<EnterBattleProcedure>();
        }
    }
}