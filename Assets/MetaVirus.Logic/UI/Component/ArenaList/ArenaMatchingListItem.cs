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
        private GTextField _textOpponentName;
        public GComponent MatchingListItem;
        private GButton _buttonChallenge;
        private UIService _uiService;
        //private ArenaService _arenaService;
        public ArenaMatchingListItem()
        {
            MatchingListItem = UIPackage.CreateObject("Common", "MatchingListFrame").asCom;
            _textRanking = MatchingListItem.GetChild("text_ranking").asTextField;
            _textOpponentName = MatchingListItem.GetChild("text_opponentName").asTextField;
            _buttonChallenge = MatchingListItem.GetChild("button_challenge").asButton;
            //_arenaService = GameFramework.GetService<ArenaService>();
            _uiService = GameFramework.GetService<UIService>();
        }

        public void RenderMatchingListItem(ArenaPlayerData matchingData)
        {
            _textRanking.text = Convert.ToString(matchingData.ArenaInfo.Rank);
            _textOpponentName.text = matchingData.PlayerName;
            // var task = _arenaService
            _buttonChallenge.onClick.Set(() =>
            {   
                _uiService.OpenWindow<UIPreparartion>();
            });
        }
    }
}