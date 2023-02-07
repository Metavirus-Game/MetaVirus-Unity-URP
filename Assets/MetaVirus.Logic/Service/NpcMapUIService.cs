using System;
using System.Collections.Generic;
using cfg.common;
using FairyGUI;
using GameEngine;
using GameEngine.Base;
using GameEngine.Common;
using GameEngine.DataNode;
using GameEngine.Entity;
using GameEngine.Event;
using GameEngine.FairyGUI;
using GameEngine.Procedure;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events;
using MetaVirus.Logic.Data.Events.Player;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Procedures;
using MetaVirus.Logic.UI.Component.NpcInteractive;
using UnityEngine;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.Service
{
    /**
     * Npc名称显示及交互UI
     */
    public class NpcMapUIService : BaseService
    {
        private EventService _eventService;
        private EntityService _entityService;
        private DataNodeService _dataNodeService;
        private ProcedureService _procedureService;

        private Dictionary<int, NpcEntity> _npcEntities;
        private Dictionary<int, NetPlayerGridItemEntity> _netPlayerEntities;
        private Dictionary<int, GComponent> _npcUIComp;
        private Dictionary<int, GComponent> _netPlayerComp;

        private GComponent _uiContainer;
        private List<Type> _showUiProcedures;
        private ProcedureBase _currProcedure;

        public override void PostConstruct()
        {
            _eventService = GetService<EventService>();
            _entityService = GetService<EntityService>();
            _dataNodeService = GetService<DataNodeService>();
            _procedureService = GetService<ProcedureService>();

            _npcEntities = new Dictionary<int, NpcEntity>();
            _npcUIComp = new Dictionary<int, GComponent>();
            _netPlayerEntities = new Dictionary<int, NetPlayerGridItemEntity>();
            _netPlayerComp = new Dictionary<int, GComponent>();
            _showUiProcedures = new List<Type>();
            _showUiProcedures.AddRange(new List<Type>
            {
                typeof(NormalMapProcedure)
            });
        }

        public override void ServiceReady()
        {
            _eventService.On<NpcEvent>(GameEvents.MapEvent.NpcEvent, OnNpcEvent);
            _eventService.On<GridItemEvent>(GameEvents.MapEvent.GridItemEvent, OnGridItemEvent);
            _eventService.On<PlayerInteractiveNpcListChangedEvent>(GameEvents.PlayerEvent.InteractiveNpcListChanged,
                OnPlayerI13ListChanged);

            _uiContainer = MakeUIContainer();
            _currProcedure = _procedureService.CurrProcedure;
            _eventService.On<ProcedureChangedEvent>(Events.Engine.ProcedureChanged, OnProcedureChanged);
        }

        private void OnPlayerI13ListChanged(PlayerInteractiveNpcListChangedEvent evt)
        {
        }


        private void OnProcedureChanged(ProcedureChangedEvent evt)
        {
            if (evt.CurrProcedure != null)
            {
                _currProcedure = evt.CurrProcedure;
            }

            if (_uiContainer != null)
            {
                _uiContainer.visible = !IsHideNpcUI();
            }
        }

        private GComponent MakeUIContainer()
        {
            var fairyService = GameFramework.GetService<FairyGUIService>();

            var comp = new GComponent
            {
                gameObjectName = "NpcHeadUIContainer",
                name = "NpcHeadUIContainer",
                touchable = false,
                visible = false
            };
            fairyService.AddToGRootFullscreen(comp);
            return comp;
        }

        private GComponent MakeGridItemUI(GridItemEntity entity)
        {
            //var info = _dataNodeService.GetData<PlayerInfo>(DataKeys.PlayerInfo);
            //var player = PlayerEntity.Current;
            // _entityService.GetEntity<PlayerEntity>(EntityGroupName.Player, info.PlayerId);
            var npcComp = UIPackage.CreateObject("Common", "NpcHeaderUI").asCom;
            var txtName = npcComp.GetChild("n0").asTextField;

            var npcLevel = entity.GridItem.Level;
            var npcName = entity.GridItem.Name;

            txtName.text = $"[color=#ffffff]Lv.{npcLevel}[/color] {npcName}";

            //TODO 此处暂时使用Npc作为玩家阵营，增加阵营系统后需修改
            var relation = GetCampRelation(Camp.Npc, entity.Camp);
            var color = NetPlayerRelationToColor(relation);
            txtName.color = color;
            txtName.textFormat.size = 30;
            return npcComp;
        }

        private GComponent MakeNpcUI(NpcEntity entity)
        {
            //var info = _dataNodeService.GetData<PlayerInfo>(DataKeys.PlayerInfo);
            var player = PlayerEntity.Current;
            // _entityService.GetEntity<PlayerEntity>(EntityGroupName.Player, info.PlayerId);
            var npcComp = UIPackage.CreateObject("Common", "NpcHeaderUI").asCom;
            var txtName = npcComp.GetChild("n0").asTextField;

            var npcLevel = entity.MapNpc.Level;
            var npcName = entity.MapNpc.Name;
            if (npcName == "") npcName = entity.Info.NpcTempId_Ref.ResDataId_Ref.Name;

            txtName.text = $"{npcName}";

            var relation = GetNpcRelationWithPlayer(player, entity);
            var color = NpcRelationToColor(relation);
            txtName.color = color;
            txtName.textFormat.size = 30;
            return npcComp;
        }

        public override void PreDestroy()
        {
            _eventService.Remove<NpcEvent>(GameEvents.MapEvent.NpcEvent, OnNpcEvent);
            _eventService.Remove<GridItemEvent>(GameEvents.MapEvent.GridItemEvent, OnGridItemEvent);
            _eventService.Remove<PlayerInteractiveNpcListChangedEvent>(GameEvents.PlayerEvent.InteractiveNpcListChanged,
                OnPlayerI13ListChanged);
        }

        private void OnGridItemEvent(GridItemEvent evt)
        {
            if (evt.EvtType == GridItemEvent.GridItemEventType.Spawn)
            {
                if (evt.ItemType is GridItemType.Player or GridItemType.Bot)
                {
                    var entity =
                        _entityService.GetEntity<NetPlayerGridItemEntity>(EntityGroupName.GridItemNetPlayer,
                            evt.ItemId);
                    if (entity != null)
                    {
                        AddNetPlayerGridItem(entity);
                    }
                }
            }
            else
            {
                if (evt.ItemType is GridItemType.Player or GridItemType.Bot)
                {
                    RemoveNetPlayer(evt.ItemId);
                }
            }
        }

        private void OnNpcEvent(NpcEvent npcEvent)
        {
            if (npcEvent.EvtType == NpcEvent.NpcEventType.Spawn)
            {
                var entity = _entityService.GetEntity<NpcEntity>(EntityGroupName.MapNpc, npcEvent.NpcId);
                if (entity != null)
                {
                    AddNpc(entity);
                }
            }
            else
            {
                RemoveNpc(npcEvent.NpcId);
            }
        }

        private void AddNetPlayerGridItem(NetPlayerGridItemEntity netPlayerEntity)
        {
            var uiComp = MakeGridItemUI(netPlayerEntity);
            _netPlayerEntities[netPlayerEntity.Id] = netPlayerEntity;
            _netPlayerComp[netPlayerEntity.Id] = uiComp;

            _uiContainer.AddChild(uiComp);
        }

        private void AddNpc(NpcEntity npcEntity)
        {
            var uiComp = MakeNpcUI(npcEntity);
            _npcEntities[npcEntity.Id] = npcEntity;
            _npcUIComp[npcEntity.Id] = uiComp;

            _uiContainer.AddChild(uiComp);
        }

        private void RemoveNetPlayer(int netPlayerId)
        {
            _netPlayerEntities.Remove(netPlayerId);
            _netPlayerComp.TryGetValue(netPlayerId, out var comp);
            if (comp != null)
            {
                _uiContainer.RemoveChild(comp, true);
            }

            _netPlayerComp.Remove(netPlayerId);
        }

        private void RemoveNpc(int npcId)
        {
            _npcEntities.Remove(npcId);
            _npcUIComp.TryGetValue(npcId, out var comp);
            if (comp != null)
            {
                _uiContainer.RemoveChild(comp, true);
            }

            _npcUIComp.Remove(npcId);
        }

        private bool IsHideNpcUI()
        {
            return _currProcedure == null || !_showUiProcedures.Contains(_currProcedure.GetType());
        }

        public override void OnUpdate(float elapseTime, float realElapseTime)
        {
            if (IsHideNpcUI())
            {
                return;
            }

            var mc = Camera.main;
            if (mc == null)
            {
                return;
            }

            foreach (var key in _npcEntities.Keys)
            {
                var entity = _npcEntities[key];
                var headUi = _npcUIComp[key];

                var hudPos = entity.NpcHUDPos;

                var screenPos = mc.WorldToScreenPoint(hudPos.transform.position);
                screenPos.y = Screen.height - screenPos.y;
                var uiPos = GRoot.inst.GlobalToLocal(screenPos);

                headUi.SetXY(uiPos.x, uiPos.y);
            }

            foreach (var key in _netPlayerEntities.Keys)
            {
                _netPlayerEntities.TryGetValue(key, out var entity);
                _netPlayerComp.TryGetValue(key, out var headUi);

                if (entity == null || headUi == null) continue;

                var hudPos = entity.NpcHUDPos;

                var screenPos = mc.WorldToScreenPoint(hudPos.transform.position);
                screenPos.y = Screen.height - screenPos.y;
                var uiPos = GRoot.inst.GlobalToLocal(screenPos);

                headUi.SetXY(uiPos.x, uiPos.y);
            }
        }
    }
}