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
using UnityEngine.Events;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle
{
    public abstract class BaseBattleInstance
    {
        public readonly BattleRecord BattleRecord;
        public BattleField BattleField { get; }
        public readonly Dictionary<int, BattleUnitEntity> Entities = new();

        protected float Timeline = 0;

        protected FsmEntity<BaseBattleInstance> BattleFsm;

        protected readonly FsmService FsmService;
        protected readonly GameDataService GameDataService;
        protected readonly BattleVfxGameService BattleVfxGameService;
        protected readonly FairyGUIService FairyService;
        protected readonly EventService EventService;
        protected readonly DataNodeService DataNodeService;
        protected readonly SoundService SoundService;
        protected ObjectPoolService ObjectPoolService;

        protected readonly Stack<ActionFrame> Frames = new();

        /// <summary>
        /// 返回当前战斗动画帧剩余帧数，如果==0则战斗完毕
        /// </summary>
        /// <returns></returns>
        public int RemainingBattleFrames => Frames.Count;

        /// <summary>
        /// 战斗是否播放完毕
        /// </summary>
        public bool BattleCompleted => RemainingBattleFrames == 0;

        public BattleProjectileManager ProjectileManager { get; }

        public BattleUIManager BattleUIManager { get; }

        public float TimeScale { get; set; } = 1;

        public float DeltaTime { get; private set; }

        public int BattleId => BattleRecord?.BattleId ?? 0;

        public string SrcName => BattleRecord?.SrcName ?? "";
        public string TarName => BattleRecord?.TarName ?? "";

        public bool Started { get; private set; }

        private string[] _loadedPkgs;

        private MapData _battleMapData;

        /// <summary>
        /// 每次update叠加tickTime值，增加到1个tick的时长后，给所有单位tick+1
        /// </summary>
        private float _tickTime;

        public BaseBattleInstance(BattleRecord battleRecord, BattleField battleField) : this()
        {
            BattleField = battleField;
            BattleRecord = battleRecord;
        }

        protected BaseBattleInstance()
        {
            FsmService = GameFramework.GetService<FsmService>();
            GameDataService = GameFramework.GetService<GameDataService>();
            ObjectPoolService = GameFramework.GetService<ObjectPoolService>();
            BattleVfxGameService = GameFramework.GetService<BattleVfxGameService>();
            FairyService = GameFramework.GetService<FairyGUIService>();
            EventService = GameFramework.GetService<EventService>();
            DataNodeService = GameFramework.GetService<DataNodeService>();
            SoundService = GameFramework.GetService<SoundService>();

            ProjectileManager = new BattleProjectileManager();
            BattleUIManager = new BattleUIManager(this);
        }

        public void OnEnter()
        {
            _tickTime = 0;
            EventService.Emit(GameEvents.BattleEvent.Battle,
                new BattleEvent(BattleId, BattleEvent.BattleEventType.Started));
            ActiveAllEntities();
            Started = true;
            BattleFsm.Start<BattleStateOverview>();
        }

        public void Skip()
        {
            BattleFsm.ChangeState<BattleStateCompleted>();
        }

        public void OnLeave()
        {
            FsmService.DestroyFsm<BaseBattleInstance>("BattleFsm");

            foreach (var entity in Entities.Values)
            {
                entity.OnRelease();
            }

            if (_battleMapData is { BattleBgm: > 0 })
            {
                SoundService.StopBGM(_battleMapData.BattleBgm_Ref.Catalog_Ref.Name, _battleMapData.BattleBgm_Ref.Name,
                    unload: true);
            }

            FairyService.ReleasePackages(_loadedPkgs);
            Entities.Clear();
            ProjectileManager.Clear();
            BattleUIManager.Clear();
            BattleVfxGameService.ReleaseVfxes();
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

        public virtual async Task AsyncLoadBattle(TaskProgressHandler handler = null)
        {
            try
            {
                var playerInfo = DataNodeService.GetData<PlayerInfo>(DataKeys.PlayerInfo);
                _battleMapData = GameDataService.GetMapData(playerInfo.CurrentMapId);

                var pkgTask = await FairyService.AddPackageAsync("ui-battle");

                _loadedPkgs = pkgTask;

                BattleFsm = FsmService.CreateFsm("BattleFsm", this,
                    new BattleStateIncActionEnergy(),
                    new BattleStateAction(),
                    new BattleStateOverview(),
                    new BattleStateCompleted()
                );

                //push all frames
                var frameList = BattleRecord.Frames;
                for (var i = frameList.Count - 1; i >= 0; i--)
                {
                    Frames.Push(frameList[i]);
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
                var commonVfxes = GameDataService.ConfigCommonVfxIds;

                //get all skill vfx ids
                var idSet = new SortedSet<int>();
                idSet.AddRange(commonVfxes);

                foreach (var entity in Entities.Values)
                {
                    idSet.AddRange(GameDataService.GetAllSkillVfxIds(entity.BattleUnit));
                }

                idSet.Remove(0);
                handler?.ReportProgress(30);


                var vfxhandler = new TaskProgressHandler();

                //load vfxes
                var task = BattleVfxGameService.AsyncLoadVfxes(idSet.ToArray(), vfxhandler);
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
                    await SoundService.AsyncLoadSoundClip(battleBgm, _battleMapData.BattleBgm_Ref.AssetAddress,
                        _battleMapData.BattleBgm_Ref.Volume,
                        _battleMapData.BattleBgm_Ref.Loop);

                    SoundService.PlayBGM(battleBgm);
                }

                EventService.Emit(GameEvents.BattleEvent.Battle,
                    new BattleEvent(BattleId, BattleEvent.BattleEventType.Ready));
                handler?.ReportProgress(100);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogException(e);
            }
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
        public virtual Transform GetFormationCenter(BattleUnitSide side)
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
        public virtual Transform GetFormationFrontCenter(BattleUnitSide side)
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
            if (Frames.Count == 0 || Frames.Peek().FrameTime > Timeline)
            {
                return null;
            }

            return pop ? Frames.Pop() : Frames.Peek();
        }


        /// <summary>
        /// 时间线增长
        /// 增长所有单位的actionEnergy
        /// </summary>
        public void IncTimeline(float timeDelta)
        {
            if (BattleRecord == null)
            {
                return;
            }

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

        public abstract void ShowBattleResult(UnityAction onExit, UnityAction onReplay);
    }
}