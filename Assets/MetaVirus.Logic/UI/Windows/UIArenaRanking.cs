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
        private GTextField _textPlayerRanking;
        private GList _listRanking;
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
            _textPlayerRanking = content.GetChildByPath("playerInfo.text_playerRank").asTextField;
            _textPlayerScore = content.GetChildByPath("playerInfo.text_playerScore").asTextField;
            _textSeasonNo = content.GetChildByPath("playerInfo.text_seasonNum").asTextField;
            
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
            _rankingData = task.Result.Result;
            // foreach (var listData in data.Result)
            // {
            //     //Debug.Log(list.PlayerName, list.ArenaInfo.Rank, list.ArenaInfo.Score);
            //     var listItem = new ArenaRankingListItem();
            //     listItem.RenderRankingListItem(listData);
            //     _listRanking.AddChild(listItem._rankingListItem);
            // }
            _listRanking.itemRenderer = RenderRankingList;
            _listRanking.numItems = _rankingData.Count;
        }

        private void RenderRankingList(int index, GObject obj)
        {
            var rankingListItem = obj.asCom;
            
            // var rankingMedal = new RankingMedal(rankingListItem, index, _rankingData);
            // rankingMedal.RenderMedal();
            var playerRanking = _rankingData[index].ArenaInfo.Rank;
            RankingMedal.RenderMedal(rankingListItem, playerRanking, index, _rankingData);
            // switch (index)
            // {
            //     case 0:
            //         var goldMedal = UIPackage.CreateObject("Common", "icon_itemicon_medalgold").asImage;
            //         goldMedal.SetSize(40,50);
            //         rankingListItem.AddChild(goldMedal).SetXY(40,40);
            //         break;
            //     case 1:
            //         var silverMedal = UIPackage.CreateObject("Common", "icon_itemicon_medalsilver").asImage;
            //         silverMedal.SetSize(40,50);
            //         rankingListItem.AddChild(silverMedal).SetXY(40,40);
            //         break;
            //     case 2:
            //         var bronzeMedal = UIPackage.CreateObject("Common", "icon_itemicon_medalbronze").asImage;
            //         bronzeMedal.SetSize(40,50);
            //         rankingListItem.AddChild(bronzeMedal).SetXY(40,40);
            //         break;
            //     default:
            //         var textPlayerRanking = rankingListItem.GetChild("text_ranking").asTextField;
            //         var playerRanking = _rankingData[index].ArenaInfo.Rank;
            //         textPlayerRanking.text = Convert.ToString(playerRanking);
            //         break;
            // }
            var textPlayerName = rankingListItem.GetChild("text_playerName").asTextField;
            var textPlayerLv = rankingListItem.GetChild("text_playerLv").asTextField;
            var textPlayerScore = rankingListItem.GetChild("text_score").asTextField;
            textPlayerName.text = _rankingData[index].PlayerName;
            textPlayerLv.text = "Lv " + Convert.ToString(_rankingData[index].PlayerLevel);
            textPlayerScore.text = Convert.ToString(_rankingData[index].ArenaInfo.Score);
        }
    }
}