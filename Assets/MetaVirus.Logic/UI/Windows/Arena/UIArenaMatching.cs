using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Utils;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Arena.data;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.Common;
using MetaVirus.Logic.Utils;
using UnityEngine;
using Constants = MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.UI.Windows
{
    [UIWindow("ui_arena_matching")]
    public class UIArenaMatching : BaseUIWindow
    {
        private ArenaService _arenaService;
        private GList _listMatching;
        private List<ArenaPlayerData> _matchingData;
        private UIService _uiService;
        private DataNodeService _dataNodeService;

        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaMatchingUI").asCom;
            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _arenaService = GameFramework.GetService<ArenaService>();
            _uiService = GameFramework.GetService<UIService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();

            _listMatching = content.GetChild("listMatching").asList;

            GameFramework.Inst.StartCoroutine(GetArenaMatchList());
        }

        private IEnumerator GetArenaMatchList()
        {
            var dt = _arenaService.GetPlayerArenaData(1,PlayerEntity.Current.Id);
            var task = _arenaService.GetArenaMatchList(1);


            yield return dt.AsCoroution();
            yield return task.AsCoroution();

            if (dt.Result.IsSuccess)
            {
                var arenaData = dt.Result.Result;

                var txtSeasonNum = ContentComp.GetChild("text_seasonNum").asTextField;
                var txtHours = ContentComp.GetChild("text_remainingTime").asTextField;
                txtSeasonNum.text = arenaData.ArenaInfo.SeasonNo.ToString();
                txtHours.text = GameUtil.HourToString(arenaData.ArenaInfo.hoursRemaining);

                var matchingListItem = ContentComp.GetChild("n4").asCom;

                RenderMatchingListItem(matchingListItem, arenaData);
            }

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
                _matchingData = task.Result.Result;
                // foreach (var listData in data.Result)
                // {
                //     var listItem = new ArenaMatchingListItem(listData.PlayerId);
                //     listItem.RenderMatchingListItem(listData);
                //     _listMatching.AddChild(listItem.MatchingListItem);
                // }
                _listMatching.itemRenderer = RenderMatchingList;
                _listMatching.numItems = _matchingData.Count;
            }
        }

        private void RenderMatchingList(int index, GObject obj)
        {
            var matchingListItem = obj.asCom;
            var arenaData = _matchingData[index];
            RenderMatchingListItem(matchingListItem, arenaData);
            // var textLv = matchingListItem.GetChild("text_opponentLv").asTextField;
            // var textOpponentName = matchingListItem.GetChild("text_opponentName").asTextField;
            // var textOpponentScore = matchingListItem.GetChild("text_opponentScore").asTextField;
            // var buttonChallenge = matchingListItem.GetChild("button_challenge").asButton;
            // var playerRanking = _matchingData[index].ArenaInfo.Rank;
            //
            // RankingMedal.RenderMedal(matchingListItem, playerRanking);
            //
            // textLv.text = Convert.ToString(_matchingData[index].PlayerLevel);
            // textOpponentName.text = _matchingData[index].PlayerName;
            // textOpponentScore.text = Convert.ToString(_matchingData[index].ArenaInfo.Score);
            // buttonChallenge.onClick.Set(() =>
            // {
            //     Debug.Log(_matchingData[index].PlayerId);
            //     _dataNodeService.SetData(Constants.DataKeys.UIArenaMatchingOpponentData, _matchingData[index].PlayerId);
            //     _uiService.OpenWindow<UIPreparation>();
            // });
        }

        private void RenderMatchingListItem(GComponent matchingListItem, ArenaPlayerData arenaPlayerData)
        {
            var textLv = matchingListItem.GetChild("text_opponentLv").asTextField;
            var textOpponentName = matchingListItem.GetChild("text_opponentName").asTextField;
            var textOpponentScore = matchingListItem.GetChild("text_opponentScore").asTextField;
            var buttonChallenge = matchingListItem.GetChild("button_challenge").asButton;
            var playerRanking = arenaPlayerData.ArenaInfo.Rank;

            RankingMedal.RenderMedal(matchingListItem, playerRanking);

            textLv.text = Convert.ToString(arenaPlayerData.PlayerLevel);
            textOpponentName.text = arenaPlayerData.PlayerName;
            textOpponentScore.text = Convert.ToString(arenaPlayerData.ArenaInfo.Score);
            buttonChallenge.onClick.Set(() =>
            {
                _dataNodeService.SetData(Constants.DataKeys.UIArenaMatchingOpponentData, arenaPlayerData.PlayerId);
                _uiService.OpenWindow<UIPreparation>();
            });
        }
    }
}