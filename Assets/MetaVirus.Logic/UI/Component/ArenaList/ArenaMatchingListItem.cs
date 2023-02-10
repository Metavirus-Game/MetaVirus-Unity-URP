using System;
using FairyGUI;
using MetaVirus.Logic.Service.Arena.data;
using UnityEngine;

namespace MetaVirus.Logic.UI.Component.ArenaList
{
    public class ArenaMatchingListItem
    {
        private GTextField _textRanking;
        private GTextField _textEnemyName;
        public GComponent _matchingListItem;
        private GButton _buttonChallenge;

        public ArenaMatchingListItem()
        {
            _matchingListItem = UIPackage.CreateObject("Common", "MatchingListFrame").asCom;
            _textRanking = _matchingListItem.GetChild("text_ranking").asTextField;
            _textEnemyName = _matchingListItem.GetChild("text_enemyName").asTextField;
            _buttonChallenge = _matchingListItem.GetChild("button_challenge").asButton;
        }

        public void RenderMatchingListItem(ArenaPlayerData matchingData)
        {
            _textRanking.text = Convert.ToString(matchingData.ArenaInfo.Rank);
            _textEnemyName.text = matchingData.PlayerName;
        }
    }
}