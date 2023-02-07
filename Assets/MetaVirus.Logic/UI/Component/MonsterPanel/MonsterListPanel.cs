using System;
using cfg.common;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.Common;
using MetaVirus.Logic.UI.Windows;
using UnityEngine.Events;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.UI.Component.MonsterPanel
{
    public class MonsterListPanel
    {
        private PlayerService _playerService;
        private DataNodeService _dataNodeService;

        private MonsterSort _sortMethod = MonsterSort.BySpecies;

        private PlayerPetDataGroup<MonsterType> _petBySpecies;
        private PlayerPetDataGroup<Quality> _petByQuality;

        public GList Panel { get; }

        // private bool _showOwned;
        // private bool _showSelected;

        private int[] _selectedPets = Array.Empty<int>();

        public int[] SelectedPets
        {
            get => _selectedPets;
            set
            {
                _selectedPets = value;
                OnSettingChanged();
            }
        }

        private int[] _checkedPets = Array.Empty<int>();

        public int[] CheckedPets
        {
            get => _checkedPets;
            set
            {
                _checkedPets = value;
                OnSettingChanged();
            }
        }

        public UnityAction<GComponent, PlayerPetData> OnItemSelected;

        public MonsterSort SortMethod
        {
            set
            {
                if (_sortMethod == value) return;
                _sortMethod = value;
                OnSettingChanged();
            }
        }

        public MonsterListPanel(GList list)
        {
            Panel = list;
        }

        public void LoadData()
        {
            _playerService = GameFramework.GetService<PlayerService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();

            if (string.IsNullOrEmpty(Panel.defaultItem))
            {
                Panel.defaultItem = "ui://5jodq0f6s7qw8m";
            }


            // _monstersList = Panel.GetChild("listMonsters").asList;
            Panel.itemRenderer = RenderMonsterList;

            LoadPetData();
            RearrangeListItems();
        }

        void OnSettingChanged()
        {
            LoadPetData();
            RearrangeListItems();
        }

        private void RearrangeListItems()
        {
            var objs = Panel.GetChildren();
            foreach (var gObject in objs)
            {
                if (gObject is not GComponent comp) continue;
                var list = comp.GetChild("list").asList;
                list.ResizeToFit();

                var listSize = list.size;
                listSize.y += 30;
                comp.SetSize(comp.size.x, listSize.y + 60);
                //list.SetPosition(0, 60, 0);
                list.size = listSize;
            }
        }


        private void LoadPetData()
        {
            switch (_sortMethod)
            {
                case MonsterSort.BySpecies:
                    _petByQuality = null;
                    _petBySpecies = _playerService.GetAllPetsBySpecies();
                    Panel.numItems = _petBySpecies.Count;
                    break;
                case MonsterSort.ByQuality:
                    _petBySpecies = null;
                    _petByQuality = _playerService.GetAllPetsByQuality();
                    Panel.numItems = _petByQuality.Count;
                    break;
            }
        }

        private void RenderMonsterList(int index, GObject item)
        {
            var comp = item.asCom;
            var list = comp.GetChild("list").asList;
            var title = comp.GetChild("title").asRichTextField;
            switch (_sortMethod)
            {
                case MonsterSort.BySpecies:
                    var key = _petBySpecies.GetKeyAt(index);
                    title.text = key.Name;
                    list.itemRenderer = (idx, o) => RenderMonsterListItem(_petBySpecies[key][idx], o);
                    list.numItems = _petBySpecies[key].Count;
                    list.onClickItem.Set(context =>
                    {
                        var obj = (GObject)context.data;
                        var idx = list.GetChildIndex(obj);
                        OnClickeMonsterListItem(obj.asCom, _petBySpecies[key][idx]);
                    });
                    break;
                case MonsterSort.ByQuality:
                    var q = _petByQuality.GetKeyAt(index);
                    title.text = QualityToStr(q);
                    list.itemRenderer = (idx, o) => RenderMonsterListItem(_petByQuality[q][idx], o);
                    list.numItems = _petByQuality[q].Count;
                    list.onClickItem.Set(context =>
                    {
                        var obj = (GObject)context.data;
                        var idx = list.GetChildIndex(obj);
                        OnClickeMonsterListItem(obj.asCom, _petByQuality[q][idx]);
                    });
                    break;
            }
        }

        private void OnClickeMonsterListItem(GComponent itemCom, PlayerPetData petData)
        {
            OnItemSelected?.Invoke(itemCom, petData);
            // _dataNodeService.SetData(DataKeys.UIMonsterListSelected, petData);
            // GameFramework.GetService<UIService>().OpenWindow<UIMonsterDetail>();
        }

        private void RenderMonsterListItem(PlayerPetData petData, GObject item)
        {
            var btn = item.asButton;
            
            var comp = btn.GetChild("n7").asCom;
            MonsterHeaderGridButton.RenderHeaderComp(petData, comp);

            var sc = comp.GetController("selected");
            var cc = comp.GetController("checked");
            if (_selectedPets == null || _selectedPets.Length == 0)
            {
                sc.selectedIndex = 0;
            }
            else
            {
                var idx = Array.IndexOf(_selectedPets, petData.Id);
                sc.selectedIndex = idx == -1 ? 0 : 1;
            }


            if (_checkedPets == null || _checkedPets.Length == 0)
            {
                cc.selectedIndex = 0;
            }
            else
            {
                var idx = Array.IndexOf(_checkedPets, petData.Id);
                cc.selectedIndex = idx == -1 ? 0 : 1;
            }
        }
    }
}