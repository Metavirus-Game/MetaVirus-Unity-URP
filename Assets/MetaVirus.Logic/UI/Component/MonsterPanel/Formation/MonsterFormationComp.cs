using System.Collections.Generic;
using System.Linq;
using FairyGUI;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.UI.Component.Common;
using UnityEngine.Events;

namespace MetaVirus.Logic.UI.Component.MonsterPanel.Formation
{
    public class MonsterFormationComp
    {
        private const int ColumnSpace = 100;
        private const int RowSpace = 30;

        private readonly GComponent _container;

        private int[] _formationInfo;

        private int _selectedSlot;

        /// <summary>
        /// 返回当前阵型上的所有怪物Id集合
        /// </summary>
        public int[] SelectedPets => (from btn in _slots where btn.PetData != null select btn.PetData.Id).ToArray();

        public int[] SlotsData
        {
            get
            {
                var ret = new int[_slots.Count];
                for (var i = 0; i < _slots.Count; i++)
                {
                    ret[i] = _slots[i].PetData?.Id ?? -1;
                }

                return ret;
            }
        }

        public int SlotCount => _formationInfo.Sum();

        public int[] FormationInfo
        {
            get => _formationInfo;
            set
            {
                _formationInfo = value;
                OnSlotCountChanged();
            }
        }

        public MonsterHeaderGridButton CurrSlot => _slots[SelectedSlot];

        public int SelectedSlot
        {
            get => _selectedSlot;
            set
            {
                if (_selectedSlot >= 0 && _selectedSlot < _slots.Count)
                {
                    _selectedSlot = value;
                    for (var i = 0; i < _slots.Count; i++)
                    {
                        _slots[i].Checked = _selectedSlot == i;
                    }
                }
            }
        }

        public UnityAction<int> OnSlotClickedAction;


        private readonly List<MonsterHeaderGridButton> _slots = new();

        /// <summary>
        /// formationInfo是临时数据，需要修改，目前是 0,1,2行每一行的slot数量
        /// </summary>
        /// <param name="rowContainer"></param>
        /// <param name="formationInfo"></param>
        public MonsterFormationComp(GComponent rowContainer, int[] formationInfo)
        {
            _container = rowContainer;
            FormationInfo = formationInfo;
            SetAllSlotsEmpty();
            SelectedSlot = 0;
        }

        public void RemovePetDataFromSlot(PlayerPetData petData)
        {
            foreach (var slot in _slots)
            {
                if (slot.PetData == petData)
                {
                    slot.PetData = null;
                }
            }
        }


        /// <summary>
        /// 将阵型中对应的petData设置为选中状态
        /// </summary>
        /// <param name="petData"></param>
        public void SelectPetData(PlayerPetData petData)
        {
            for (var i = 0; i < _slots.Count; i++)
            {
                var slot = _slots[i];
                if (slot.PetData == petData)
                {
                    SelectedSlot = i;
                    break;
                }
            }
        }

        /// <summary>
        /// 设置slot对应的petData
        /// </summary>
        /// <param name="slot">slotId, 0 to 4</param>
        /// <param name="petData">slot对应的petData，如果为null则移除这个位置的petData</param>
        public void SetSlotPetData(int slot, PlayerPetData petData)
        {
            if (slot < 0 || slot >= _slots.Count)
            {
                return;
            }

            _slots[slot].PetData = petData;
        }

        public void SetSelectedSlotPetData(PlayerPetData petData)
        {
            var slot = CurrSlot;
            var moveNext = slot.PetData == null;

            slot.PetData = petData;

            if (!moveNext || petData == null) return;

            var idx = _selectedSlot + 1;
            if (idx >= _slots.Count)
            {
                idx = 0;
            }

            //find next empty slot
            while (_slots[idx].PetData != null && _slots[idx].PetData != petData)
            {
                idx++;
                if (idx >= _slots.Count)
                {
                    idx = 0;
                }
            }

            SelectedSlot = idx;
        }

        private void OnSlotCountChanged()
        {
            if (SlotCount > _slots.Count)
            {
                for (var i = _slots.Count; i < SlotCount; i++)
                {
                    var slotBtn = new MonsterHeaderGridButton();
                    _slots.Add(slotBtn);
                    _container.AddChild(slotBtn.HeaderButton);
                }
            }
            else if (SlotCount < _slots.Count)
            {
                while (_slots.Count > SlotCount)
                {
                    _slots.RemoveAt(0);
                }
            }

            RearrangeSlots();
        }

        private void OnSlotClicked(int index)
        {
            SelectedSlot = index;
            OnSlotClickedAction?.Invoke(index);
        }

        private void RearrangeSlots()
        {
            if (SlotCount == 0) return;
            var w = _container.width;
            var bw = _slots[0].HeaderButton.width;
            var bh = _slots[0].HeaderButton.height;

            var bTop = 0;
            var slotStart = 0;
            for (var row = 0; row < _formationInfo.Length; row++)
            {
                var rowCount = _formationInfo[row];
                var left = (w - (bw * rowCount + ColumnSpace * (rowCount - 1))) / 2;

                for (var i = 0; i < rowCount; i++)
                {
                    var idx = slotStart + i;
                    var btn = _slots[idx].HeaderButton;
                    btn.SetPosition(left, bTop, 0);

                    btn.onClick.Set(() => OnSlotClicked(idx));

                    left += bw + ColumnSpace;
                }

                bTop += (int)bh + RowSpace;
                slotStart += rowCount;
            }
        }

        private void SetAllSlotsEmpty()
        {
            foreach (var slot in _slots)
            {
                slot.Empty = true;
            }
        }
    }
}