using System.Collections.Generic;
using System.Linq;
using cfg.skill;
using MetaVirus.Battle.Record;
using MetaVirus.Logic.Service.Battle.Instruction;
using MetaVirus.Logic.Service.Vfx;

namespace MetaVirus.Logic.Service.Battle.data
{
    public class SkillAttachEffectInfo
    {
        public int[] EffectIds => _effIdxList.ToArray();

        private readonly List<int> _effIdxList = new();

        private readonly Dictionary<int, List<SkillCastDataInfo>> _castDataInfos = new();

        public List<SkillCastDataInfo> AllAttachInfo
        {
            get
            {
                var ret = new List<SkillCastDataInfo>();
                foreach (var skillCastDataInfos in _castDataInfos.Values)
                {
                    ret.AddRange(skillCastDataInfos);
                }

                return ret;
            }
        }

        public void AddEffect(SkillCastDataPb castData)
        {
            if (!_effIdxList.Contains(castData.Index))
            {
                _effIdxList.Add(castData.Index);
            }

            AddEffectToList(castData);
        }

        private void AddEffectToList(SkillCastDataPb castData)
        {
            var has = _castDataInfos.TryGetValue(castData.Index, out var list);
            if (!has)
            {
                list = new List<SkillCastDataInfo>();
                _castDataInfos[castData.Index] = list;
            }

            var found = list.FirstOrDefault(data => data.TargetId == castData.TargetId);

            if (found != null)
            {
                found.EffectValue += castData.EffectValue;
            }
            else
            {
                list.Add(new SkillCastDataInfo(castData));
            }
        }

        public List<SkillCastDataInfo> GetEffectCastDataList(int effectId)
        {
            var succ = _castDataInfos.TryGetValue(effectId, out var list);
            return succ ? list : new List<SkillCastDataInfo>();
        }

        public void ApplyEffect(BaseBattleInstance battle, SkillInfo skill, AttachEffect attachEffect,
            SkillCastDataInfo castData, float damagePercent = 1)
        {
            var effect = castData.Data;
            var effInst = new OnDamageInstruction(effect, skill, damagePercent, attachEffect);
            var tar = battle.GetUnitEntity(effect.TargetId);
            tar?.RunInstruction(effInst);
        }

        public void ApplyEffects(BaseBattleInstance battle, int effectId, SkillInfo skill, AttachEffect attachEffect,
            float damagePercent = 1)
        {
            var list = GetEffectCastDataList(effectId);
            foreach (var castData in list)
            {
                ApplyEffect(battle, skill, attachEffect, castData, damagePercent);
            }
        }
    }
}