using System.Collections;
using FairyGUI;
using GameEngine;
using GameEngine.Utils;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using System;

namespace MetaVirus.Logic.UI.Windows
{   
    [UIWindow("ui_arena_ranking")]
    public class UIArenaRanking : BaseUIWindow
    {
        private ArenaService _arenaService;
        private PlayerService _playerService;
        private GTextField _textPlayerScore;
        private GTextField _textSeasonNo;
        private GTextField _textPlayerRanking;
        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaRankingUI").asCom;
            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _arenaService = GameFramework.GetService<ArenaService>();
            _playerService = GameFramework.GetService<PlayerService>();
            GameFramework.Inst.StartCoroutine(GetArenaTopRankList());
            GameFramework.Inst.StartCoroutine(GetPlayerArenaData());
            _textPlayerRanking = content.GetChildByPath("playerInfo.text_playerRanking").asTextField;
            _textPlayerScore = content.GetChildByPath("playerInfo.text_playerScore").asTextField;
            _textSeasonNo = content.GetChildByPath("playerInfo.text_seasonNum").asTextField;
        }
        
        private IEnumerator GetPlayerArenaData()
        {   
            // get player Data
            var playerInfo = _playerService.CurrentPlayerInfo;
            var task = _arenaService.GetPlayerArenaData(1, playerInfo.PlayerId);
            yield return task.AsCoroution();
            var data = task.Result;
            _textSeasonNo.text = Convert.ToString(data.Result.ArenaInfo.SeasonNo);
            _textPlayerScore.text = Convert.ToString(data.Result.ArenaInfo.Score);
            _textPlayerRanking.text = Convert.ToString(data.Result.ArenaInfo.Rank);
        }
        
        private IEnumerator GetArenaTopRankList()
        {
            // get ranking list data
            var task = _arenaService.GetArenaTopRankList(1);
            yield return task.AsCoroution();
            var data = task.Result;
        }
    }
}