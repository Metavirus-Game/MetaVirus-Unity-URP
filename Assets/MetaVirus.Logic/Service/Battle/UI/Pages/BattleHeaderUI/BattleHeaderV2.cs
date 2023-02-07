using cfg.common;
using DG.Tweening;
using FairyGUI;
using GameEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Battle.UI.Pages.BattleHeaderUI
{
    public class BattleHeaderV2
    {
        public BattleUnitEntity Unit => _unit;
        public GComponent Component => _headerComp;

        private readonly GameDataService _dataService;

        private GComponent _headerComp;
        private BattleUnitEntity _unit;
        private GLoader _headerImg;

        private bool _isTop = false;


        //controls
        private GProgressBar _hpBar;
        private GProgressBar _mpBar;
        private GComponent _header;
        private GLoader _headerBg;
        private GLoader _levelBg;
        private GTextField _txtLevel;


        public BattleHeaderV2(GComponent headerComp, BattleUnitEntity unit, bool isTop = false)
        {
            _isTop = isTop;
            _dataService = GameFramework.GetService<GameDataService>();
            _headerComp = headerComp;
            _unit = unit;

            Load();
        }

        private void Load()
        {
            _header = _headerComp.GetChild("Header").asCom;
            //header icon
            _headerImg = _header.GetChild("PortraitLoader").asLoader;
            _headerImg.url = FairyImageUrl.Header(Unit.BattleUnit.ResourceId);

            //header bg 
            _headerBg = _header.GetChild("card_bg").asLoader;
            _headerBg.url = FairyImageUrl.HeaderBg(_unit.Quality);

            //level
            _levelBg = _header.GetChild("card_level_bg").asLoader;
            _txtLevel = _header.GetChild("card_level_txt").asTextField;

            _levelBg.url = FairyImageUrl.LevelFlag(_unit.Quality);
            _txtLevel.text = _unit.BattleUnit.Level.ToString();

            //hp mp bar
            _hpBar = _headerComp.GetChild("HpBar").asProgress;
            _mpBar = _headerComp.GetChild("MpBar").asProgress;

            var hpBarFill = _hpBar.GetChild("bar").asImage;
            var mpBarFill = _mpBar.GetChild("bar").asImage;

            hpBarFill.color = _dataService.BattleUIHpBarColor;
            mpBarFill.color = _dataService.BattleUIMpBarColor;

            _hpBar.max = Unit.BattleUnit.GetProperty(AttributeId.CalcHpMax);
            _mpBar.max = Unit.BattleUnit.GetProperty(AttributeId.CalcMpMax);


            RefreshHp();
            RefreshMp();
        }

        public void RefreshHp()
        {
            var v = Unit.BattleUnit.GetProperty(AttributeId.CalcHp);
            _hpBar.TweenValue(v, 0.2f);
        }

        public void RefreshMp()
        {
            var v = Unit.BattleUnit.GetProperty(AttributeId.CalcMp);
            _mpBar.TweenValue(v, 0.2f);
        }

        public void ToFocus()
        {
            var startValue = _isTop ? _headerComp.height : 0;
            var endValue = _isTop ? _headerComp.height + 40 : -40;
            DOTween.To(value => { _header.y = value; }, startValue, endValue, 0.3f);
        }

        public void ToNormal()
        {
            var startValue = _isTop ? _headerComp.height + 40 : -40;
            var endValue = _isTop ? _headerComp.height : 0;
            DOTween.To(value => { _header.y = value; }, startValue, endValue, 0.3f);
        }

        public void RefreshDead()
        {
            _headerComp.grayed = Unit.IsDead;
        }

        public void Release()
        {
            _headerComp.RemoveFromParent();
            _headerComp.Dispose();
        }
    }
}