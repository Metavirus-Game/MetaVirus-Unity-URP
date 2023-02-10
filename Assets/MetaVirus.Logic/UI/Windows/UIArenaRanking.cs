using System.Collections;
using FairyGUI;
using GameEngine;
using GameEngine.Utils;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using System;
using MetaVirus.Logic.Service.Arena.data;
using MetaVirus.Logic.UI.Component.ArenaList;
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
            _textPlayerRanking = content.GetChildByPath("playerInfo.text_playerRanking").asTextField;
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
            var data = task.Result;
            foreach (var listData in data.Result)
            {
                //Debug.Log(list.PlayerName, list.ArenaInfo.Rank, list.ArenaInfo.Score);
                var listItem = new ArenaRankingListItem();
                listItem.RenderRankingListItem(listData);
                _listRanking.AddChild(listItem._rankingListItem);
            }
        }
    }
}