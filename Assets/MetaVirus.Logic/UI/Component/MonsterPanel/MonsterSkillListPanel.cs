using cfg.skill;
using FairyGUI;
using MetaVirus.Logic.Data.Provider;

namespace MetaVirus.Logic.UI.Component.MonsterPanel
{
    public class MonsterSkillListPanel
    {
        private readonly GComponent _abilityPanel;
        private GList _lstAbilityLearned;

        private IMonsterDataProvider _dataProvider;

        private const int TitleHeight = 46 + 70;
        private const int ListLearnedSpace = 25;

        public IMonsterDataProvider DataProvider
        {
            get => _dataProvider;
            set
            {
                _dataProvider = value;
                OnDataProviderChanged();
            }
        }

        private void OnDataProviderChanged()
        {
            _lstAbilityLearned.numItems = _dataProvider.Skills.Length;
            _lstAbilityLearned.ResizeToFit();

            _abilityPanel.height = TitleHeight + ListLearnedSpace + _lstAbilityLearned.size.y + ListLearnedSpace;
        }

        public MonsterSkillListPanel(GComponent abilityPanel)
        {
            _abilityPanel = abilityPanel;
            LoadComponents();
        }

        private void LoadComponents()
        {
            _lstAbilityLearned = _abilityPanel.GetChild("lstLearned").asList;
            _lstAbilityLearned.itemRenderer = AbilityListItemRenderer;
        }


        private void AbilityListItemRenderer(int index, GObject item)
        {
            var skillInfo = _dataProvider.Skills[index];
            var skillData = skillInfo.BattleSkillData;
            var comp = item.asCom;

            var typeCtrl = comp.GetController("skillType");
            var mpCtrl = comp.GetController("showMp");
            var atkTypeCtrl = comp.GetController("atkType");

            typeCtrl.SetSelectedIndex((int)skillData.Type);

            var mpUsage = skillInfo.LevelInfo?.MpUsage ?? 0;
            mpCtrl.SetSelectedIndex(mpUsage == 0 ? 0 : 1);
            atkTypeCtrl.SetSelectedIndex(skillData.AtkAttribute == AtkAttribute.Physical ? 0 : 1);

            var txtRange = comp.GetChild("txtRange").asTextField;
            txtRange.text = skillData.AtkScope == SkillScope.All ? "All" : "1";

            var txtLevel = comp.GetChild("txtLevel").asTextField;
            txtLevel.text = skillInfo.Level.ToString();

            var txtName = comp.GetChild("txtName").asTextField;
            txtName.text = skillData.Name;

            var txtMpCost = comp.GetChild("txtMpCost").asTextField;
            txtMpCost.text = mpUsage.ToString();

            var txtDesc = comp.GetChild("txtDesc").asRichTextField;
            txtDesc.text = skillInfo.SkillDesc;

            var lstAttach = comp.GetChild("lstAttach").asList;

            //var effsInfo = skillInfo.AttachEffects;
            var buffs = skillInfo.AttachBuffs;

            lstAttach.itemRenderer = (idx, obj) =>
            {
                var compEff = obj.asCom;
                var buff = buffs[idx];
                compEff.GetChild("text").text = buff.BuffDesc;
            };

            lstAttach.numItems = buffs.Length;

            // lstAttach.itemRenderer = (idx, obj) =>
            // {
            //     var compEff = obj.asCom;
            //     var effInfo = effsInfo[idx];
            //
            //     var iconLoader = compEff.GetChild("icon").asLoader;
            //     iconLoader.url = effInfo.EffectIcon;
            //     iconLoader.color = effInfo.EffectColor;
            //     compEff.GetChild("text").text = effInfo.EffectDesc;
            // };
            //
            // lstAttach.numItems = effsInfo.Length;
        }
    }
}