using System.Collections.Generic;
using cfg.buff;
using cfg.skill;
using GameEngine;
using GameEngine.Utils;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Logic.Service;
using Unity.VisualScripting;

namespace MetaVirus.Logic.Data.Battle
{
    public class MonsterSkillAttachBuff
    {
        private BuffAttach _buffAttach;
        private IMonsterDataProvider _skillOwner;
        private GameDataService _gameDataService;

        public MonsterSkillAttachBuff(BuffAttach buffAttach, IMonsterDataProvider skillOwner)
        {
            _buffAttach = buffAttach;
            _skillOwner = skillOwner;
            _gameDataService = GameFramework.GetService<GameDataService>();
        }

        public string BuffDesc
        {
            get
            {
                var strKey = "battle.skill.desc.attach.buff";

                var clr = _buffAttach.BuffId_Ref.Type == BuffType.Buff
                    ? _gameDataService.BattleColorHpInc().ToHtmlColor()
                    : _gameDataService.BattleColorHpDec().ToHtmlColor();

                var name = $"<a href=''>[color={clr}]{_buffAttach.BuffId_Ref.Name}[/color]</a>";
                var level = _buffAttach.BuffLevel;
                var round = _buffAttach.AttachCount;

                var map = new Dictionary<string, string>
                {
                    { "%level", level.ToString() },
                    { "%name", name },
                    { "%round", round.ToString() }
                };

                return _gameDataService.GetLocalizeStr(strKey, map);
            }
        }
    }
}