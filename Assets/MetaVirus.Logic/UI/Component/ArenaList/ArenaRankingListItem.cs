using System;
using FairyGUI;
using MetaVirus.Logic.Service.Arena.data;
using UnityEngine;
using UnityEngine.tvOS;

namespace MetaVirus.Logic.UI.Component.ArenaList
{
    public class ArenaRankingListItem
    {
        private GTextField _textPlayerRanking;
        private GTextField _textPlayerName;
        private GTextField _textPlayerScore;
        public GComponent _rankingListItem;

        public ArenaRankingListItem()
        {
            _rankingListItem = UIPackage.CreateObject("Common", "RankingListFrame").asCom;
            _textPlayerName = _rankingListItem.GetChild("text_playerName").asTextField;
            _textPlayerRanking = _rankingListItem.GetChild("text_ranking").asTextField;
            _textPlayerScore = _rankingListItem.GetChild("text_score").asTextField;
        }

        public void RenderRankingListItem(ArenaPlayerData rankingData)
        {
            _textPlayerName.text = rankingData.PlayerName;
            _textPlayerRanking.text = Convert.ToString(rankingData.ArenaInfo.Rank);
            _textPlayerScore.text = Convert.ToString(rankingData.ArenaInfo.Score);
        }
    }
}