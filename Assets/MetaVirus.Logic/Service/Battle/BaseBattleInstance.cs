using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cfg.common;
using cfg.map;
using GameEngine;
using GameEngine.Common;
using GameEngine.DataNode;
using GameEngine.Event;
using GameEngine.FairyGUI;
using GameEngine.Fsm;
using GameEngine.ObjectPool;
using GameEngine.Procedure;
using GameEngine.Sound;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.Service.Battle.Frame;
using MetaVirus.Logic.Service.Battle.Fsm.BattleFsm;
using MetaVirus.Logic.Service.Battle.Projectile;
using MetaVirus.Logic.Service.Battle.UI;
using MetaVirus.Logic.Service.Vfx;
using MetaVirus.Logic.Utils;
using Unity.VisualScripting;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle
{
    public abstract class BaseBattleInstance
    {
        protected readonly BattleRecord BattleRecord;
        public BattleField BattleField { get; }
        public readonly Dictionary<int, BattleUnitEntity> Entities = new();

        protected float Timeline = 0;

        private FsmEntity<BaseBattleInstance> _battleFsm;

        private readonly FsmService _fsmService;
        private readonly GameDataService _gameDataService;
        private readonly BattleVfxGameService _battleVfxGameService;
        private readonly FairyGUIService _fairyService;
        private readonly EventService _eventService;
        private readonly DataNodeService _dataNodeService;
        private readonly SoundService _soundService;
        private ObjectPoolService _objectPoolService;

        private readonly Stack<ActionFrame> _frames = new();

        public BattleProjectileManager ProjectileManager { get; }

        public BattleUIManager BattleUIManager { get; }

        public float TimeScale { get; set; } = 1;

        public float DeltaTime { get; private set; }

        public int BattleId => BattleRecord?.BattleId ?? 0;

        public bool Started { get; private set; }

        private string[] _loadedPkgs;

        private MapData _battleMapData;

        /// <summary>
        /// 每次update叠加tickTime值，增加到1个tick的时长后，给所有单位tick+1
        /// </summary>
        private float _tickTime;

        public BaseBattleInstance(BattleRecord battleRecord, BattleField battleField)
        {
            BattleField = battleField;
            BattleRecord = battleRecord;
            _fsmService = GameFramework.GetService<FsmService>();
            _gameDataService = GameFramework.GetService<GameDataService>();
            _objectPoolService = GameFramework.GetService<ObjectPoolService>();
            _battleVfxGameService = GameFramework.GetService<BattleVfxGameService>();
            _fairyService = GameFramework.GetService<FairyGUIService>();
            _eventService = GameFramework.GetService<EventService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _soundService = GameFramework.GetService<SoundService>();

            ProjectileManager = new BattleProjectileManager();
            BattleUIManager = new BattleUIManager(this);
        }

        public void OnEnter()
        {
            _tickTime = 0;
            _eventService.Emit(GameEvents.BattleEvent.Battle,
                new BattleEvent(BattleId, BattleEvent.BattleEventType.Started));
            ActiveAllEntities();
            Started = true;
            _battleFsm.Start<BattleStateOverview>();
        }

        public void OnLeave()
        {
            _fsmService.DestroyFsm<BaseBattleInstance>("BattleFsm");

            foreach (var entity in Entities.Values)
            {
                entity.OnRelease();
            }

            if (_battleMapData is { BattleBgm: > 0 })
            {
                _soundService.StopBGM(_battleMapData.BattleBgm_Ref.Catalog_Ref.Name, _battleMapData.BattleBgm_Ref.Name,
                    unload: true);
            }

            _fairyService.ReleasePackages(_loadedPkgs);
            Entities.Clear();
            ProjectileManager.Clear();
            BattleUIManager.Clear();
            _battleVfxGameService.ReleaseVfxes();
        }

        public void OnUpdate(float elapseTime, float realElapseTime)
        {
            DeltaTime = elapseTime * TimeScale;

            foreach (var bue in Entities.Values)
            {
                bue.OnUpdate(DeltaTime, realElapseTime);
            }

            ProjectileManager.OnUpdate(elapseTime, realElapseTime);
            BattleUIManager.OnUpdate(elapseTime, realElapseTime);
        }

        public async Task AsyncLoadBattle(TaskProgressHandler handler = null)
        {
            var playerInfo = _dataNodeService.GetData<PlayerInfo>(DataKeys.PlayerInfo);
            _battleMapData = _gameDataService.GetMapData(playerInfo.CurrentMapId);

            var pkgTask = await _fairyService.AddPackageAsync("ui-battle");

            _loadedPkgs = pkgTask;

            _battleFsm = _fsmService.CreateFsm("BattleFsm", this,
                new BattleStateIncActionEnergy(), new BattleStateAction(), new BattleStateOverview()
            );

            //push all frames
            var frameList = BattleRecord.Frames;
            for (var i = frameList.Count - 1; i >= 0; i--)
            {
                _frames.Push(frameList[i]);
            }

            handler?.ReportProgress(10);

            //load all entities
            foreach (var entity in BattleRecord.SrcUnits.Select(unit => new BattleUnitEntity(unit, this)))
            {
                await entity.LoadEntityAsync();
                Entities[entity.Id] = entity;
            }

            foreach (var entity in BattleRecord.TarUnits.Select(unit => new BattleUnitEntity(unit, this)))
            {
                await entity.LoadEntityAsync();
                Entities[entity.Id] = entity;
            }

            handler?.ReportProgress(20);
            //get all common vfx ids
            var commonVfxes = _gameDataService.ConfigCommonVfxIds;

            //get all skill vfx ids
            var idSet = new SortedSet<int>();
            idSet.AddRange(commonVfxes);

            foreach (var entity in Entities.Values)
            {
                idSet.AddRange(_gameDataService.GetAllSkillVfxIds(entity.BattleUnit));
            }

            idSet.Remove(0);
            handler?.ReportProgress(30);


            var vfxhandler = new TaskProgressHandler();

            //load vfxes
            var task = _battleVfxGameService.AsyncLoadVfxes(idSet.ToArray(), vfxhandler);
            while (!task.IsCompleted)
            {
                await Task.Delay(10);
                handler?.ReportProgress(30 + (int)(30f * vfxhandler.Progress / 100f));
            }

            handler?.ReportProgress(60);
            //load all projectiles
            await ProjectileManager.AsyncLoadProjectiles(Entities.Values.ToArray());

            handler?.ReportProgress(70);
            //load battle ui
            await BattleUIManager.AsyncLoadBattleUI();

            handler?.ReportProgress(80);
            //load battle bgm
            if (_battleMapData is { BattleBgm: > 0 })
            {
                var battleBgm = SoundService.ToFullPath(_battleMapData.BattleBgm_Ref.Catalog_Ref.Name,
                    _battleMapData.BattleBgm_Ref.Name);
                await _soundService.AsyncLoadSoundClip(battleBgm, _battleMapData.BattleBgm_Ref.AssetAddress,
                    _battleMapData.BattleBgm_Ref.Volume,
                    _battleMapData.BattleBgm_Ref.Loop);

                _soundService.PlayBGM(battleBgm);
            }

            _eventService.Emit(GameEvents.BattleEvent.Battle,
                new BattleEvent(BattleId, BattleEvent.BattleEventType.Ready));
            handler?.ReportProgress(100);
        }

        private void ActiveAllEntities()
        {
            //active all entities
            foreach (var entity in Entities.Values)
            {
                var slotTrans = BattleField.GetSlotTrans(entity.BattleUnit);
                entity.ActiveAt(slotTrans.position, slotTrans.rotation);
            }
        }

        public IEnumerator LoadBattleCoro()
        {
            var task = AsyncLoadBattle();
            yield return task.AsCoroution();

            // var pkgTask = _fairyService.AddPackageAsync("ui-battle");
            // yield return pkgTask.AsCoroution();
            //
            // _loadedPkgs = pkgTask.Result;
            //
            // _battleFsm = _fsmService.CreateFsm("BattleFsm", this,
            //     new BattleStateIncActionEnergy(), new BattleStateAction(), new BattleStateOverview()
            // );
            //
            // //push all frames
            // var frameList = BattleRecord.Frames;
            // for (var i = frameList.Count - 1; i >= 0; i--)
            // {
            //     _frames.Push(frameList[i]);
            // }
            //
            // //load all entities
            // foreach (var entity in BattleRecord.SrcUnits.Select(unit => new BattleUnitEntity(unit, this)))
            // {
            //     var task = entity.LoadEntityAsync();
            //     yield return task.AsCoroution();
            //     Entities[entity.Id] = entity;
            // }
            //
            // foreach (var entity in BattleRecord.TarUnits.Select(unit => new BattleUnitEntity(unit, this)))
            // {
            //     var task = entity.LoadEntityAsync();
            //     yield return task.AsCoroution();
            //     Entities[entity.Id] = entity;
            // }
            //
            // //load battle ui
            // var uiTask = BattleUIManager.AsyncLoadBattleUI();
            // yield return uiTask.AsCoroution();
            //
            // //get all common vfx ids
            // var commonVfxes = _gameDataService.ConfigCommonVfxIds;
            //
            // //get all skill vfx ids
            // var idSet = new SortedSet<int>();
            // idSet.AddRange(commonVfxes);
            //
            // foreach (var entity in Entities.Values)
            // {
            //     idSet.AddRange(_gameDataService.GetAllSkillVfxIds(entity.BattleUnit));
            // }
            //
            // idSet.Remove(0);
            //
            // //load vfxes
            // var vfxTask = _battleVfxGameService.AsyncLoadVfxes(idSet.ToArray());
            // yield return vfxTask.AsCoroution();
            //
            // //load all projectiles
            // var projTask = ProjectileManager.AsyncLoadProjectiles(Entities.Values.ToArray());
            // yield return projTask.AsCoroution();
            //
            // //active all entities
            // foreach (var entity in Entities.Values)
            // {
            //     var slotTrans = BattleField.GetSlotTrans(entity.BattleUnit);
            //     entity.ActiveAt(slotTrans.position, slotTrans.rotation);
            // }
            //
            //
            // _battleFsm.Start<BattleStateOverview>();
        }

        public BattleUnitEntity GetUnitEntity(int unitId)
        {
            Entities.TryGetValue(unitId, out var entity);
            return entity;
        }

        public BattleUnitEntity GetUnitEntity(BattleUnitSide side, int slot)
        {
            var id = BattleUnit.MakeUnitId(side, slot);
            return GetUnitEntity(id);
        }

        public List<BattleUnitEntity> GetUnitEntities(BattleUnitSide side)
        {
            return Entities.Values.Where(entity => entity.BattleUnit.Side == side).ToList();
        }

        /// <summary>
        /// 获取阵型中心位置
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public Transform GetFormationCenter(BattleUnitSide side)
        {
            var slotTrans = BattleField.GetSlotTrans(side, 5);
            // var unit = GetUnitEntity(side, 5);
            // return unit == null ? slotTrans : unit.UnitAni.GetBeatTransform();
            return slotTrans;
        }

        /// <summary>
        /// 获取阵型前排中心位置
        /// </summary>
        /// <param name="side"></param>
        /// <returns></returns>
        public Transform GetFormationFrontCenter(BattleUnitSide side)
        {
            var slotTrans = BattleField.GetSlotTrans(side, 2);
            var unit = GetUnitEntity(side, 2);
            return unit == null ? slotTrans : unit.UnitAni.GetBeatTransform();
        }

        /// <summary>
        /// 获取当前可执行的ActionFrame
        /// </summary>
        /// <param name="pop">是否从栈中弹出这个ActionFrame</param>
        /// <returns></returns>
        public ActionFrame CurrentActionFrame(bool pop = false)
        {
            if (_frames.Count == 0 || _frames.Peek().FrameTime > Timeline)
            {
                return null;
            }

            return pop ? _frames.Pop() : _frames.Peek();
        }

        /// <summary>
        /// 时间线增长
        /// 增长所有单位的actionEnergy
        /// </summary>
        public void IncTimeline(float timeDelta)
        {
            var deltaMs = timeDelta * 1000;
            _tickTime += deltaMs;
            //增长时间线
            Timeline += deltaMs;

            if (_tickTime > BattleRecord.TickInterval)
            {
                //剩余的时间留到下一轮
                _tickTime %= BattleRecord.TickInterval;

                // var tickElapse = (int)(_tickTime / BattleRecord.TickInterval);
                // //每个单位增长对应的tick
                // foreach (var unit in Entities.Values)
                // {
                //     //unit.BattleUnit.IncActionEnergy(delta, BattleRecord.TickInterval);
                //     unit.BattleUnit.IncTick(tickElapse);
                // }
            }
            //为了平滑battleUnit的actionEnergy显示，在不足一个TickInterval的时间段里，也增长actionEnergy的值

            var tickElapse = deltaMs / BattleRecord.TickInterval;
            //每个单位增长对应的tick
            foreach (var unit in Entities.Values)
            {
                //unit.BattleUnit.IncActionEnergy(delta, BattleRecord.TickInterval);
                unit.BattleUnit.IncTick(tickElapse);
            }
        }
    }
}