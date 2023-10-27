using System.Collections.Generic;
using cfg.common;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.MonsterPanel;
using UnityEngine;

namespace MetaVirus.Logic.UI.Windows
{
    [UIWindow("ui_monsters_list")]
    public class UIMonstersList : BaseUIWindow
    {
        private GList _monstersList;
        private PlayerService _playerService;
        private DataNodeService _dataNodeService;

        private MonsterListPanel _monsterListPanel;

        protected override GComponent MakeContent()
        {
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _playerService = GameFramework.GetService<PlayerService>();
            var comp = UIPackage.CreateObject("Common", "MonsterListUI").asCom;
            _monsterListPanel = new MonsterListPanel(comp.GetChild("listMonsters").asList)
            {
                OnItemSelected = (comp, petData) =>
                {
                    _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailData, petData);
                    _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailDataList,
                        _playerService.GetPetListProvider());
                    GameFramework.GetService<UIService>().OpenWindow<UIMonsterDetail>();
                }
            };

            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _monsterListPanel.LoadData();

            var btnSortBy = content.GetChild("btnSortBy").asButton;
            btnSortBy.onClick.Set(c =>
            {
                _monsterListPanel.SortMethod =
                    btnSortBy.selected ? Constants.MonsterSort.ByQuality : Constants.MonsterSort.BySpecies;
            });
        }
    }
}