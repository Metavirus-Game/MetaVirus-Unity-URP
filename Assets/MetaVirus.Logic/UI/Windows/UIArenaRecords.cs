using System;
using System.Collections;
using System.Collections.Generic;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Event;
using GameEngine.Network;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Events.Arena;
using MetaVirus.Logic.Data.Network;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Arena.data;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using Unity.VisualScripting;
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
        private List<ArenaPlayerRecord> _playerRecord;
        private GList _listRecords;

        private EventService _eventService;
        // private DataNodeService _dataNodeService = GameFramework.GetService<DataNodeService>();

        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaRecordsUI").asCom;


            // comp.onClick.Add((context) => { context.StopPropagation(); });

            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _eventService = GameFramework.GetService<EventService>();
            _eventService.On<NewRecordNotifitionEvent>(GameEvents.ArenaEvent.NewRecordNotifition, OnNewRecords);
            _arenaService = GameFramework.GetService<ArenaService>();
            _playerService = GameFramework.GetService<PlayerService>();
            GameFramework.Inst.StartCoroutine(GetPlayerArenaData());
            _textDrawCount = content.GetChildByPath("playerInfo.text_drawNum").asTextField;
            _textLoseCount = content.GetChildByPath("playerInfo.text_loseNum").asTextField;
            _textWinCount = content.GetChildByPath("playerInfo.text_winNum").asTextField;
            _textPlayerScore = content.GetChildByPath("playerInfo.text_playerScore").asTextField;
            _textSeasonNo = content.GetChildByPath("playerInfo.text_seasonNum").asTextField;

            // _dataNodeService = GameFramework.GetService<DataNodeService>();
            GameFramework.Inst.StartCoroutine(GetArenaPlayerRecords());
            _listRecords = content.GetChild("listRecords").asList;
        }

        private void OnNewRecords(NewRecordNotifitionEvent evt)
        {
            _playerRecord = _arenaService.GetArenaPlayerRecordsCache(1);
            _listRecords.numItems = _playerRecord.Count;
        }

        private IEnumerator GetPlayerArenaData()
        {
            var playerInfo = _playerService.CurrentPlayerInfo;
            var task = _arenaService.GetPlayerArenaData(1, playerInfo.PlayerId);
            yield return task.AsCoroution();
            var data = task.Result;
            _textWinCount.text = Convert.ToString(data.Result.ArenaInfo.WinCount);
            _textLoseCount.text = Convert.ToString(data.Result.ArenaInfo.LoseCount);
            _textDrawCount.text = Convert.ToString(data.Result.ArenaInfo.DrawCount);
            _textSeasonNo.text = Convert.ToString(data.Result.ArenaInfo.SeasonNo);
            _textPlayerScore.text = Convert.ToString(data.Result.ArenaInfo.Score);
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
            var textName = comp.GetChild("text_opponentName").asTextField;
            var buttonReplay = comp.GetChild("button_replay").asButton;
            var buttonFight = comp.GetChild("button_fight").asButton;
            Debug.Log(_playerRecord[index].Result);
            switch (_playerRecord[index].Result)
            {
                case 0:
                    textResult.text = "Win";
                    textResult.color = Color.red;
                    break;
                case 1:
                    textResult.text = "Draw";
                    textResult.color = Color.white;
                    break;
                case 2:
                    textResult.text = "Lose";
                    textResult.color = Color.blue;
                    break;
            }

            textName.text = _playerRecord[index].AttackName;
        }

        public override void Release()
        {
            _eventService.Remove<NewRecordNotifitionEvent>(GameEvents.ArenaEvent.NewRecordNotifition, OnNewRecords);
        }
    }
}