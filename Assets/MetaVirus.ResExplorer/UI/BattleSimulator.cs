using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using cfg.battle;
using cfg.common;
using cfg.skill;
using GameEngine;
using GameEngine.Utils;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Battle.Fsm.BattleFsm;
using MetaVirus.Logic.Service.Battle.Instruction;
using MetaVirus.Logic.Service.Vfx;
using MetaVirus.Logic.Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

namespace MetaVirus.ResExplorer.UI
{
    public class BattleSimulator : BaseBattleInstance
    {
        private BattleArea _battleArea;

        private static BattleSimulator _inst;
        private BattleVfxGameService _vfxGameService;

        public static BattleSimulator Inst => _inst;

        private BattleUnitEntity _attack;

        private BattleUnitEntity[] _defence;

        private static readonly string EntityGroupName = "BattleSimulator_EntityGroup";

        internal BattleSimulator(BattleArea battleArea) : base(null, battleArea.GetComponent<BattleField>())
        {
            _battleArea = battleArea;
            _inst = this;
            _vfxGameService = GameFramework.GetService<BattleVfxGameService>();
            GameFramework.Inst.StartCoroutine(LoadSimulator());
        }

        public override Transform GetFormationCenter(BattleUnitSide side)
        {
            return _battleArea.defenceCenter;
        }

        private IEnumerator LoadSimulator()
        {
            var commonVfxes = GameDataService.ConfigCommonVfxIds;
            var idSet = new SortedSet<int>();
            idSet.AddRange(commonVfxes);

            var vfxTask = BattleVfxGameService.AsyncLoadVfxes(idSet.ToArray());
            yield return vfxTask.AsCoroution();

            BattleFsm = FsmService.CreateFsm("BattleFsm", this,
                new BattleStateIncActionEnergy(),
                new BattleStateAction(),
                new BattleStateOverview(),
                new BattleStateCompleted()
            );

            // load 1 attack
            var attackId = BattleArea.RandomUnitId();
            var attack = BattleUnit.FromMonsterData(((int)BattleUnitSide.Source << 16) | attackId, attackId, 1,
                BattleUnitSide.Source, 1);

            attack.SetProperty(AttributeId.CalcHpMax, 100);
            attack.SetProperty(AttributeId.CalcHp, 100);

            _attack = new BattleUnitEntity(attack, this);
            var task = _attack.LoadEntityAsync();
            yield return task.AsCoroution();
            Entities.Add(_attack.Id, _attack);

            _attack.ActiveAt(_battleArea.attackUnit.transform.position, _battleArea.attackUnit.transform.rotation);

            _defence = new BattleUnitEntity[5];
            for (var i = 0; i < 5; i++)
            {
                var defenceId = BattleArea.RandomUnitId();
                var defence = BattleUnit.FromMonsterData(((int)BattleUnitSide.Target << 16) | defenceId, defenceId, 1,
                    BattleUnitSide.Target, i + 1);

                defence.SetProperty(AttributeId.CalcHpMax, 100);
                defence.SetProperty(AttributeId.CalcHp, 100);

                _defence[i] = new BattleUnitEntity(defence, this);
                task = _defence[i].LoadEntityAsync();
                yield return task.AsCoroution();


                _defence[i].ActiveAt(_battleArea.defenceSlots[i].transform.position,
                    _battleArea.defenceSlots[i].transform.rotation);
                Entities.Add(_defence[i].Id, _defence[i]);
            }

            BattleFsm.Start<BattleStateIncActionEnergy>();
        }

        public void SwitchAllEntities(bool show)
        {
            foreach (var battleUnitEntity in Entities.Values)
            {
                battleUnitEntity.GameObject.SetActive(show);
            }
        }

        public override void ShowBattleResult(UnityAction onExit, UnityAction onReplay)
        {
        }

        public override async Task AsyncLoadBattle(TaskProgressHandler handler = null)
        {
        }

        public void DoProjectileAttack(ProjectileData pd, UnityAction onFinished)
        {
            var targets = new FrameSkillCastDataPb[_defence.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                var entity = _defence[i];
                var pb = new FrameSkillCastDataPb();
                targets[i] = pb;

                pb.CastData = new SkillCastDataPb
                {
                    TargetId = entity.Id,
                    SkillState = (int)SkillCastStateType.Hit,
                    SkillAttribute = (int)AtkAttribute.Magic,
                    EffectAttr = (int)AttributeId.CalcHp,
                    EffectValue = 0,
                    ValueType = (int)SkillCastValueType.Decrease,
                    Index = i,
                };
            }

            var instruction = new ProjectileSimulatorInstruction(pd, targets);
            _attack.RunInstruction(instruction);
        }
    }
}