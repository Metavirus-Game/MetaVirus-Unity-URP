using System.Collections.Generic;
using System.Linq;
using FairyGUI;

namespace MetaVirus.Logic.Service.Battle.UI.Pages.BattleHeaderUI
{
    public class BattleHeaderContainer
    {
        private GComponent _container;

        private readonly List<BattleHeaderV2> _headers = new();

        private readonly Dictionary<BattleUnitEntity, BattleHeaderV2> _unit2Header = new();

        private readonly int _arrangeSpace;

        public BattleHeaderV2 this[BattleUnitEntity index]
        {
            get
            {
                _unit2Header.TryGetValue(index, out var unit);
                return unit;
            }
        }

        public BattleHeaderContainer(GComponent container, int arrangeSpace = 15)
        {
            _container = container;
            _arrangeSpace = arrangeSpace;
        }

        public void AddHeader(BattleHeaderV2 battleHeader)
        {
            _headers.Add(battleHeader);
            _unit2Header[battleHeader.Unit] = battleHeader;
            _container.AddChild(battleHeader.Component);
            Rearrange();
        }

        private void Rearrange()
        {
            var containerWidth = _container.width;
            var totalWidth = _headers.Sum(header => header.Component.width);

            totalWidth += _arrangeSpace * _headers.Count - 1;

            var left = (containerWidth - totalWidth) / 2;

            foreach (var header in _headers)
            {
                header.Component.SetXY(left + header.Component.width / 2, 0);
                left += header.Component.width + _arrangeSpace;
            }
        }

        public void Release()
        {
            foreach (var header in _headers)
            {
                header.Release();
            }

            _container.RemoveFromParent();
            _container.Dispose();
        }
    }
}