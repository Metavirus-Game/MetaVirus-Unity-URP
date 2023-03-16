using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using GameEngine;
using GameEngine.Event;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Events.Arena;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Arena.data;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.Common;
using UnityEngine;

namespace MetaVirus.Logic.UI.Windows
{
    [UIWindow("ui_arena_record")]
    public class UIArenaRecords : BaseUIWindow
    {
        private ArenaService _arenaService;
        private PlayerService _playerService;
        private GTextField _textWinCount;
        private GTextField _textLoseCount;
        private GTextField _textDrawCount;
        private GTextField _textSeasonNo;

        private GTextField _textPlayerScore;

        // private GTextField _textPlayerRank;
        private GTextField _textPlayerName;
        private GComponent _playerInfoComp;
        private List<ArenaPlayerRecord> _playerRecord;
        private GList _listRecords;

        private EventService _eventService;
        // private DataNodeService _dataNodeService = GameFramework.GetService<DataNodeService>();


        private PlayerInfo _playerInfo;

        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaRecordsUI").asCom;


            // comp.onClick.Add((context) => { context.StopPropagation(); });

            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _playerInfo = PlayerEntity.Current.PlayerInfo;

            _eventService = GameFramework.GetService<EventService>();
            _eventService.On<NewRecordNotifitionEvent>(GameEvents.ArenaEvent.NewRecordNotifition, OnNewRecords);
            _arenaService = GameFramework.GetService<ArenaService>();
            _playerService = GameFramework.GetService<PlayerService>();
            GameFramework.Inst.StartCoroutine(GetPlayerArenaData());
            //_textDrawCount = content.GetChildByPath("playerInfo.text_drawNum").asTextField;
            _textLoseCount = content.GetChildByPath("playerInfo.text_loseCount").asTextField;
            _textWinCount = content.GetChildByPath("playerInfo.text_winCount").asTextField;
            _textPlayerScore = content.GetChildByPath("playerInfo.text_playerScore").asTextField;
            // _textPlayerRank = content.GetChildByPath("playerInfo.text_playerRank").asTextField;
            _textSeasonNo = content.GetChildByPath("playerInfo.text_seasonNum").asTextField;
            _textPlayerName = content.GetChildByPath("playerInfo.text_playerName").asTextField;
            _playerInfoComp = content.GetChild("playerInfo").asCom;

            // _dataNodeService = GameFramework.GetService<DataNodeService>();
            GameFramework.Inst.StartCoroutine(GetArenaPlayerRecords());
            _listRecords = content.GetChild("listRecords").asList;
        }

        private void OnNewRecords(NewRecordNotifitionEvent evt)
        {
            _playerRecord = _arenaService.GetArenaPlayerRecordsCache(1);
            _listRecords.numItems = _playerRecord.Count;
            GameFramework.Inst.StartCoroutine(GetPlayerArenaData());
        }

        private IEnumerator GetPlayerArenaData()
        {
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
                _textWinCount.text = Convert.ToString(data.Result.ArenaInfo.WinCount);
                _textLoseCount.text = Convert.ToString(data.Result.ArenaInfo.LoseCount);
                //_textDrawCount.text = Convert.ToString(data.Result.ArenaInfo.DrawCount);
                _textSeasonNo.text = Convert.ToString(data.Result.ArenaInfo.SeasonNo);
                _textPlayerScore.text = Convert.ToString(data.Result.ArenaInfo.Score);
                // _textPlayerRank.text = Convert.ToString(data.Result.ArenaInfo.Rank);
                _textPlayerName.text = playerInfo.Name;
                RankingMedal.RenderMedal(_playerInfoComp, data.Result.ArenaInfo.Rank);
            }
        }

        private IEnumerator GetArenaPlayerRecords()
        {
            var task = _arenaService.GetArenaPlayerRecords(1);
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
                _playerRecord = task.Result.Result;
                Debug.Log(_playerRecord);
                _listRecords.itemRenderer = RenderRecordsList;
                _listRecords.numItems = _playerRecord.Count;
            }
        }

        private void RenderRecordsList(int index, GObject obj)
        {
            var comp = obj.asCom;
            var textResult = comp.GetChild("text_result").asTextField;
            var txtAttack = comp.GetChild("txtAttack").asTextField;
            var txtDefence = comp.GetChild("txtDefence").asTextField;

            var record = _playerRecord[index];

            txtAttack.text = record.AttackId == _playerInfo.PlayerId ? "You" : record.AttackName;
            txtDefence.text = record.DefenceId == _playerInfo.PlayerId ? "You" : record.DefenceName;

            var winCtrl = comp.GetController("win");
            var newCtrl = comp.GetController("new");

            //这个战斗结果是以本场战斗的attack为主体，如果玩家是defence，需要调转result
            var result = record.Result;
            if (record.DefenceId == _playerInfo.PlayerId)
            {
                result = 2 - result;
            }

            winCtrl.SetSelectedIndex(result);

            var buttonReplay = comp.GetChild("button_replay").asButton;
            buttonReplay.onClick.Set(() => { GameFramework.Inst.StartCoroutine(ReplayArenaRecord(index)); });
            //var buttonFight = comp.GetChild("button_fight").asButton;
            // switch (_playerRecord[index].Result)
            // {
            //     case 0:
            //         textResult.text = "Win";
            //         textResult.color = Color.red;
            //         break;
            //     case 1:
            //         textResult.text = "Draw";
            //         textResult.color = Color.white;
            //         break;
            //     case 2:
            //         textResult.text = "Lose";
            //         textResult.color = Color.blue;
            //         break;
            // }

            // textName.text = _playerRecord[index].AttackName;
        }

        private IEnumerator ReplayArenaRecord(int recordIndex)
        {
            if (recordIndex >= 0 && recordIndex < _playerRecord.Count)
            {
                var record = _playerRecord[recordIndex];

                if (record.BattleRecord is { Length: > 0 })
                {
                    //已有记录，直接进入战斗
                    var br = BattleRecord.FromGZipData(record.BattleRecord);
                    BattleService.EnterBattle(br);
                }

                var waitingWnd = UIWaitingWindow.ShowWaiting();
                var task = _arenaService.GetArenaPlayerRecord(1, record.RecordId);
                yield return task.AsCoroution();
                waitingWnd.Hide();
                var nr = task.Result;
                if (nr.IsTimeout)
                {
                    UIDialog.ShowTimeoutMessage();
                }
                else if (nr.IsError)
                {
                    UIDialog.ShowErrorMessage(nr.MessageCode);
                }
                else
                {
                    var abr = nr.Result;
                    record.BattleRecord = abr.BattleRecord;
                    var br = BattleRecord.FromGZipData(abr.BattleRecord);
                    BattleService.EnterBattle(br);
                }
            }
        }

        public override void Release()
        {
            _eventService.Remove<NewRecordNotifitionEvent>(GameEvents.ArenaEvent.NewRecordNotifition, OnNewRecords);
        }
    }
}