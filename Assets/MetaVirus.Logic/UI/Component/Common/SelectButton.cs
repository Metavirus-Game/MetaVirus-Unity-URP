using System.Collections.Generic;
using FairyGUI;
using UnityEngine.Events;

namespace MetaVirus.Logic.UI.Component.Common
{
    public class SelectButton
    {
        private readonly GButton _btnDec;
        private readonly GButton _btnInc;
        private readonly GTextField _txt;

        private int _selectIndex = 0;

        private readonly UnityAction<SelectButtonData> _onItemSelected;
        public GComponent SelectButtonComp { get; }

        private readonly List<SelectButtonData> _items;

        public SelectButton(GComponent selectButtonComp, List<SelectButtonData> items,
            UnityAction<SelectButtonData> onItemSelected)
        {
            SelectButtonComp = selectButtonComp;
            _items = items;
            _btnDec = selectButtonComp.GetChild("btnDec").asButton;
            _btnDec.onClick.Set(() =>
            {
                _selectIndex--;
                if (_selectIndex < 0)
                {
                    _selectIndex = _items.Count - 1;
                }

                OnSelectChanged();
            });
            _btnInc = selectButtonComp.GetChild("btnInc").asButton;
            _btnInc.onClick.Set(() =>
            {
                _selectIndex++;
                if (_selectIndex >= _items.Count)
                {
                    _selectIndex = 0;
                }

                OnSelectChanged();
            });
            _txt = selectButtonComp.GetChild("txtNumber").asTextField;
            _txt.text = _items[_selectIndex].DataTitle;
            _onItemSelected = onItemSelected;
        }

        private void OnSelectChanged()
        {
            _txt.text = _items[_selectIndex].DataTitle;
            _onItemSelected?.Invoke(_items[_selectIndex]);
        }
    }
}