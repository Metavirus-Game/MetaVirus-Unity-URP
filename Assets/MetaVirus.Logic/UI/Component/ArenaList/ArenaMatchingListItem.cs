using System;
using FairyGUI;
using GameEngine;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Arena.data;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Windows;
using UnityEngine;

namespace MetaVirus.Logic.UI.Component.ArenaList
{
    public class ArenaMatchingListItem
    {
        private GTextField _textRanking;
        private GTextField _textEnemyName;
        public GComponent _matchingListItem;
        private GButton _buttonChallenge;
        private UIService _uiService;
        //private ArenaService _arenaService;
        public ArenaMatchingListItem()
        {
            _matchingListItem = UIPackage.CreateObject("Common", "MatchingListFrame").asCom;
            _textRanking = _matchingListItem.GetChild("text_ranking").asTextField;
            _textEnemyName = _matchingListItem.GetChild("text_enemyName").asTextField;
            _buttonChallenge = _matchingListItem.GetChild("button_challenge").asButton;
            //_arenaService = GameFramework.GetService<ArenaService>();
            _uiService = GameFramework.GetService<UIService>();
        }

        public void RenderMatchingListItem(ArenaPlayerData matchingData)
        {
            _textRanking.text = Convert.ToString(matchingData.ArenaInfo.Rank);
            _textEnemyName.text = matchingData.PlayerName;
            // var task = _arenaService
            _buttonChallenge.onClick.Set(() =>
            {
                _uiService.OpenWindow<UIPreparartion>();
            });
        }
    }
}