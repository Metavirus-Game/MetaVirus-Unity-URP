using System.Collections.Generic;
using System.Linq;
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

        private GProgressBar _hpBarSrc;
        private GProgressBar _hpBarTar;

        private EventService _eventService;

        private readonly Dictionary<int, BattleActionBarHeader> _headers = new();

        private int _totalHpTar = 0;
        private int _totalHpSrc = 0;

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

            var srcs = Battle.GetUnitEntities(BattleUnitSide.Source);
            var tars = Battle.GetUnitEntities(BattleUnitSide.Target);

            _hpBarSrc = _actionBar.GetChild("n39").asProgress;
            _hpBarTar = _actionBar.GetChild("n40").asProgress;

            _totalHpSrc = srcs.Sum(entity => entity.BattleUnit.GetProperty(AttributeId.CalcHpMax));
            _totalHpTar = tars.Sum(entity => entity.BattleUnit.GetProperty(AttributeId.CalcHpMax));
            _hpBarSrc.value = _hpBarSrc.max = _totalHpSrc;
            _hpBarTar.value = _hpBarTar.max = _totalHpTar;

            _eventService.On<BattleUnitPropertiesChangedEvent>(GameEvents.BattleEvent.OnUnitPropertiesChanged,
                OnUnitPropertiesChanged);

            //setup src and target name
            var txtSrcName = _actionBar.GetChild("txtAttack").asTextField;
            var txtTarName = _actionBar.GetChild("txtDefence").asTextField;

            txtSrcName.text = Battle.SrcName;
            txtTarName.text = Battle.TarName;

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

        private void OnUnitPropertiesChanged(BattleUnitPropertiesChangedEvent evt)
        {
            if (evt.ChangedProperty == AttributeId.CalcHp)
            {
                var bu = evt.UnitEntity.BattleUnit;
                if (bu.Side == BattleUnitSide.Source)
                {
                    _totalHpSrc -= evt.ValueFrom - evt.ValueTo;
                    _hpBarSrc.TweenValue(_totalHpSrc, 0.2f);
                }
                else
                {
                    _totalHpTar -= evt.ValueFrom - evt.ValueTo;
                    _hpBarTar.TweenValue(_totalHpTar, 0.2f);
                }
            }
        }

        internal override void Release()
        {
            _eventService.Remove<BattleUnitPropertiesChangedEvent>(GameEvents.BattleEvent.OnUnitPropertiesChanged,
                OnUnitPropertiesChanged);
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