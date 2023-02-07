using System.Collections;
using GameEngine;
using GameEngine.Fsm;
using GameEngine.Utils;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Battle.Scene;
using MetaVirus.Logic.Utils;
using static UnityEngine.Object;

namespace MetaVirus.Logic.FsmStates.EnterBattleProcedureFsm
{
    public class EnterBattleState : FsmState<EnterBattleProcedure>
    {
        private BattleService _battleService;

        public override void OnEnter(FsmEntity<EnterBattleProcedure> fsm)
        {
            _battleService = GameFramework.GetService<BattleService>();
            GameFramework.Inst.StartCoroutine(EnterBattleCor(fsm));
        }

        private IEnumerator EnterBattleCor(FsmEntity<EnterBattleProcedure> fsm)
        {
            var owner = fsm.Owner;
            var loadingPage = owner.LoadingPage;
            var progress = loadingPage.ProgressBar;

            var handler = new TaskProgressHandler();
            //载入所有的战斗单位
            var battleTask = _battleService.AsyncRunBattle(owner.BattleRecord, handler);

            while (!battleTask.IsCompleted)
            {
                progress.value = handler.Progress;
                yield return null;
            }

            yield return battleTask.AsCoroution();

            Enter(fsm);
        }


        //进入战斗
        private void Enter(FsmEntity<EnterBattleProcedure> fsm)
        {
            //开启地图上的战斗摄像机
            var battleCamera = FindObjectOfType<BattleCamera>();
            battleCamera.TurnOn();

            //关闭地图上的场景摄像机
            var sceneCamera = FindObjectOfType<SceneCamera>();
            sceneCamera.TurnOff();

            //进入战斗procedure
            fsm.Owner.ChangeProcedure<NormalBattleProcedure>();
        }
    }
}