using System.Collections.Generic;
using cfg.common;
using cfg.skill;
using GameEngine;
using GameEngine.Utils;
using MetaVirus.Logic.AttrsCalculator;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Logic.Service;
using UnityEngine;

namespace MetaVirus.Logic.Data.Battle
{
    public class MonsterSkillAttachEffectInfo
    {
        private GameDataService _gameDataService;
        private readonly IMonsterDataProvider _skillOwner;

        private AttachEffect _attachEffect;

        public string EffectIcon
        {
            get
            {
                var icon = Constants.AtkAttributeToImageName(_attachEffect.Attribute);
                return string.IsNullOrEmpty(icon) ? "" : Constants.FairyImageUrl.Common(icon);
            }
        }

        public Color EffectColor => Constants.AtkAttributeToColor(_attachEffect.Attribute)[0];

        public MonsterSkillAttachEffectInfo(AttachEffect attachEffect, IMonsterDataProvider skillOwner)
        {
            _attachEffect = attachEffect;
            _skillOwner = skillOwner;
            _gameDataService = GameFramework.GetService<GameDataService>();
        }

        public string EffectDesc
        {
            get
            {
                // 计算属性来源是自身，或者计算方式是绝对值
                // if (_attachEffect.CalcSource == AttachEffectSource.Self ||
                //     _attachEffect.Value.CalcType == AtkCalcType.Value)
                // {
                //     return MakeEffectDesc_Self();
                // }
                //
                // if (_attachEffect.CalcSource == AttachEffectSource.Target)
                // {
                //     return MakeEffectDesc_Target();
                // }


                return MakeEffectDesc();
            }
        }


        private string MakeEffectDesc()
        {
            var key = "battle.skill.attach.desc.target.damage";
            if (_attachEffect.Type == AttachEffectType.Heal)
            {
                key = "battle.skill.attach.desc.target.healing";
            }

            var calcAttr = _attachEffect.CalcAttribute;
            if (calcAttr == AttributeId.AttrNone)
            {
                calcAttr = _attachEffect.Attribute == AtkAttribute.Physical
                    ? AttributeId.CalcAtk
                    : AttributeId.CalcMAtk;
            }

            var damage = BattleCalculator.CalcDamage(calcAttr, _attachEffect.Value, _skillOwner);

            var clr = EffectColor.ToHtmlColor();

            var dmgStr = $"[color={clr}]{damage}[/color]";

            if (damage == 0 || _attachEffect.Target == AttachEffectTarget.Self)
            {
                dmgStr = "";
            }

            if (_attachEffect.CalcSource == AttachEffectSource.Target)
            {
                dmgStr = "";
            }

            var atkAttr =
                $"[color={clr}] <img src='{EffectIcon}' width='30' height='30' scale='Scale' color='{clr}'/> {_gameDataService.GetAtkAttributeName(_attachEffect.Attribute)}[/color]";

            var map = new Dictionary<string, string>
            {
                { "%damage", dmgStr },
                { "%atkAttr", atkAttr }
            };

            return GameDataService.LT(key, map);
        }
    }
}