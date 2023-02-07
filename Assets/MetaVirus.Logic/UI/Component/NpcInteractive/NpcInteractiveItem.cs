using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.FairyGUI;
using GameEngine.Network;
using GameEngine.ObjectPool;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Protocols.Scene;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Windows;
using MetaVirus.Net.Messages.Scene;
using UnityEngine;

namespace MetaVirus.Logic.UI.Component.NpcInteractive
{
    public class NpcInteractiveItem : IRecyclable
    {
        private int _npcId;
        private EntityService _entityService;
        private DataNodeService _dataNodeService;
        private NetworkService _networkService;
        private FairyGUIService _guiService;

        //交互间隔
        private const float InteractiveDuration = 1;
        private float _btnClickTime = 0;

        public NpcEntity NpcEntity { get; private set; }

        public GComponent UiComp { get; private set; }

        private GButton _btnHeader;

        public NpcInteractiveItem()
        {
            _networkService = GameFramework.GetService<NetworkService>();
            _entityService = GameFramework.GetService<EntityService>();
            _guiService = GameFramework.GetService<FairyGUIService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();
        }

        public void SetNpcId(int npcId)
        {
            _npcId = npcId;
            NpcEntity = _entityService.GetEntity<NpcEntity>(Constants.EntityGroupName.MapNpc, npcId);
            if (UiComp == null)
            {
                MakeComponent();
            }

            RefreshContent();
        }

        private void MakeComponent()
        {
            _btnHeader = UIPackage.CreateObject("Common", "BtnCircleHead_Red").asButton;

            UiComp = new GComponent
            {
                size = _btnHeader.size
            };

            UiComp.AddChild(_btnHeader);

            _btnHeader.SetPosition(0, 0, 0);
            _btnHeader.onClick.Set(OnBtnClicked);
        }

        private void OnBtnClicked()
        {
            if (Time.realtimeSinceStartup - _btnClickTime < InteractiveDuration) return;
            _btnClickTime = Time.realtimeSinceStartup;

            _dataNodeService.SetData(Constants.DataKeys.UIDataInteractiveNpc, NpcEntity);
            GameFramework.GetService<UIService>().OpenWindow<NpcInteractiveWindow>();

            // var touchPb = new TouchNpcRequestCSPb
            // {
            //     NpcId = _npcId
            // };

            //int dstId = PlayerEntity.Current.PlayerInfo.sceneServerId;
            //_networkService.SendPacketTo(new TouchNpcRequestCs(touchPb), dstId, resp => { Debug.Log(resp); });
        }

        private void RefreshContent()
        {
            UiComp.visible = true;
        }

        public void OnSpawn()
        {
            if (UiComp != null)
            {
                UiComp.visible = false;
            }
        }

        public void OnRecycle()
        {
            if (UiComp != null)
            {
                UiComp.RemoveFromParent();
            }
        }

        public void OnDestroy()
        {
            if (UiComp != null)
            {
                UiComp.RemoveFromParent();
                UiComp.Dispose();
            }
        }
    }
}