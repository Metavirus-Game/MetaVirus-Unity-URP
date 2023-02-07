using System.Collections.Generic;
using cfg.common;
using FairyGUI;
using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.FairyGUI;
using GameEngine.ObjectPool;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;
using UnityEngine;

namespace MetaVirus.Logic.Service.Battle.UI.Pages.FloatingText
{
    internal struct DamageFloatingPanel
    {
        public BattleUnitEntity UnitEntity;
        public GComponent FloatingPanel;

        public void AdjustPosition(Camera battleCamera)
        {
            var hitPos = UnitEntity.UnitAni.GetVfxBindPos(VfxBindPos.HitPos);
            var screenPos = battleCamera.WorldToScreenPoint(hitPos.position);
            screenPos.y = Screen.height - screenPos.y;
            var uiPos = GRoot.inst.GlobalToLocal(screenPos);
            FloatingPanel.SetXY(uiPos.x, uiPos.y);
        }
    }

    public class DamageFloatingUIComponent : BattleUIComponent
    {
        private string FloatingTextPoolName => $"FloatingTextPool-{Battle.BattleId}";

        private readonly FairyGUIService _guiService;
        private readonly ObjectPoolService _poolService;
        private readonly EventService _eventService;


        private readonly ObjectPool<FloatingText> _floatingTextPool;
        private readonly List<FloatingText> _runningFloatingTexts = new();


        private readonly Dictionary<int, DamageFloatingPanel> _panels = new();

        public DamageFloatingUIComponent(BattleUIManager manager, BaseBattleInstance battle) : base(manager, battle)
        {
            _poolService = GameFramework.GetService<ObjectPoolService>();
            _guiService = GameFramework.GetService<FairyGUIService>();
            _eventService = GameFramework.GetService<EventService>();

            _floatingTextPool = _poolService.NewObjectPool(FloatingTextPoolName,
                newObjFunc: () => new FloatingText());
            _eventService.On<BattleOnDamageEvent>(GameEvents.BattleEvent.OnSkillDamage, OnDamage);
            _eventService.On<BattleOnBuffEffectDamageEvent>(GameEvents.BattleEvent.OnBuffEffectDamage,
                OnBuffEffectDamage);
        }

        private void OnBuffEffectDamage(BattleOnBuffEffectDamageEvent evt)
        {
            MakeFloatingText(evt);
        }

        private void OnDamage(BattleOnDamageEvent evt)
        {
            MakeFloatingText(evt);
        }

        private void MakeFloatingText(object evt)
        {
            FloatingTextData data;
            BattleUnitEntity entity = null;

            if (evt is BattleOnDamageEvent d)
            {
                data = FloatingTextData.FromSkillCastInfo(d.CastData);
                entity = d.UnitEntity;
            }
            else if (evt is BattleOnBuffEffectDamageEvent b)
            {
                data = FloatingTextData.FromBuffEffect(b.Buff, b.EffectAttr, b.EffectValue);
                entity = b.UnitEntity;
            }
            else
            {
                data = default;
            }

            if (entity == null) return;

            if (_panels.TryGetValue(entity.Id, out var ui))
            {
                var delay = 0f;
                var last = FindLatestFloatingText(entity.Id);
                if (last is { AliveTime: < 0.4f })
                {
                    delay = 0.4f - last.AliveTime;
                }

                var ft = _floatingTextPool.Get<FloatingText>();
                _runningFloatingTexts.Add(ft);
                ft.SetInfo(ui.FloatingPanel, data, entity, delay);
                ft.OnComplete = () =>
                {
                    _runningFloatingTexts.Remove(ft);
                    _floatingTextPool.Release(ft);
                };
            }
        }

        private FloatingText FindLatestFloatingText(int entityId)
        {
            var last = _runningFloatingTexts.FindLast((ft) => ft.BindEntityId == entityId);
            return last;
        }


        internal override void Load()
        {
            foreach (var entity in Battle.Entities.Values)
            {
                var panel = CreateFloatingPanel(entity);
                _panels[entity.Id] = panel;
            }
        }

        internal override void Release()
        {
            _eventService.Remove<BattleOnDamageEvent>(GameEvents.BattleEvent.OnSkillDamage, OnDamage);
            _poolService.ClearObjectPool<FloatingText>(FloatingTextPoolName);

            foreach (var p in _panels.Values)
            {
                GRoot.inst.RemoveChild(p.FloatingPanel, true);
            }
        }

        private DamageFloatingPanel CreateFloatingPanel(BattleUnitEntity entity)
        {
            var name = $"DamageFloating-{entity.LogName}";
            var p = new GComponent
            {
                name = name,
                gameObjectName = name,
                pivot = new Vector2(0.5f, 0.5f),
                pivotAsAnchor = true,
                size = Vector2.zero,
            };

            p.displayObject.layer = LayerMask.NameToLayer("FloatingTextUI");

            GRoot.inst.AddChild(p);

            return new DamageFloatingPanel
            {
                UnitEntity = entity,
                FloatingPanel = p,
            };
        }

        internal override void OnUpdate(float elapseTime, float realElapseTime)
        {
            foreach (var panel in _panels.Values)
            {
                panel.AdjustPosition(Battle.BattleField.battleCamera.Camera);
            }

            foreach (var ft in _runningFloatingTexts)
            {
                ft.OnUpdate(elapseTime, realElapseTime);
            }
        }
    }
}