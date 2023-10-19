using System.Collections.Generic;
using cfg.common;
using FairyGUI;
using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.FairyGUI;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.UI.Pages.BattleHeaderUI
{
    public class BattleHeaderUIComponent : BattleUIComponent
    {
        private EventService _eventService;
        private FairyGUIService _fairyService;


        private BattleHeaderContainer _sourceContainer;
        private BattleHeaderContainer _targetContainer;


        // private readonly Dictionary<BattleUnitEntity, BattleHeader> _headers = new();
        // public readonly Vector2 HeaderSpace = new(21, 112);

        public BattleHeaderUIComponent(BattleUIManager manager, BaseBattleInstance battle) : base(manager, battle)
        {
            _eventService = GameFramework.GetService<EventService>();
            _fairyService = GameFramework.GetService<FairyGUIService>();
            _eventService.On<BattleOnUnitAction>(GameEvents.BattleEvent.OnUnitAction, OnUnitAction);
            _eventService.On<BattleUnitPropertiesChangedEvent>(GameEvents.BattleEvent.OnUnitPropertiesChanged,
                OnUnitPropertiesChanged);
        }

        private void OnUnitPropertiesChanged(BattleUnitPropertiesChangedEvent evt)
        {
            var unit = evt.UnitEntity;
            var header = _sourceContainer[unit] ?? _targetContainer[unit];

            if (header == null)
            {
                return;
            }

            if (evt.ChangedProperty == AttributeId.CalcHp)
            {
                header.RefreshHp();
            }
            else if (evt.ChangedProperty == AttributeId.CalcMp)
            {
                header.RefreshMp();
            }
        }

        private void OnUnitAction(BattleOnUnitAction evt)
        {
            var unit = evt.UnitEntity;
            var header = _sourceContainer[unit] ?? _targetContainer[unit];

            if (header == null)
            {
                return;
            }

            if (evt.ActionState == BattleOnUnitAction.Action.Starting)
            {
                header.ToFocus();
            }
            else if (evt.ActionState == BattleOnUnitAction.Action.Finished)
            {
                header.ToNormal();
            }
            else if (evt.ActionState == BattleOnUnitAction.Action.Dead)
            {
                header.RefreshDead();
            }
        }

        internal override void Load()
        {
            var sources = Battle.GetUnitEntities(BattleUnitSide.Source);
            var targets = Battle.GetUnitEntities(BattleUnitSide.Target);
            var containerSource = Manager.UIBattlePage.GetChild("BattleHeaderContainer_Source").asCom;
            var containerTarget = Manager.UIBattlePage.GetChild("BattleHeaderContainer_Target").asCom;

            _sourceContainer = new BattleHeaderContainer(containerSource);
            _targetContainer = new BattleHeaderContainer(containerTarget);
            
            foreach (var unit in sources)
            {
                var header = UIPackage.CreateObject("BattlePage", "BattleHeaderUI").asCom;
                var battleHeader = new BattleHeaderV2(header, unit);
                _sourceContainer.AddHeader(battleHeader);
            }

            foreach (var unit in targets)
            {
                var header = UIPackage.CreateObject("BattlePage", "BattleHeaderUI_Top").asCom;
                var battleHeader = new BattleHeaderV2(header, unit, true);
                _targetContainer.AddHeader(battleHeader);
            }

            // parentUp.SetXY(1060, parentUp.y);
            // for (var i = 0; i < sources.Count; i++)
            // {
            //     var unit = sources[i];
            //     var header = MakeUnitHeader(unit, i, parent);
            //     _headers[unit] = header;
            // }
            //
            // for (var i = 0; i < targets.Count; i++)
            // {
            //     var unit = targets[i];
            //     var header = MakeUnitHeader(unit, i, parentUp, true);
            //     _headers[unit] = header;
            // }
        }

        // private BattleHeader MakeUnitHeader(BattleUnitEntity unit, int index, GComponent parent, bool up = false)
        // {
        //     var res = up ? "BattleHeadBar_Right" : "BattleHeadBar";
        //     var header = UIPackage.CreateObject("BattlePage", res).asCom;
        //     var battleHeader = new BattleHeader(header, unit);
        //
        //     parent.AddChild(header);
        //     header.scale = new Vector2(1.4f, 1.4f);
        //     header.pivot = up ? new Vector2(1, 0) : new Vector2(0, 0);
        //     header.pivotAsAnchor = true;
        //
        //     var headerX = index % 2 == 0 ? 0 : HeaderSpace.x;
        //     var headerY = HeaderSpace.y * index;
        //
        //     if (up)
        //     {
        //         headerX *= -1;
        //     }
        //
        //     header.SetXY(headerX, headerY);
        //
        //     return battleHeader;
        // }

        internal override void Release()
        {
            // foreach (var header in _headers.Values)
            // {
            //     header.HeaderComp.RemoveFromParent();
            //     header.HeaderComp.Dispose();
            // }

            _sourceContainer.Release();
            _targetContainer.Release();

            _eventService.Remove<BattleOnUnitAction>(GameEvents.BattleEvent.OnUnitAction, OnUnitAction);
            _eventService.Remove<BattleUnitPropertiesChangedEvent>(GameEvents.BattleEvent.OnUnitPropertiesChanged,
                OnUnitPropertiesChanged);
        }

        internal override void OnUpdate(float elapseTime, float realElapseTime)
        {
        }
    }
}