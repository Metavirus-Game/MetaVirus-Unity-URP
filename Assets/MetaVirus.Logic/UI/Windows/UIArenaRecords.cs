using System;
using System.Collections;
using FairyGUI;
using GameEngine;
using GameEngine.Utils;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using UnityEngine;

namespace MetaVirus.Logic.UI.Windows
{   
    [UIWindow("ui_arena_record")]
    public class UIArenaRecords: BaseUIWindow
    {
        private ArenaService _arenaService;
        private PlayerService _playerService;
        private GTextField _textWinCount;
        private GTextField _textLoseCount;
        private GTextField _textDrawCount;
        private GTextField _textSeasonNo;
        private GTextField _textPlayerScore;
        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaRecordsUI").asCom;
            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _arenaService = GameFramework.GetService<ArenaService>();
            _playerService = GameFramework.GetService<PlayerService>();
            GameFramework.Inst.StartCoroutine(GetArenaRecords());
            _textDrawCount = content.GetChildByPath("playerInfo.text_drawNum").asTextField;
            _textLoseCount = content.GetChildByPath("playerInfo.text_loseNum").asTextField;
            _textWinCount = content.GetChildByPath("playerInfo.text_winNum").asTextField;
            _textPlayerScore = content.GetChildByPath("playerInfo.text_playerScore").asTextField;
            _textSeasonNo = content.GetChildByPath("playerInfo.text_seasonNum").asTextField;
      
        }

        private IEnumerator GetArenaRecords()
        {
            var playerInfo = _playerService.CurrentPlayerInfo;
            var task = _arenaService.GetPlayerArenaData(1, playerInfo.PlayerId);
            yield return task.AsCoroution();
            var data = task.Result;
            _textWinCount.text = Convert.ToString(data.Result.ArenaInfo.WinCount);
            _textLoseCount.text = Convert.ToString(data.Result.ArenaInfo.LoseCount);
            _textDrawCount.text = Convert.ToString(data.Result.ArenaInfo.DrawCount);
            _textSeasonNo.text = Convert.ToString(data.Result.ArenaInfo.SeasonNo);
            _textPlayerScore.text = Convert.ToString(data.Result.ArenaInfo.Score);
        }
    }
}