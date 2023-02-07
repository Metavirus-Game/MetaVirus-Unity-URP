using System.Collections.Generic;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Event;
using GameEngine.Network;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events.Player;
using MetaVirus.Logic.Protocols.Scene;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Net.Messages.Common;
using MetaVirus.Net.Messages.Scene;
using UnityEngine;

namespace MetaVirus.Logic.UI.Windows
{
    [UIWindow("ui_npc_interactive")]
    public class NpcInteractiveWindow : BaseUIWindow
    {
        private NpcEntity _npcEntity;
        private DataNodeService _dataNodeService;
        private NetworkService _networkService;
        private EventService _eventService;
        private GameDataService _gameDataService;
        private NpcFunctionService _npcFunctionService;

        private GLoader3D _modelLoader;
        private GRichTextField _txtGreeting;
        private GRichTextField _txtNpcName;
        private GList _funcList;

        private IList<PBNpcFunctionItem> _items;

        private PBNpcFunctionItem _itemSelected;

        protected override float ContentInitAlpha => 1;

        public NpcInteractiveWindow()
        {
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _networkService = GameFramework.GetService<NetworkService>();
            _eventService = GameFramework.GetService<EventService>();
            _npcFunctionService = GameFramework.GetService<NpcFunctionService>();
            SetBgFadeInSetting(true);
        }

        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "NpcInteractiveUI").asCom;
            _modelLoader = comp.GetChild("ModelLoader").asLoader3D;
            _txtGreeting = comp.GetChild("txtGreeting").asRichTextField;
            _txtNpcName = comp.GetChild("txtNpcName").asRichTextField;
            _funcList = comp.GetChild("funcList").asList;
            _funcList.itemRenderer = RenderItem;
            _funcList.onClickItem.Set(OnClickItem);
            return comp;
        }

        protected override void AddComponentToParent(GComponent parentComp, GComponent content)
        {
            content.SetSize(parentComp.size.x, content.size.y);
            content.AddRelation(parentComp, RelationType.Bottom_Bottom);
            content.AddRelation(parentComp, RelationType.Width);
            content.SetPivot(0, 1);
            content.pivotAsAnchor = true;
            content.SetPosition(0, parentComp.size.y + content.size.y, 0);
            parentComp.AddChild(content);
        }


        private void OnClickItem(EventContext context)
        {
            var obj = (GObject)context.data;
            var idx = _funcList.GetChildIndex(obj);
            Debug.Log("按了 " + idx);
            Hide();

            if (idx == _items.Count)
            {
                //点的是关闭按钮
                _itemSelected = null;
            }
            else
            {
                _itemSelected = _items[idx];
            }
        }

        private void RenderItem(int index, GObject obj)
        {
            var comp = obj.asCom;
            var txtItem = comp.GetChild("n1").asRichTextField;
            if (index == _items.Count)
            {
                txtItem.text = "Bye";
            }
            else
            {
                txtItem.text = _items[index].MenuItem;
            }
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _npcEntity = _dataNodeService.GetData<NpcEntity>(Constants.DataKeys.UIDataInteractiveNpc);

            _eventService.Emit(GameEvents.PlayerEvent.InteractingWithNpc,
                new PlayerInteractingWithNpcEvent(PlayerInteractingWithNpcEvent.EventType.Start, _npcEntity));

            _txtNpcName.text = _npcEntity.MapNpc.Name;
            var touchPb = new TouchNpcRequestCSPb
            {
                NpcId = _npcEntity.Id
            };

            var dstId = PlayerEntity.Current.PlayerInfo.sceneServerId;
            _networkService.SendPacketTo(new TouchNpcRequestCs(touchPb), dstId, resp =>
            {
                var touchResp = resp.GetPacket<TouchNpcResponseSc>();
                _txtGreeting.text = touchResp.ProtoBufMsg.Greeting;
                _items = touchResp.ProtoBufMsg.FuncItems;
                _funcList.numItems = _items.Count + 1;
            });
        }

        protected override float DoShowAni()
        {
            var py = ContentComp.position.y;
            var toPy = py - ContentComp.size.y;
            GTween.To(py, toPy, 0.4f).SetEase(EaseType.BackOut).OnUpdate(
                t => ContentComp.SetPosition(0, t.value.x, 0)
            );
            return 0.4f;
        }

        protected override float DoHideAni()
        {
            var py = ContentComp.position.y;
            var toPy = GRoot.inst.size.y + ContentComp.size.y;
            GTween.To(py, toPy, 0.4f).OnUpdate(
                t =>
                {
                    ContentComp.SetPosition(0, t.value.x, 0);
                    if (t.normalizedTime >= 0.7f)
                    {
                        //关闭窗口的动画播放到70%的时候，执行选择的功能
                        if (_itemSelected == null) return;
                        _npcFunctionService.ProcessNpcFunction(_itemSelected.FuncId, _itemSelected.TypeParam);
                        _itemSelected = null;
                    }
                });
            _eventService.Emit(GameEvents.PlayerEvent.InteractingWithNpc,
                new PlayerInteractingWithNpcEvent(PlayerInteractingWithNpcEvent.EventType.End, _npcEntity));
            return 0.4f;
        }

        public override void Release()
        {
            base.Release();
        }
    }
}