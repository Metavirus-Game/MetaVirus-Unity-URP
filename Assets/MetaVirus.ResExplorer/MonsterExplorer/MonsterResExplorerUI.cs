using System.Collections.Generic;
using cfg.common;
using FairyGUI;
using GameEngine;
using MetaVirus.Logic.Service;
using MetaVirus.ResExplorer.UI;

namespace MetaVirus.ResExplorer.MonsterExplorer
{
    public class MonsterResExplorerUI
    {
        private GComponent _comp;

        private GList _lstData;

        private List<NpcResourceData> _monsterDatas;

        private MonsterEditorComponent _monsterEditorComponent;
        private readonly GameDataService _gameDataService;


        public MonsterResExplorerUI(GComponent monsterExplorerComp)
        {
            _comp = monsterExplorerComp;
            _gameDataService = GameFramework.GetService<GameDataService>();
            LoadData();
            Load();
        }

        private void LoadData()
        {
            _monsterDatas = _gameDataService.gameTable.NpcResourceDatas.DataList;
        }

        private void Load()
        {
            var container = _comp.GetChild("n2").asCom;

            var monsterComp = UIPackage.CreateObject("ZEditorResExplorer", "MonsterEditor").asCom;
            _monsterEditorComponent = new MonsterEditorComponent(monsterComp);

            monsterComp.size = container.size;
            container.AddChild(monsterComp);
            monsterComp.AddRelation(container, RelationType.Size);

            // var tree = comp.GetChild("n0").asTree;
            // FillTree(tree);

            _lstData = _comp.GetChild("listData").asList;
            _lstData.itemRenderer = RenderListData_Monster;
            _lstData.numItems = _monsterDatas.Count;
            _lstData.onClickItem.Set(OnListItemClicked);
        }

        private void OnListItemClicked(EventContext context)
        {
            var obj = (GObject)context.data;
            var idx = _lstData.GetChildIndex(obj);

            var data = _monsterDatas[idx];
            _monsterEditorComponent.EditMonster = data;
        }

        void RenderListData_Monster(int index, GObject obj)
        {
            var btnData = obj.asButton;
            var data = _monsterDatas[index];
            var txtId = btnData.GetChild("txtId");
            txtId.text = data.Id.ToString();
            btnData.title = data.Name;

            var bgCtrl = btnData.GetController("bg");
            bgCtrl.SetSelectedIndex(index % 2);
        }
    }
}