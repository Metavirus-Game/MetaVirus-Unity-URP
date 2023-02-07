using System.Collections.Generic;
using cfg.common;
using GameEngine;
using GameEngine.Base;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.Network;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Data.Npc;
using MetaVirus.Logic.Protocols.Scene;
using MetaVirus.Logic.Utils;
using MetaVirus.Net.Messages.Scene;

namespace MetaVirus.Logic.Service.Npc
{
    /// <summary>
    /// 负责管理服务器下发的Grid中的所有item
    /// 详见服务器的AOILayerManager
    /// </summary>
    public class GridItemRefreshService : BaseService
    {
        private EntityService _entityService;
        private DataNodeService _dataService;
        private GameDataService _gameDataService;
        private EventService _eventService;
        private NetworkService _networkService;

        private int _currentMapId = -1;

        /// <summary>
        /// 当前是否为在地图中的状态
        /// 例如战斗状态时，会收到leaveMap的事件，但同时还需要接收当前地图玩家的位置信息
        /// </summary>
        private bool _inMapState = false;

        //地图未载入时，暂存gridItem的队列
        private readonly List<GridItemEntity> _gridItemsLoadQueue = new();

        /// <summary>
        /// GridItem数据集合
        /// GridItemEntity中会引用此处的数据
        /// 修改_gridItems中的数据，会同时影响到对应的NetPlayerGridItemEntity
        /// </summary>
        private readonly GridItems _gridItems = new();

        public override void PostConstruct()
        {
            _eventService = GameFramework.GetService<EventService>();
            _networkService = GameFramework.GetService<NetworkService>();
            _entityService = GameFramework.GetService<EntityService>();
        }

        public override void ServiceReady()
        {
            _networkService.RegisterPacketListener(Protocols.Protocols.Scene.Main,
                Protocols.Protocols.Scene.ScNotifyGridItemsEnter, OnGridItemEnter);
            _networkService.RegisterPacketListener(Protocols.Protocols.Scene.Main,
                Protocols.Protocols.Scene.ScNotifyGridItemsLeave, OnGridItemLeave);
            _networkService.RegisterPacketListener(Protocols.Protocols.Scene.Main,
                Protocols.Protocols.Scene.ScNotifyGridItemsMove, OnGridItemMove);

            _eventService.On<MapChangedEvent>(GameEvents.MapEvent.MapChanged, OnMapChanged);
        }

        private void AddEntityToGroup(GridItemEntity entity)
        {
            switch (entity.Type)
            {
                case GridItemType.Player:
                case GridItemType.Bot:
                    _entityService.AddEntity(Constants.EntityGroupName.GridItemNetPlayer, entity);
                    break;

                default:
                    break;
            }
        }

        private void OnMapChanged(MapChangedEvent evt)
        {
            if (evt.EvtType == MapChangedEvent.MapChangeEventType.Leave)
            {
                _inMapState = false;
                //离开地图，清除所有的gridItemEntity
                _entityService.RemoveGroup(Constants.EntityGroupName.GridItemNetPlayer);
                // _currentMapId = -1;
            }
            else if (evt.EvtType == MapChangedEvent.MapChangeEventType.Enter)
            {
                _inMapState = true;
                _entityService.GetEntityGroup(Constants.EntityGroupName.GridItemNetPlayer);
                if (_currentMapId == evt.MapId)
                {
                    //并没有切换地图，重新读取entities
                    foreach (var gridItem in _gridItems[_currentMapId].Items)
                    {
                        var entity = GridItemEntity.NewGridItemEntity(gridItem);
                        AddEntityToGroup(entity);
                    }
                }
                else
                {
                    _currentMapId = evt.MapId;
                    //更换地图id，清除所有gridItem数据，保留当前地图下的数据
                    _gridItems.ClearAllItems(_currentMapId);
                }

                LoadAllEntities();
            }
        }

        private void OnGridItemEnter(RespPacket recv)
        {
            if (recv.IsSuccess)
            {
                var enterMsg = ((ScNotifyGridItemsEnter)recv.Packet).ProtoBufMsg;
                AddGridItem(enterMsg);
            }
        }

