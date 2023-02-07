using cfg.common;
using DG.Tweening;
using FairyGUI;
using GameEngine;
using MetaVirus.Logic.Data;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.UI.Pages.BattleActionBar
{
    public class BattleActionBarHeader
    {
        public enum Position
        {
            Top = 1,
            Bottom = 2,
        }

        public int Id => Entity.Id;

        public BattleUnitEntity Entity { get; }

        private GComponent _headerComp;
        private GLoader _headerLoader;
        private Position _position;
        private GComponent _parent;
        private GImage _glowFrame;

        private float _widthPerEnergy;

        private GameDataService _dataService;

        public BattleActionBarHeader(GComponent header, GComponent parent, BattleUnitEntity entity,
            Position position = Position.Top)
        {
            _dataService = GameFramework.GetService<GameDataService>();
            Entity = entity;
            _headerComp = header;
            _position = position;
            _parent = parent;

            var width = _parent.size.x;
            var energyMax = Entity.BattleUnit.RoundActionEnergy;

            _widthPerEnergy = width / energyMax;

            parent.AddChild(_headerComp);

            _glowFrame = _headerComp.GetChild("GlowFrame").asImage;
            _glowFrame.visible = true;
            _glowFrame.alpha = 0;

            var headBg = _headerComp.GetChild("n13").asImage;
            headBg.color = _dataService.QualityToColor(entity.Quality);

            _headerLoader = _headerComp.GetChildByPath("Portrait.Loader").asLoader;
            _headerLoader.url = Constants.FairyImageUrl.Header(Entity.BattleUnit.ResourceId);
            _headerLoader.SetXY(70, 70);

            _headerComp.SetPivot(0.5f, 1);
            if (position == Position.Bottom)
            {
                _headerComp.SetPivot(0.5f, 0);
            }

            _headerComp.pivotAsAnchor = true;
            //_headerComp.GetChild(arrowName).visible = true;

            _headerComp.SetXY(0, 0);
            _headerComp.visible = true;
            _headerComp.SetScale(0.5f, 0.5f);
        }

        public void SetHeaderZOrder(int order)
        {
            _headerComp.sortingOrder = order;
        }

        public void ToFocus()
        {
            DOTween.To(value => { _glowFrame.alpha = value; }, 0, 1, 0.5f);
            DOTween.To(value => { _headerComp.SetScale(value, value); }, 0.5f, 0.7f, 0.5f).SetEase(Ease.OutBack);
        }

        public void ToNormal()
        {
            DOTween.To(value => { _glowFrame.alpha = value; }, 1, 0, 0.2f);
            DOTween.To(value => { _headerComp.SetScale(value, value); }, 0.7f, 0.5f, 0.2f);
        }

        public void OnUpdate(float elapseTime, float realElapseTime)
        {
            var energy = Entity.BattleUnit.GetProperty(AttributeId.CalcActionEnergy);

            var max = Entity.BattleUnit.RoundActionEnergy;
            if (energy > max)
            {
                energy = max;
            }

            var width = _widthPerEnergy * energy;
            _headerComp.SetXY(width, 0);
            _headerComp.grayed = Entity.IsDead;
        }

        public void Release()
        {
            _headerComp.RemoveFromParent();
            _headerComp.Dispose();
        }
    }
}