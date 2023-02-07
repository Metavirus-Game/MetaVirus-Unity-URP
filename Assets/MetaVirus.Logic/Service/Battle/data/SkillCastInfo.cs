using System.Collections.Generic;
using System.Linq;
using MetaVirus.Battle.Record;

namespace MetaVirus.Logic.Service.Battle.data
{
    public class SkillCastInfo
    {
        public BattleUnitEntity ActionUnit { get; }
        public SkillInfo CastSkill { get; }

        public int[] TargetIds { get; }

        private readonly List<SkillCastDataInfo> _castDataInfos = new();

        private readonly Dictionary<int, SkillAttachEffectInfo> _targetAttachEffects = new();

        private readonly Dictionary<int, List<BuffAttachDataPb>> _targetAttachBuffs = new();

        public List<SkillCastDataInfo> CastDatas => _castDataInfos;

        private SkillCastActionFramePb _castAction;

        public SkillCastInfo(BattleUnitEntity actionUnit, SkillInfo castSkill, SkillCastActionFramePb castAction)
        {
            ActionUnit = actionUnit;
            CastSkill = castSkill;

            foreach (var castData in castAction.CastList)
            {
                var info = new SkillCastDataInfo(castData.CastData);
                _castDataInfos.Add(info);
                foreach (var effect in castData.AttachEffects)
                {
                    var attachInfo = GetAttachEffects(effect.TargetId, true);
                    attachInfo.AddEffect(effect);
                }

                foreach (var buff in castData.AttachBuffs)
                {
                    var buffs = GetAttachBuffs(buff.TargetId, true);
                    buffs.Add(buff);
                }
            }

            TargetIds = _castDataInfos.Select(info => info.TargetId).ToArray();
        }

        public SkillCastInfo(BattleUnitEntity actionUnit, SkillInfo castSkill, SkillCastDataInfo castData)
        {
            ActionUnit = actionUnit;
            CastSkill = castSkill;
            _castDataInfos.Add(castData);
            TargetIds = _castDataInfos.Select(info => info.TargetId).ToArray();
        }

        /// <summary>
        /// 返回技能给目标施加的附加效果
        /// </summary>
        /// <param name="targetId"></param>
        /// <param name="autoCreate"></param>
        /// <returns></returns>
        private SkillAttachEffectInfo GetAttachEffects(int targetId, bool autoCreate)
        {
            if (!_targetAttachEffects.ContainsKey(targetId) && autoCreate)
            {
                _targetAttachEffects[targetId] = new SkillAttachEffectInfo();
            }

            _targetAttachEffects.TryGetValue(targetId, out var effect);
            return effect;
        }

        public SkillAttachEffectInfo GetAttachEffects(int targetId)
        {
            return GetAttachEffects(targetId, false);
        }

        private List<BuffAttachDataPb> GetAttachBuffs(int targetId, bool autoCreate)
        {
            if (!_targetAttachBuffs.ContainsKey(targetId) && autoCreate)
            {
                _targetAttachBuffs[targetId] = new List<BuffAttachDataPb>();
            }

            _targetAttachBuffs.TryGetValue(targetId, out var buffs);
            return buffs;
        }

        public List<BuffAttachDataPb> GetAttachBuffs(int targetId)
        {
            return GetAttachBuffs(targetId, false);
        }
    }
}