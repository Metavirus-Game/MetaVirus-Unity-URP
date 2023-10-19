using cfg.common;
using DG.Tweening;
using FairyGUI;
using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;

namespace MetaVirus.Logic.Service.Battle.UI.Pages.BattleHeaderUI
{
    public class BattleHeader
    {
        public GComponent HeaderComp => _header;
        private readonly GComponent _header;
        public BattleUnitEntity Unit { get; }

        private GLoader _headerImg;
        private GImage _headerGlowFrame;
        private GTextField _headerName;
        private GProgressBar _hpBar;
        private GProgressBar _mpBar;
        private GTextField _hpValue;
        private GTextField _mpValue;

        private readonly GameDataService _dataService;

        public BattleHeader(GComponent headerComp, BattleUnitEntity unit)
        {
            _dataService = GameFramework.GetService<GameDataService>();
            _header = headerComp;
            Unit = unit;
            Load();
        }

        private void Load()
        {
            _headerImg = _header.GetChildByPath("Header.Portrait.Loader").asLoader;
            _headerGlowFrame = _header.GetChildByPath("Header.GlowFrame").asImage;
            _headerGlowFrame.visible = true;
            _headerGlowFrame.alpha = 0;

            var qualityClr = _dataService.QualityToColor(Unit.Quality);
            var img = _header.GetChildByPath("Header.n13").asImage;
            img.color = qualityClr;

            img = _header.GetChild("n4").asImage;
            img.color = qualityClr;

            img = _header.GetChild("n5").asImage;
            img.color = qualityClr;

            _headerName = _header.GetChild("TxtName").asTextField;
            _hpBar = _header.GetChild("HpBar").asProgress;
            _mpBar = _header.GetChild("MpBar").asProgress;
            _hpValue = _hpBar.GetChild("TxtValue").asTextField;
            _mpValue = _mpBar.GetChild("TxtValue").asTextField;
            var hpBarFill = _hpBar.GetChild("bar").asImage;
            var mpBarFill = _mpBar.GetChild("bar").asImage;

            hpBarFill.color = _dataService.BattleUIHpBarColor;
            mpBarFill.color = _dataService.BattleUIMpBarColor;

            _hpBar.max = Unit.BattleUnit.GetProperty(AttributeId.CalcHpMax);
            _mpBar.max = Unit.BattleUnit.GetProperty(AttributeId.CalcMpMax);

            RefreshHp();
            RefreshMp();

            _headerImg.url = Constants.FairyImageUrl.Header(Unit.BattleUnit.ResourceId);
            _headerImg.SetXY(70, 70);
        }

        public void RefreshHp()
        {
            var v = Unit.BattleUnit.GetProperty(AttributeId.CalcHp);
            _hpBar.TweenValue(v, 0.2f);
            _hpValue.text = v.ToString();
        }

        public void RefreshMp()
        {
            var v = Unit.BattleUnit.GetProperty(AttributeId.CalcMp);
            _mpBar.TweenValue(v, 0.2f);
            _mpValue.text = Unit.BattleUnit.GetProperty(AttributeId.CalcMp).ToString();
        }

        public void ToFocus()
        {
            DOTween.To(value => { _headerGlowFrame.alpha = value; }, 0, 1, 0.5f);
        }

        public void ToNormal()
        {
            DOTween.To(value => { _headerGlowFrame.alpha = value; }, 1, 0, 0.2f);
        }

        public void RefreshDead()
        {
            HeaderComp.grayed = Unit.IsDead;
        }
    }
}