using GameEngine;
using GameEngine.Base;
using GameEngine.Common;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.Network;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Protocols.Scene;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Utils;
using MetaVirus.Net.Messages.Common;
using MetaVirus.Net.Messages.Scene;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service
{
    public class PositionService : BaseService
    {
        private EntityService _entityService;
        private EventService _eventService;
        private DataNodeService _dataService;
        private NetworkService _networkService;
        private PlayerService _playerService;

        private int currentMapId = -1;

        // private PlayerEntity _playerEntity;
        // private PlayerInfo _playerInfo;

        private Vector3? lastReportPos;
        private float lastReportTime;

        //坐标上报间隔
        public const float ReportInterval = 0.25f;

        //没有移动时的上报间隔
        private const float IdleInterval = 10;

        public override void PostConstruct()
        {
            _eventService = GameFramework.GetService<EventService>();
            _entityService = GameFramework.GetService<EntityService>();
            _dataService = GameFramework.GetService<DataNodeService>();
            _networkService = GameFramework.GetService<NetworkService>();
            _playerService = GameFramework.GetService<PlayerService>();
        }

        public override void ServiceReady()
        {
            _eventService.On<MapChangedEvent>(GameEvents.MapEvent.MapChanged, OnMapChanged);
        }

        private void OnMapChanged(MapChangedEvent evt)
        {
            if (evt.EvtType == MapChangedEvent.MapChangeEventType.Enter)
            {
                currentMapId = evt.MapId;
                
                var playerInfo = _playerService.CurrentPlayerInfo;
                if (playerInfo != null)
                {
                    var pbPid = new PBPlayerId()
                    {
                        Id = playerInfo.PlayerId,
                    };
                    var report = new CsPlayerReportEnterMap(pbPid);
                    _networkService.SendPacketTo(report, playerInfo.sceneServerId);
                }

                lastReportPos = _playerService.CurrentPlayer.Position;
                lastReportTime = Time.realtimeSinceStartup;
                
            }
            // else if (evt.EvtType == MapChangedEvent.MapChangeEventType.Leave)
            // {
            //     currentMapId = -1;
            //     lastReportPos = null;
            // }

            
        }

        public override void PreDestroy()
        {
            _eventService.Remove<MapChangedEvent>(GameEvents.MapEvent.MapChanged, OnMapChanged);
        }

        public override void OnUpdate(float elapseTime, float realElapseTime)
        {
            var playerEntity = _playerService.CurrentPlayer;
            if (currentMapId > 0 && playerEntity != null)
            {
                if (Time.realtimeSinceStartup - lastReportTime >= ReportInterval)
                {
                    var pos = playerEntity.Position;
                    if (!lastReportPos.HasValue || pos != lastReportPos.Value)
                    {
                        lastReportTime = Time.realtimeSinceStartup;
                        lastReportPos = playerEntity.Position;

                        ReportPosition(playerEntity.PlayerInfo.sceneServerId);
                    }
                }

                if (Time.realtimeSinceStartup - lastReportTime > IdleInterval)
                {
                    lastReportTime = Time.realtimeSinceStartup;
                    lastReportPos = playerEntity.Position;

                    ReportPosition(playerEntity.PlayerInfo.sceneServerId);
                }
            }
        }

        private PlayerReportPositionPb _reportPb;

        private void ReportPosition(int sceneServerId)
        {
            if (_reportPb == null)
            {
                _reportPb = new PlayerReportPositionPb();
            }

            _reportPb.PlayerId = _playerService.CurrentPlayer.Id;
            _reportPb.Position = _playerService.CurrentPlayer.Position.ToPbVector3();
            _reportPb.Rotation = _playerService.CurrentPlayer.Rotation.ToPbVector3();

            //Debug.Log($"Report Position: {_playerEntity.Position}");

            var req = new CsPlayerReportPosition(_reportPb);
            _networkService.SendPacketTo(req, sceneServerId);
        }
    }
}