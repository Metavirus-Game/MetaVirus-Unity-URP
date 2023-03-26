using System.Collections;
using FairyGUI;
using GameEngine;
using GameEngine.Utils;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using System;
using System.Collections.Generic;
using MetaVirus.Logic.Service.Arena.data;
using MetaVirus.Logic.UI.Component.Common;
using UnityEngine;

namespace MetaVirus.Logic.UI.Windows
{
    [UIWindow("ui_arena_ranking")]
    public class UIArenaRanking : BaseUIWindow
    {
        private ArenaService _arenaService;
        private PlayerService _playerService;
        private GTextField _textPlayerScore;
        private GTextField _textSeasonNo;
        private GTextField _textWinCount;

        private GTextField _textLoseCount;

        //private GTextField _textPlayerRanking;
        private GComponent _playerInfoComp;
        private GTextField _textPlayerName;
        private GList _listRanking;
        private GButton _playerRankComp;
        private List<ArenaPlayerData> _rankingData;

        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaRankingUI").asCom;
            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            // get player data
            _playerService = GameFramework.GetService<PlayerService>();
            _arenaService = GameFramework.GetService<ArenaService>();
            GameFramework.Inst.StartCoroutine(GetPlayerArenaData());
            //_textPlayerRanking = content.GetChildByPath("playerInfo.text_playerRank").asTextField;
            _textPlayerScore = content.GetChildByPath("playerInfo.text_playerScore").asTextField;
            _textSeasonNo = content.GetChildByPath("playerInfo.text_seasonNum").asTextField;
            _textPlayerName = content.GetChildByPath("playerInfo.text_playerName").asTextField;
            _textWinCount = content.GetChildByPath("playerInfo.text_winCount").asTextField;
            _textLoseCount = content.GetChildByPath("playerInfo.text_loseCount").asTextField;
            _playerInfoComp = content.GetChild("playerInfo").asCom;
            // get ranking data
            GameFramework.Inst.StartCoroutine(GetArenaTopRankList());
            _listRanking = content.GetChild("listRanking").asList;
        }

        private IEnumerator GetPlayerArenaData()
        {
            // get player ranking Data
            var playerInfo = _playerService.CurrentPlayerInfo;
            var task = _arenaService.GetPlayerArenaData(1, playerInfo.PlayerId);
            yield return task.AsCoroution();
            if (task.Result.IsTimeout)
            {
                UIDialog.ShowTimeoutMessage();
            }
            else if (task.Result.IsError)
            {
                UIDialog.ShowErrorMessage(task.Result.MessageCode);
            }
            else
            {
                var data = task.Result;
                RenderPlayerInfo(data.Result, playerInfo.Name);

                _playerRankComp = ContentComp.GetChild("playerRank").asButton;
                RenderRankingList(data.Result, _playerRankComp);
                _playerRankComp.onChanged.Set(() =>
                {
                    if (!_playerRankComp.selected) return;
                    RenderPlayerInfo(data.Result, playerInfo.Name);
                    _listRanking.SelectNone();
                });
                _playerRankComp.selected = true;
            }
        }

        private void RenderPlayerInfo(ArenaPlayerData playerData, string playerName = null)
        {
            playerName ??= playerData.PlayerName;

            _textSeasonNo.text = playerData.ArenaInfo.SeasonNo.ToString();
            _textPlayerScore.text = playerData.ArenaInfo.Score.ToString();
            _textPlayerName.text = playerName;
            _textWinCount.text = playerData.ArenaInfo.WinCount.ToString();
            _textLoseCount.text = playerData.ArenaInfo.LoseCount.ToString();
            RankingMedal.RenderMedal(_playerInfoComp, playerData.ArenaInfo.Rank);
        }

        private IEnumerator GetArenaTopRankList()
        {
            // get ranking list data
            var task = _arenaService.GetArenaTopRankList(1);
            yield return task.AsCoroution();
            if (task.Result.IsTimeout)
            {
                UIDialog.ShowTimeoutMessage();
            }
            else if (task.Result.IsError)
            {
                UIDialog.ShowErrorMessage(task.Result.MessageCode);
            }
            else
            {
                _rankingData = task.Result.Result;
                _listRanking.itemRenderer = RenderRankingList;
                _listRanking.numItems = _rankingData.Count;
                _listRanking.onClickItem.Set(context =>
                {
                    var index = _listRanking.GetChildIndex(context.data as GObject);
                    var rankingData = _rankingData[index];
                    RenderPlayerInfo(rankingData);
                    _playerRankComp.selected = false;
                });
            }
        }

        private void RenderRankingList(int index, GObject obj)
        {
            var rankingListItem = obj.asCom;
            var arenaPlayerData = _rankingData[index];
            RenderRankingList(arenaPlayerData, rankingListItem);

            // var playerRanking = _rankingData[index].ArenaInfo.Rank;
            // RankingMedal.RenderMedal(rankingListItem, playerRanking);
            // var textPlayerName = rankingListItem.GetChild("text_playerName").asTextField;
            // var textPlayerLv = rankingListItem.GetChild("text_playerLv").asTextField;
            // var textPlayerScore = rankingListItem.GetChild("text_score").asTextField;
            // textPlayerName.text = _rankingData[index].PlayerName;
            // textPlayerLv.text = "Lv " + Convert.ToString(_rankingData[index].PlayerLevel);
            // textPlayerScore.text = Convert.ToString(_rankingData[index].ArenaInfo.Score);
        }

        private void RenderRankingList(ArenaPlayerData arenaPlayerData, GObject obj)
        {
            var rankingListItem = obj.asCom;
            var playerRanking = arenaPlayerData.ArenaInfo.Rank;
            RankingMedal.RenderMedal(rankingListItem, playerRanking);
            var textPlayerName = rankingListItem.GetChild("text_playerName").asTextField;
            var textPlayerLv = rankingListItem.GetChild("text_playerLv").asTextField;
            var textPlayerScore = rankingListItem.GetChild("text_score").asTextField;
            textPlayerName.text = arenaPlayerData.PlayerName;
            textPlayerLv.text = Convert.ToString(arenaPlayerData.PlayerLevel);
            textPlayerScore.text = Convert.ToString(arenaPlayerData.ArenaInfo.Score);
        }
    }
}