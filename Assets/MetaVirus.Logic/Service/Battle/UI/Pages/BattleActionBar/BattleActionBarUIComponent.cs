using System.Collections.Generic;
using cfg.common;
using FairyGUI;
using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;

namespace MetaVirus.Logic.Service.Battle.UI.Pages.BattleActionBar
{
    public class BattleActionBarUIComponent : BattleUIComponent
    {
        private GComponent _actionBar;

        private GComponent _hangerTop;
        private GComponent _hangerBottom;

        private EventService _eventService;

        private readonly Dictionary<int, BattleActionBarHeader> _headers = new();

        public BattleActionBarUIComponent(BattleUIManager manager, BaseBattleInstance battle) : base(manager, battle)
        {
            _eventService = GameFramework.GetService<EventService>();
            _eventService.On<BattleOnUnitAction>(GameEvents.BattleEvent.OnUnitAction, OnUnitAction);
        }

        private void OnUnitAction(BattleOnUnitAction evt)
        {
            var unit = evt.UnitEntity;
            foreach (var uh in _headers.Values)
            {
                if (unit.Id == uh.Id)
                {
                    uh.SetHeaderZOrder(9999);
                }
                else
                {
                    uh.SetHeaderZOrder(uh.Entity.BattleUnit.GetProperty(AttributeId.CalcActionEnergy));
                }
            }

            if (_headers.TryGetValue(unit.Id, out var header))
            {
                if (evt.ActionState == BattleOnUnitAction.Action.Starting)
                {
                    header.ToFocus();
                }
                else if (evt.ActionState == BattleOnUnitAction.Action.Finished)
                {
                    header.ToNormal();
                }
            }
        }

        internal override void Load()
        {
            _actionBar = Manager.UIBattlePage.GetChild("BattleActionBar").asCom;
            _hangerTop = _actionBar.GetChild("Hanger_Top").asCom;
            _hangerBottom = _actionBar.GetChild("Hanger_Bottom").asCom;

            foreach (var entity in Battle.Entities.Values)
            {
                var headerComp = UIPackage.CreateObject("BattlePage", "BattleActionHeader").asCom;
                var side = entity.BattleUnit.Side;
                var parent = side == BattleUnitSide.Source ? _hangerBottom : _hangerTop;
                var header = new BattleActionBarHeader(headerComp, parent, entity,
                    side == BattleUnitSide.Source
                        ? BattleActionBarHeader.Position.Bottom
                        : BattleActionBarHeader.Position.Top);

                _headers[entity.Id] = header;
            }
        }

        internal override void Release()
        {
            foreach (var header in _headers.Values)
            {
                header.Release();
            }
        }

        internal override void OnUpdate(float elapseTime, float realElapseTime)
        {
            foreach (var header in _headers.Values)
            {
                header.OnUpdate(elapseTime, realElapseTime);
            }
        }
    }
}