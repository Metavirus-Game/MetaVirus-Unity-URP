using GameEngine;
using GameEngine.Base.Attributes;
using GameEngine.Common;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.Fsm;
using GameEngine.Procedure;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Data.Player;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Procedures
{
    /**
     * 普通地图逻辑
     */
    [Procedure]
    public class NormalMapProcedure : ProcedureBase
    {
        private DataNodeService _dataService;
        private EntityService _entityService;
        private EventService _eventService;

        public override void OnInit(FsmEntity<ProcedureService> fsm)
        {
            _dataService = GameFramework.GetService<DataNodeService>();
            _entityService = GameFramework.GetService<EntityService>();
            _eventService = GameFramework.GetService<EventService>();
        }

        public override void OnEnter(FsmEntity<ProcedureService> fsm)
        {
            var info = _dataService.GetData<PlayerInfo>(DataKeys.PlayerInfo);
            var pe = _entityService.GetEntity<PlayerEntity>(EntityGroupName.Player, info.PlayerId);
            var player = pe.Player;

            // var mapId = _dataService.GetData<int>(DataKeys.MapCurrentId);
            //
            // if (mapId != -1 && mapId != info.CurrentMapId)
            // {
            //     _eventService.Emit(GameEvents.MapEvent.MapChanged,
            //         new MapChangedEvent(MapChangedEvent.MapChangeEventType.Leave, mapId));
            // }

            _dataService.SetData(DataKeys.MapCurrentId, info.CurrentMapId);
            _dataService.SetData(DataKeys.MapCurrentLayer, info.CurrentLayerId);

            if (player != null)
            {
                player.SetActive(true);
                pe.PlayerController.enabled = true;
                player.transform.position = info.Position;

                //刚进地图，避免战斗
                pe.AvoidBattle = true;
                // player.transform.forward = Vector3.forward;
            }

            _eventService.Emit(GameEvents.MapEvent.MapChanged,
                new MapChangedEvent(MapChangedEvent.MapChangeEventType.Enter, info.CurrentMapId));
        }

        public override void OnLeave(FsmEntity<ProcedureService> fsm, bool isShutdown)
        {
            var info = _dataService.GetData<PlayerInfo>(DataKeys.PlayerInfo);
            var pe = _entityService.GetEntity<PlayerEntity>(EntityGroupName.Player, info.PlayerId);
            pe?.Player.SetActive(false);

            //将mapchanged的事件从OnLeave前移到了OnEnter
            _eventService.Emit(GameEvents.MapEvent.MapChanged,
                new MapChangedEvent(MapChangedEvent.MapChangeEventType.Leave, info.CurrentMapId));
        }
    }
}