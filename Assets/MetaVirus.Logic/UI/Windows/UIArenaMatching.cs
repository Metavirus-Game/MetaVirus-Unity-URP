using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Utils;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Arena.data;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.Common;
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
            GameFramework.Inst.StartCoroutine(GetArenaMatchList());
            _listMatching = content.GetChild("listMatching").asList;
        }

        private IEnumerator GetArenaMatchList()
        {
            var task = _arenaService.GetArenaMatchList(1);
            yield return task.AsCoroution();
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

        private void RenderMatchingList(int index, GObject obj)
        {
            var matchingListItem = obj.asCom;
            var textLv = matchingListItem.GetChild("text_opponentLv").asTextField;
            var textOpponentName = matchingListItem.GetChild("text_opponentName").asTextField;
            var textOpponentScore = matchingListItem.GetChild("text_opponentScore").asTextField;
            var buttonChallenge = matchingListItem.GetChild("button_challenge").asButton;
            var playerRanking = _matchingData[index].ArenaInfo.Rank;
      
            RankingMedal.RenderMedal(matchingListItem, playerRanking, index, _matchingData);

            textLv.text = "Lv " + Convert.ToString(_matchingData[index].PlayerLevel);
            textOpponentName.text = _matchingData[index].PlayerName;
            textOpponentScore.text = Convert.ToString(_matchingData[index].ArenaInfo.Score);
            buttonChallenge.onClick.Set(() =>
            {   
                Debug.Log(_matchingData[index].PlayerId);
                _dataNodeService.SetData(Constants.DataKeys.UIArenaMatchingOpponentData, _matchingData[index].PlayerId);
                _uiService.OpenWindow<UIPreparartion>();
            });
        }
    }
}