using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using GameEngine;
using GameEngine.Event;
using GameEngine.Network;
using GameEngine.Utils;
using UnityEngine;

namespace  MetaVirus.Logic.UI.Windows
{   
    // 不算全局窗体类，需要添加这个属性吗？
    [UIWindow("ui_arena_main")]
    public class UIArenaMain : BaseUIWindow
    {
        private ArenaService _arenaService;
        private PlayerService _playerService;
        private GTextField _textPlayerScore;
        private GTextField _textSeasonNo;
        private GTextField _textRemainingTime;
        private GButton _btnStart;
        private GButton _btnRanking;
        private GButton _btnRecords;
        private UIService _uiService;

        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaMainUI").asCom;
            return comp;
        }
        
        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _arenaService = GameFramework.GetService<ArenaService>();
            _playerService = GameFramework.GetService<PlayerService>();
            GameFramework.Inst.StartCoroutine(GetPlayerArenaData());
            _textPlayerScore = content.GetChild("text_mainScore").asTextField;
            _textSeasonNo = content.GetChild(("text_seasonNum")).asTextField;
            _textRemainingTime = content.GetChild("text_remainingTime").asTextField;
            _btnStart = content.GetChild("button_start").asButton;
            _btnRecords = content.GetChild("button_records").asButton;
            _btnRanking = content.GetChild("button_ranking").asButton;

            _uiService = GameFramework.GetService<UIService>();

            _btnRecords.onClick.Set(() =>
                _uiService.OpenWindow<UIArenaRecords>());
            
            _btnRanking.onClick.Set(()=> _uiService.OpenWindow<UIArenaRanking>());
            
            _btnStart.onClick.Set(()=> _uiService.OpenWindow<UIArenaMatching>());
        }

        private IEnumerator GetPlayerArenaData()
        {
            var playerInfo = _playerService.CurrentPlayerInfo;
            var task = _arenaService.GetPlayerArenaData(1, playerInfo.PlayerId);
            yield return task.AsCoroution();
            var data = task.Result;
            _textSeasonNo.text = Convert.ToString(data.Result.ArenaInfo.SeasonNo);
            _textPlayerScore.text = Convert.ToString(data.Result.ArenaInfo.Score);
            _textRemainingTime.text = Convert.ToString(data.Result.ArenaInfo.hoursRemaining);
        }
    }
}
