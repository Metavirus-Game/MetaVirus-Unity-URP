using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cfg.buff;
using cfg.common;
using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.Fsm;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;
using MetaVirus.Logic.Service.Battle.data;
using MetaVirus.Logic.Service.Battle.Fsm.BattleUnitFsm;
using MetaVirus.Logic.Service.Battle.Instruction;
using MetaVirus.Logic.Service.Vfx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle
{
    public class BattleUnitEntity
    {
        public int Id => BattleUnit.Id;

        public BattleUnit BattleUnit { get; }

        public Quality Quality => (Quality)BattleUnit.Quality;
        public BaseBattleInstance BattleInstance { get; }

        public BattleUnitAni UnitAni { get; private set; }

        public GameObject GameObject { get; private set; }
        public Transform Transform => GameObject.transform;

        private readonly FsmEntity<BattleUnitEntity> _fsm;
        private readonly FsmService _fsmService;

        private readonly List<BattleInstruction> _instructions = new List<BattleInstruction>();
        public BattleInstruction CurrInstruction => _instructions.Count > 0 ? _instructions[0] : null;

        private readonly Dictionary<int, UnitBuffAttached> _buffAttacheds = new();

        private BattleVfxGameService _vfxGameService;

        private EventService _eventService;

        public bool IsDead
        {
            get => BattleUnit.IsDead;
            set
            {
                if (BattleUnit.IsDead != value)
                {
                    BattleUnit.IsDead = value;
                    _eventService.Emit(GameEvents.BattleEvent.OnUnitAction,
                        new BattleOnUnitAction(this, BattleOnUnitAction.Action.Dead));
                }
            }
        }

        public BattleUnitEntity(BattleUnit battleUnit, BaseBattleInstance battleInstance)
        {
            BattleUnit = battleUnit;
            BattleInstance = battleInstance;
            _fsmService = GameFramework.GetService<FsmService>();
            _eventService = GameFramework.GetService<EventService>();
            _fsm = _fsmService.CreateFsm("BattleUnitEntity_" + BattleUnit.Id, this,
                new UnitStateIdle(), new UnitStateCastSkill(), new UnitStateChangeProperties(),
                new UnitStateAttachBuff(), new UnitStateBuffEffect(), new UnitStateOnDamage(), new UnitStateDead(),
                new UnitStateRelive()
            );

            _vfxGameService = GameFramework.GetService<BattleVfxGameService>();
        }

        public async Task<BattleUnitEntity> LoadEntityAsync()
        {
            var npcResAddress = ResAddress.BattleUnitRes(BattleUnit.ResourceId);

            var npcResObj = await Addressables.InstantiateAsync(npcResAddress).Task;
            Object.DontDestroyOnLoad(npcResObj);

            var scale = BattleUnit.Scale;
            npcResObj.transform.localScale = new Vector3(scale, scale, scale);
            npcResObj.SetActive(false);

            GameObject = npcResObj;
            UnitAni = GameObject.GetComponent<BattleUnitAni>();
            if (UnitAni == null)
            {
                UnitAni = GameObject.AddComponent<BattleUnitAni>();
            }

            UnitAni.OnAniEvent += OnBattleAniEvent;
            UnitAni.BattleInstance = BattleInstance;
            UnitAni.BattleUnit = BattleUnit;

            GameObject.name = LogName;

            _fsm.Start<UnitStateIdle>();

            return this;
        }

        private void OnBattleAniEvent(string evtName)
        {
        }

        public void OnUpdate(float timeElapse, float realTimeElapse)
        {
        }

        /// <summary>
        /// 在指定位置激活当前单位
        /// </summary>
        /// <param name="position"></param>
        /// <param name="rotation"></param>
        public void ActiveAt(Vector3 position, Quaternion rotation)
        {
            GameObject.transform.position = position;
            GameObject.transform.rotation = rotation;
            GameObject.SetActive(true);
            UnitAni.OnActive();
        }

        public UnitBuffAttached AttachBuff(int buffId, BuffInfo buffInfo, int remaining, int casterId,
            SkillInfo srcSkill)
        {
            var buff = new UnitBuffAttached(buffId, buffInfo, remaining, casterId, srcSkill);
            _buffAttacheds[buffId] = buff;

            if (buff.AttachVfx == 0) return buff;

            //带有attach vfx
            var loaded = _vfxGameService.IsVfxLoaded(buff.AttachVfx);
            var bindPos = UnitAni.GetVfxBindPos(buff.AttachVfx);
            if (loaded)
            {
                buff.AttachVfxObj =
                    _vfxGameService.InstanceVfx(buff.AttachVfx, bindPos.gameObject, false, false);
            }

            return buff;
        }

        public void ClearBuffs()
        {
            foreach (var buffId in _buffAttacheds.Keys.ToArray())
            {
                RemoveBuff(buffId);
            }
        }

        public UnitBuffAttached ChangeBuffRemaining(int buffId, int remaining)
        {
            var buff = GetAttachedBuff(buffId);
            if (buff != null)
            {
                buff.Remaining = remaining;
            }

            return buff;
        }

        public void RemoveBuff(int buffId)
        {
            var buff = GetAttachedBuff(buffId);
            if (buff != null)
            {
                _buffAttacheds.Remove(buffId);
                _vfxGameService.ReleaseVfxInst(buff.AttachVfxObj);
            }
        }

        public UnitBuffAttached GetAttachedBuff(int buffId)
        {
            _buffAttacheds.TryGetValue(buffId, out var buff);
            return buff;
        }


        public void RunInstruction(BattleInstruction instruction)
        {
            if (IsDead)
            {
                //死亡状态系下只执行死亡和复活指令
                if (instruction is DeadInstruction or ReliveInstruction)
                {
                    _instructions.Add(instruction);
                }
                else
                {
                    instruction.SetDone();
                }

                return;
            }

            if (instruction.AddMethod == InstructionAddMethod.AddLast)
            {
                _instructions.Add(instruction);
            }
            else
            {
                var i = 0;
                for (i = 0; i < _instructions.Count; i++)
                {
                    if (_instructions[i].AddMethod != InstructionAddMethod.AddFirst)
                    {
                        break;
                    }
                }

                _instructions.Insert(i, instruction);
            }

            instruction.OnAdded();
        }

        public void RemoveInstruction(BattleInstruction instruction)
        {
            _instructions.Remove(instruction);
            instruction.OnRemoved();
        }

        public void OnInstructionStateChanged(BattleInstruction inst)
        {
            if (inst.State == InstructionState.Done)
            {
                _instructions.Remove(inst);
                inst.OnRemoved();
            }
        }


        public void OnRelease()
        {
            Addressables.ReleaseInstance(GameObject);
            _fsmService.DestroyFsm<BattleUnitEntity>(_fsm.Name);
        }

        public float SetProperty(AttributeId attributeId, int value)
        {
            var oldValue = BattleUnit.GetProperty(attributeId);
            BattleUnit.SetProperty(attributeId, value);

            _eventService.Emit(GameEvents.BattleEvent.OnUnitPropertiesChanged,
                new BattleUnitPropertiesChangedEvent(this, attributeId, oldValue, value));

            return value;
        }


        public float IncProperty(AttributeId attributeId, int value)
        {
            var oldValue = BattleUnit.GetProperty(attributeId);
            var newValue = BattleUnit.IncProperty(attributeId, value);

            _eventService.Emit(GameEvents.BattleEvent.OnUnitPropertiesChanged,
                new BattleUnitPropertiesChangedEvent(this, attributeId, oldValue, newValue));

            return newValue;
        }

        public float DecProperty(AttributeId attributeId, int value)
        {
            var oldValue = BattleUnit.GetProperty(attributeId);
            var v = BattleUnit.DecProperty(attributeId, value);

            _eventService.Emit(GameEvents.BattleEvent.OnUnitPropertiesChanged,
                new BattleUnitPropertiesChangedEvent(this, attributeId, oldValue, v));

            if (attributeId == AttributeId.CalcHp && v == 0 && !IsDead)
            {
                IsDead = true;
                RunInstruction(new DeadInstruction());
            }

            return v;
        }

        public float IncProperty(int attributeId, int value)
        {
            return IncProperty((AttributeId)attributeId, value);
        }

        public float DecProperty(int attributeId, int value)
        {
            return DecProperty((AttributeId)attributeId, value);
        }

        public void OnTimeScaleChanged()
        {
        }

        public string LogName => $"[{Id:x5}]{BattleUnit.Name} Lv.{BattleUnit.Level}";
    }
}