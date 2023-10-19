using System;
using System.Collections.Generic;
using System.Text;
using FairyGUI;
using GameEngine;
using GameEngine.Common;
using GameEngine.Event;
using GameEngine.FairyGUI;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Battle;

namespace MetaVirus.Logic.Service.Battle.UI.Pages.BattleRecordTextUI
{
    public class BattleRecordTextUIComponent : BattleUIComponent
    {
        private EventService _eventService;
        private FairyGUIService _fairyService;
        private GRichTextField _recordTxt;

        private readonly List<BattleRecordTextInfo> _infos = new();

        public BattleRecordTextUIComponent(BattleUIManager manager, BaseBattleInstance battle) : base(manager, battle)
        {
            _eventService = GameFramework.GetService<EventService>();
            _fairyService = GameFramework.GetService<FairyGUIService>();
            _eventService.On<BattleOnUnitAction>(GameEvents.BattleEvent.OnUnitAction, OnUnitAction);
        }

        private void OnUnitAction(BattleOnUnitAction evt)
        {
            var actionUnit = evt.UnitEntity;
            var castInfo = evt.SkillCastInfo;
            if (evt.ActionState == BattleOnUnitAction.Action.Casting)
            {
                //释放技能
                var info = new BattleRecordTextInfo(Battle, actionUnit, castInfo);
                _infos.Insert(0, info);
                RefreshRecordText();
            }
            else if (evt.ActionState == BattleOnUnitAction.Action.Hitting)
            {
                //技能命中
                foreach (var info in _infos)
                {
                    if (info.ActionUnit == actionUnit &&
                        info.SkillCastInfo.CastSkill.Skill == evt.SkillCastInfo.CastSkill.Skill)
                    {
                        info.SetStateHit();
                        RefreshRecordText();
                        break;
                    }
                }
            }
        }

        private void RefreshRecordText()
        {
            var sb = new StringBuilder();
            foreach (var info in _infos)
            {
                var txt = info.ToRichText();
                if (string.IsNullOrEmpty(txt)) continue;
                sb.Append(txt);
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }

            var str = sb.ToString();
            _recordTxt.text = str;
        }

        internal override void Load()
        {
            _recordTxt = Manager.UIBattlePage.GetChildByPath("TxtBattleRecord.TextField").asRichTextField;
        }

        internal override void Release()
        {
            _eventService.Remove<BattleOnUnitAction>(GameEvents.BattleEvent.OnUnitAction, OnUnitAction);
            _infos.Clear();
        }

        internal override void OnUpdate(float elapseTime, float realElapseTime)
        {
        }
    }
}