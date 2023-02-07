using System.Collections.Generic;
using cfg.common;
using cfg.map;
using GameEngine.Base;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.Network;
using GameEngine.ObjectPool;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Data.Npc;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Protocols.Scene;
using MetaVirus.Logic.Utils;
using MetaVirus.Net.Messages.Common;
using Unity.VisualScripting;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service.Npc
{
    /**
     * Npc刷新服务，管理当前地图上的所有刷新点，以及怪物刷新的逻辑
     */
    public class NpcRefreshService : BaseService
    {
        private EntityService _entityService;
        private DataNodeService _dataService;
        private GameDataService _gameDataService;
        private EventService _eventService;
        private ObjectPoolService _objectPoolService;
        private NetworkService _networkService;

        private ObjectPool<NpcRefreshPoint> _refreshPointPool;
        private Transform _refreshPoints;

        private int _currentMapId;

        private readonly Dictionary<int, List<MapNpc>> _mapNpcDic = new();

        public override void PostConstruct()
        {
            _entityService = GetService<EntityService>();
            _dataService = GetService<DataNodeService>();
            _gameDataService = GetService<GameDataService>();
            _eventService = GetService<EventService>();
            _objectPoolService = GetService<ObjectPoolService>();
            _networkService = GetService<NetworkService>();
        }

        public override void PreDestroy()
        {
            _eventService.Remove<MapChangedEvent>(GameEvents.MapEvent.MapChanged, OnMapChanged);
            _networkService.UnRegisterPacketListener(Protocols.Protocols.Scene.Main,
                Protocols.Protocols.Scene.ScNotifyRefreshNpc, OnRecvNotifyPacket);
            Destroy(_refreshPoints.GameObject());
        }

        public override void ServiceReady()
        {
            _refreshPoints = new GameObject("RefreshPoints").transform;
            DontDestroyOnLoad(_refreshPoints.gameObject);

            _eventService.On<MapChangedEvent>(GameEvents.MapEvent.MapChanged, OnMapChanged);
            _refreshPointPool = _objectPoolService.NewObjectPool("RefreshPointPool",
                newObjFunc: CreateNpcRefreshPoint);

            _networkService.RegisterPacketListener(Protocols.Protocols.Scene.Main,
                Protocols.Protocols.Scene.ScNotifyRefreshNpc, OnRecvNotifyPacket);
        }

        private void OnRecvNotifyPacket(RespPacket packet)
        {
            if (packet.IsSuccess)
            {
                if (_currentMapId == -1) return;
                
                var p = (ScNotifyRefreshNpc)packet.Packet;
                var npcs = p.ProtoBufMsg.RefreshNpcs;

                var mapData = _gameDataService.GetMapData(_currentMapId);
                //var list = GetMapNpcList(_currentMapId);

                foreach (var npc in npcs)
                {
                    var id = npc.NpcId;
                    var state = (MapNpcState)npc.State;

                    var infoId = MapNpc.GetNpcRefreshInfoId(id);
                    var type = (MapNpc.Type)MapNpc.GetNpcType(id);
                    var info = FindRefreshInfo(
                        type == MapNpc.Type.Npc ? mapData.Npcs : mapData.Monsters,
                        infoId);

                    var mapNpc = new MapNpc(id, npc.DisplayName, npc.Level, npc.Position.ToVector3(),
                        npc.Rotation.ToVector3(), state, info);
                    //list.Add(mapNpc);

                    var point = _refreshPointPool.Get<NpcRefreshPoint>();
                    point.Info = mapNpc;
                    point.MapId = _currentMapId;
                    point.Refresh();
                }
            }
        }

        private NpcRefreshInfo FindRefreshInfo(List<NpcRefreshInfo> infoList, int infoId)
        {
            return infoList.Find(info => info.NpcInfoId == infoId);
        }

        private List<MapNpc> GetMapNpcList(int mapId)
        {
            _mapNpcDic.TryGetValue(mapId, out var list);
            if (list == null)
            {
                list = new List<MapNpc>();
                _mapNpcDic[mapId] = list;
            }

            return list;
        }


        private NpcRefreshPoint CreateNpcRefreshPoint()
        {
            var go = new GameObject("NpcRefreshPoint");
            go.SetActive(false);
            //DontDestroyOnLoad(go);
            go.transform.SetParent(_refreshPoints, false);
            var point = go.AddComponent<NpcRefreshPoint>();
            return point;
        }

        private void LeaveMap(int mapId)
        {
            if (_currentMapId == mapId)
            {
                _currentMapId = -1;
                //先清除当前的刷新点及关联Entity
                _refreshPointPool.ReleaseAll();
                //_entityService.RemoveGroup(EntityGroupName.MapNpc);
            }
        }

        private void EnterMap(int mapId)
        {
            _currentMapId = mapId;

            // var mapData = _gameDataService.GetMapData(mapId);
            //
            // //创建刷新点
            // foreach (var monster in mapData.Monsters)
            // {
            //     var point = _refreshPointPool.Get<NpcRefreshPoint>();
            //     point.Info = monster;
            //     point.MapId = mapId;
            //     point.Refresh();
            // }
            //
            // foreach (var npc in mapData.Npcs)
            // {
            //     var point = _refreshPointPool.Get<NpcRefreshPoint>();
            //     point.Info = npc;
            //     point.MapId = mapId;
            //     point.Refresh();
            // }
        }

        private void OnMapChanged(MapChangedEvent evt)
        {
            if (evt.EvtType == MapChangedEvent.MapChangeEventType.Enter)
            {
                EnterMap(evt.MapId);
            }
            else
            {
                LeaveMap(evt.MapId);
            }
        }
    }
}