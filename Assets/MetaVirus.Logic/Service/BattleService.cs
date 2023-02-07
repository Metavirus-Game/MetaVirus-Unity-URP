using System.Collections;
using System.Threading.Tasks;
using GameEngine.Base;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Utils;

namespace MetaVirus.Logic.Service
{
    public class BattleService : BaseService
    {
        private BaseBattleInstance _battleInstance;
        private BattleField _battleField;

        public BaseBattleInstance CurrentBattle => _battleInstance;
        public BattleField BattleField => _battleField;

        private float _timeScale = 1;

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

        
        public async Task<BaseBattleInstance> AsyncRunBattle(BattleRecord battleRecord, TaskProgressHandler handler)
        {
            handler?.ReportProgress(0);
            _battleField = FindObjectOfType<BattleField>();
            var bi = new NormalBattleInstance(battleRecord, _battleField);
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
    }
}