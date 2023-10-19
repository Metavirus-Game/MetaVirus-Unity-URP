using System.Collections.Generic;
using System.Linq;
using cfg.battle;
using cfg.common;
using FairyGUI;
using GameEngine;
using GameEngine.Event;
using MetaVirus.Logic.Service;
using MetaVirus.ResExplorer.UI;

namespace MetaVirus.ResExplorer.MonsterExplorer
{
    public class VfxResExplorerUI
    {
        private GComponent _comp;

        private GList _lstData;

        private List<VFXData> _vfxDatas;

        private readonly GameDataService _gameDataService;
        private VfxEditorComponent _vfxEditorComponent;

        private int _currVfxType = 0;
        private readonly EventService _eventService;

        public VfxResExplorerUI(GComponent monsterExplorerComp)
        {
            _comp = monsterExplorerComp;
            _gameDataService = GameFramework.GetService<GameDataService>();
            _eventService = GameFramework.GetService<EventService>();
            LoadData();
            Load();
        }

        private void LoadData()
        {
            _vfxDatas = _gameDataService.gameTable.VFXDatas.DataList;
        }

        private void OnVfxTypeChanged()
        {
            _vfxDatas = _gameDataService.gameTable.VFXDatas.DataList
                .Where(v => v.Id > 0 && (_currVfxType == 0 || (int)v.Type == _currVfxType)).ToList();
            _lstData.numItems = _vfxDatas.Count;
        }

        private void Load()
        {
            _lstData = _comp.GetChild("listData").asList;
            var editorComp = _comp.GetChild("vfxEditor").asCom;
            _vfxEditorComponent = new VfxEditorComponent(editorComp);

            var cmbVfxType = _comp.GetChild("cmbVfxType").asComboBox;
            cmbVfxType.items = new[] { "全部特效", "命中特效", "伴随特效", "技能群体特效(暂时没用)", "投射物特效", "投射物发射特效" };
            cmbVfxType.selectedIndex = _currVfxType;
            OnVfxTypeChanged();

            cmbVfxType.onChanged.Add(() =>
                {
                    _currVfxType = cmbVfxType.selectedIndex;
                    OnVfxTypeChanged();
                }
            );

            //var container = _comp.GetChild("n2").asCom;

            // var monsterComp = UIPackage.CreateObject("ZEditorResExplorer", "MonsterEditor").asCom;
            // _monsterEditorComponent = new MonsterEditorComponent(monsterComp);
            //
            // monsterComp.size = container.size;
            // container.AddChild(monsterComp);
            // monsterComp.AddRelation(container, RelationType.Size);

            // var tree = comp.GetChild("n0").asTree;
            // FillTree(tree);

            _lstData.itemRenderer = RenderListData_Monster;
            _lstData.numItems = _vfxDatas.Count;
            _lstData.onClickItem.Set(OnListItemClicked);
        }

        private void OnListItemClicked(EventContext context)
        {
            var obj = (GObject)context.data;
            var idx = _lstData.GetChildIndex(obj);

            var data = _vfxDatas[idx];
            //_monsterEditorComponent.EditMonster = data;
            _vfxEditorComponent.CurrVfxData = data;
        }

        void RenderListData_Monster(int index, GObject obj)
        {
            var btnData = obj.asButton;
            var data = _vfxDatas[index];
            var txtId = btnData.GetChild("txtId");
            txtId.text = data.Id.ToString();
            btnData.title = data.Name;

            var bgCtrl = btnData.GetController("bg");
            bgCtrl.SetSelectedIndex(index % 2);
        }
    }
}