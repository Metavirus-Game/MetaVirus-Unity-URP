using System.Collections;
using System.Threading.Tasks;
using cfg.map;
using FairyGUI;
using GameEngine;
using GameEngine.Base;
using GameEngine.Config;
using GameEngine.DataNode;
using GameEngine.Procedure;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.Service.Arena.data;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Utils;
using UnityEngine;

namespace MetaVirus.Logic.Service
{
    public class BattleService : BaseService
    {
        private BaseBattleInstance _battleInstance;
        private BattleField _battleField;

        public BaseBattleInstance CurrentBattle => _battleInstance;
        public BattleField BattleField => _battleField;

        private int _timeSpeedRateIndex = 0;

        /// <summary>
        /// 当前战斗播放速度 
        /// </summary>
        public int TimeSpeedRate => GameConfig.Inst.TimeScaleOptions[_timeSpeedRateIndex];

        /// <summary>
        /// 切换到下一个战斗播放速度
        /// </summary>
        public int NextTimeSpeedOption()
        {
            _timeSpeedRateIndex++;
            if (_timeSpeedRateIndex >= GameConfig.Inst.TimeScaleOptions.Length)
            {
                _timeSpeedRateIndex = 0;
            }

            Time.timeScale = TimeSpeedRate;

            return TimeSpeedRate;
        }

        /// <summary>
        /// 恢复TimeScale
        /// </summary>
        public void ResetTimeSpeed()
        {
            //只恢复Time.timeScale，当前速度选项保留，下次战斗沿用
            Time.timeScale = 1;
        }

        /// <summary>
        /// 应用当前设定的战斗速度
        /// </summary>
        public void ApplyTimeSpeed()
        {
            Time.timeScale = TimeSpeedRate;
        }

        /// <summary>
        /// 暂时没用
        /// </summary>
        private float _timeScale = 1;

        /// <summary>
        /// 暂时没用
        /// </summary>
        public float TimeScale
        {
            get => _timeScale;
            set
            {
                if (_timeScale != value)
                {
                    _timeScale = value;
                    OnTimeScaleChanged();
                }
            }
        }

        private void OnTimeScaleChanged()
        {
            if (_battleInstance != null)
            {
                _battleInstance.TimeScale = _timeScale;
            }
        }

        public override void PostConstruct()
        {
        }


        public async Task<BaseBattleInstance> AsyncRunBattle(BattleRecord battleRecord, TaskProgressHandler handler,
            EventCallback0 skipCallback = null, EventCallback0 replayCallback = null)
        {
            handler?.ReportProgress(0);
            _battleField = FindObjectOfType<BattleField>();
            var bi = new NormalBattleInstance(battleRecord, _battleField, skipCallback, replayCallback);
            _battleInstance = bi;
            await bi.AsyncLoadBattle(handler);

            bi.OnEnter();
            return _battleInstance;
        }

        public async Task<BaseBattleInstance> AsyncRunArenaMatch(ArenaBattleResult result, TaskProgressHandler handler)
        {
            handler?.ReportProgress(0);
            _battleField = FindObjectOfType<BattleField>();
            var bi = new ArenaBattleInstance(result, _battleField);
            _battleInstance = bi;
            await bi.AsyncLoadBattle(handler);

            bi.OnEnter();
            return _battleInstance;
        }

        public void ReleaseBattle()
        {
            _battleInstance?.OnLeave();
            _battleInstance = null;
            _battleField = null;
        }

        public override void OnUpdate(float elapseTime, float realElapseTime)
        {
            _battleInstance?.OnUpdate(elapseTime, realElapseTime);
        }

        /// <summary>
        /// 播放一段竞技场战斗记录
        /// </summary>
        /// <param name="arenaBattleResult"></param>
        /// <param name="backProcedure">战斗结束后返回的procedure，默认为当前的procedure</param>
        public static void EnterArenaMatch(ArenaBattleResult arenaBattleResult, ProcedureBase backProcedure = null)
        {
            var procedureService = GameFramework.GetService<ProcedureService>();
            var procedureEntity = procedureService.ProcedureFsm;
            var enterBattle = procedureEntity.GetState<EnterBattleProcedure>();
            enterBattle.SetArenaResult(arenaBattleResult);


            GameFramework.GetService<DataNodeService>()
                .SetData(Constants.DataKeys.BattleBackProcedure, backProcedure?.GetType());

            procedureEntity.ChangeState<EnterBattleProcedure>();
        }

        /// <summary>
        /// 直接播放一段战斗记录
        /// </summary>
        /// <param name="result"></param>
        /// <param name="backProcedure">战斗结束后返回的procedure，默认为当前的procedure</param>
        public static void EnterBattle(BattleRecord result, ProcedureBase backProcedure = null)
        {
            var procedureService = GameFramework.GetService<ProcedureService>();
            var procedureEntity = procedureService.ProcedureFsm;
            var enterBattle = procedureEntity.GetState<EnterBattleProcedure>();
            enterBattle.SetBattleRecord(result);


            GameFramework.GetService<DataNodeService>()
                .SetData(Constants.DataKeys.BattleBackProcedure, backProcedure?.GetType());

            procedureEntity.ChangeState<EnterBattleProcedure>();
        }

        /// <summary>
        /// 与指定npc发生战斗交互
        /// </summary>
        /// <param name="npcId"></param>
        /// <param name="npcInfo"></param>
        /// <param name="backProcedure">战斗结束后返回的procedure，默认为当前的procedure</param>
        public static void EnterBattle(int npcId, NpcRefreshInfo npcInfo, ProcedureBase backProcedure = null)
        {
            var procedureService = GameFramework.GetService<ProcedureService>();
            var procedureEntity = procedureService.ProcedureFsm;
            var enterBattle = procedureEntity.GetState<EnterBattleProcedure>();
            enterBattle.SetBattleInfo(npcId, npcInfo);

            GameFramework.GetService<DataNodeService>()
                .SetData(Constants.DataKeys.BattleBackProcedure, backProcedure?.GetType());

            procedureEntity.ChangeState<EnterBattleProcedure>();
        }
    }
}