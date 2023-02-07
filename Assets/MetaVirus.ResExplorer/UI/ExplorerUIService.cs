using System;
using cfg.common;
using FairyGUI;
using GameEngine;
using GameEngine.Base;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.Procedure;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.Service;
using MetaVirus.ResExplorer.Procedures;
using UnityEngine;

namespace MetaVirus.ResExplorer.UI
{
    public class ExplorerUIService : BaseService
    {
        private GComponent _root;
        private GameDataService _gameDataService;

        public override void PostConstruct()
        {
            Event.On(GameEvents.ResourceEvent.AllResLoaded, OnAllResLoaded);
        }

        public override void PreDestroy()
        {
            Event.Remove(GameEvents.ResourceEvent.AllResLoaded, OnAllResLoaded);
        }

        private void OnAllResLoaded()
        {
            var rootComp = new GComponent();
            var background = new GGraph();
            background.SetSize(rootComp.width, rootComp.height);
            background.AddRelation(rootComp, RelationType.Size);
            background.DrawRect(background.width, background.height, 0, Color.white,
                new Color(1, 1, 1, 1));


            rootComp.AddChild(background);
            rootComp.SetSize(GRoot.inst.width * 0.3f, GRoot.inst.height);
            rootComp.SetPosition(0, 0, 0);

            GRoot.inst.AddChild(rootComp);

            GRoot.inst.onSizeChanged.Add(() =>
            {
                rootComp.SetSize(GRoot.inst.width * 0.3f, GRoot.inst.height);
                rootComp.SetPosition(0, 0, 0);
            });

            _root = rootComp;
            MakeGTree();
        }

        public override void ServiceReady()
        {
            _gameDataService = GameFramework.GetService<GameDataService>();
        }

        void RenderTreeNode(GTreeNode node, GComponent obj)
        {
            var textField = obj.GetChild("n4").asRichTextField;
            if (node.data != null)
            {
                textField.text = node.data switch
                {
                    NpcResourceData resData => $"[{resData.Id}]{resData.Name}",
                    NpcTemplateData tmpData =>
                        $"[{tmpData.Id}]{(string.IsNullOrEmpty(tmpData.Name) ? tmpData.ResDataId_Ref.Name : tmpData.Name)}",
                    _ => textField.text
                };
            }
            //textField.name = node.text;
        }

        void OnExpand(GTreeNode node, bool expand)
        {
            Debug.Log(node.data);
        }

        private void MakeGTree()
        {
            var tree = new GTree
            {
                defaultItem = "ui://hyqj2n8gqbl10",
                clickToExpand = 1,
            };

            tree.selectionMode = ListSelectionMode.Single;

            tree.SetSize(_root.width, _root.height);
            tree.AddRelation(_root, RelationType.Size);
            _root.AddChild(tree);

            tree.treeNodeRender = RenderTreeNode;
            tree.treeNodeWillExpand = OnExpand;

            var rootNode = tree.rootNode;
            var npcResList = new GTreeNode(true);
            var npcTemplateList = new GTreeNode(true);

            foreach (var resData in _gameDataService.gameTable.NpcResourceDatas.DataList)
            {
                var node = new GTreeNode(false)
                {
                    data = resData
                };
                npcResList.AddChild(node);
            }

            foreach (var tempData in _gameDataService.gameTable.NpcTemplateDatas.DataList)
            {
                var node = new GTreeNode(false)
                {
                    data = tempData
                };
                npcTemplateList.AddChild(node);
            }

            rootNode.AddChild(npcResList);
            rootNode.AddChild(npcTemplateList);

            npcResList.cell.GetChild("n4").text = "Npc Model Recources";
            npcResList.data = typeof(NpcResourceData);

            npcTemplateList.cell.GetChild("n4").text = "Npc Templates";
            npcTemplateList.data = typeof(NpcTemplateData);
        }
    }
}