        private void OnGridItemMove(RespPacket recv)
        {
            if (recv.IsSuccess)
            {
                var moveMsg = ((ScNotifyGridItemsMove)recv.Packet).ProtoBufMsg;
                foreach (var item in moveMsg.GridItems)
                {
                    if (_inMapState)
                    {
                        var entity = _entityService.GetEntity<NetPlayerGridItemEntity>(
                            Constants.EntityGroupName.GridItemNetPlayer, item.Id);

                        entity?.UpdateMove(GridItem.FromPbGridItem(item));
                    }
                    else
                    {
                        //非地图状态，直接记录位置信息到gridItem
                        var gridItem = _gridItems.GetGridItem(_currentMapId, item.Id);
                        gridItem?.UpdatePosition(item.Position.ToVector3());
                        gridItem?.UpdateRotation(item.Rotation.ToVector3());
                    }
                }
            }
        }

        private void OnGridItemLeave(RespPacket recv)
        {
            var leaveMsg = ((ScNotifyGridItemsLeave)recv.Packet).ProtoBufMsg;
            RemoveGridItem(leaveMsg);
        }

        private void RemoveGridItem(SC_NotifyGridItemsLeavePb leaveMsg)
        {
            foreach (var leaveItem in leaveMsg.GridItems)
            {
                if (_currentMapId == -1)
                {
                    var entity = _gridItemsLoadQueue.Find(entity => entity.Id == leaveItem.Id);
                    if (entity != null)
                    {
                        _gridItemsLoadQueue.Remove(entity);
                    }
                }
                else
                {
                    //移除entity
                    var group = _entityService.GetEntityGroup(Constants.EntityGroupName.GridItemNetPlayer);
                    group.RemoveEntity(leaveItem.Id);
                }

                //移除数据
                _gridItems[leaveItem.MapId].RemoveGridItem(leaveItem.Id);
            }
        }


        private void AddGridItem(SC_NotifyGridItemsEnterPb enterMsg)
        {
            foreach (var pbGridItem in enterMsg.GridItems)
            {
                var item = GridItem.FromPbGridItem(pbGridItem);
                var type = (GridItemType)pbGridItem.Type;

                _gridItems.AddGridItem(item);

                var entity = GridItemEntity.NewGridItemEntity(item);

                if (_currentMapId == -1)
                {
                    //地图还未载入完成，暂存
                    if (type is GridItemType.Player or GridItemType.Bot)
                    {
                        _gridItemsLoadQueue.Add(entity);
                    }
                }
                else
                {
                    if (_inMapState)
                    {
                        AddEntityToGroup(entity);
                    }
                }
            }
        }

        /// <summary>
        /// 载入所有暂存的entity
        /// </summary>
        private void LoadAllEntities()
        {
            foreach (var entity in _gridItemsLoadQueue)
            {
                switch (entity.Type)
                {
                    case GridItemType.Player:
                    case GridItemType.Bot:
                        _entityService.AddEntity(Constants.EntityGroupName.GridItemNetPlayer, entity);
                        break;
                    default:
                        break;
                }
            }

            _gridItemsLoadQueue.Clear();
        }

        public override void PreDestroy()
        {
            _entityService.RemoveGroup(Constants.EntityGroupName.GridItemNetPlayer);

            _networkService.UnRegisterPacketListener(Protocols.Protocols.Scene.Main,
                Protocols.Protocols.Scene.ScNotifyGridItemsEnter, OnGridItemEnter);
            _networkService.UnRegisterPacketListener(Protocols.Protocols.Scene.Main,
                Protocols.Protocols.Scene.ScNotifyGridItemsLeave, OnGridItemLeave);
            _networkService.UnRegisterPacketListener(Protocols.Protocols.Scene.Main,
                Protocols.Protocols.Scene.ScNotifyGridItemsMove, OnGridItemMove);
            _eventService.Remove<MapChangedEvent>(GameEvents.MapEvent.MapChanged, OnMapChanged);
        }
    }
}