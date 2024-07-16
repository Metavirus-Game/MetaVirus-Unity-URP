using System;
using System.Collections;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Event;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Arena.data;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.MonsterPanel.Formation;

namespace MetaVirus.Logic.UI.Windows
{
    public class UIPreparation : BaseUIWindow
    {
        private DataNodeService _dataNodeService;
        private ArenaService _arenaService;
        private EventService _eventService;
        private PlayerService _playerService;
        private int _currOpponentId;

        /// <summary>
        /// 玩家当前选择的partyId
        /// </summary>
        private int _currCastId;

        private GComponent _framePlayer;
        private GComponent _frameOpponent;
        private GTextField _textOpponentName;
        private GTextField _textOpponentLevel;
        private GTextField _textPlayerName;
        private GTextField _textPlayerLevel;
        private MonsterFormationComp _monsterFormationCompOpponent;
        private MonsterFormationComp _monsterFormationCompPlayer;
        private readonly int[] _formationInfo = { 2, 3, 0 };
        private PlayerParty[] _playerParties;
        private ArenaFormationDetail _arenaFormationDetail;
        private GList _listCast;
        private GButton _btnReady;

        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaPreparationUI").asCom;

            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            var playerInfo = PlayerEntity.Current.PlayerInfo;

            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _arenaService = GameFramework.GetService<ArenaService>();
            _playerService = GameFramework.GetService<PlayerService>();
            _eventService = GameFramework.GetService<EventService>();
            _currOpponentId = _dataNodeService.GetDataAndClear<int>(Constants.DataKeys.UIArenaMatchingOpponentData);

            //当前阵型ID初始化为玩家当前选择的出战的小队Id
            _currCastId = playerInfo.AttackPartyId;

            //Debug.Log(_currCastId);
            _btnReady = content.GetChild("button_ready").asButton;
            // Render opponent info
            GameFramework.Inst.StartCoroutine(GetPlayerArenaFormation());
            _frameOpponent = content.GetChild("frame_opponent").asCom;
            _textOpponentName = content.GetChild("text_oppoName").asTextField;
            _textOpponentLevel = content.GetChild("text_oppoLV").asTextField;
            GameFramework.Inst.StartCoroutine(GetOpponentData());
            // _textOpponentLevel.text = Convert.ToString(_arenaService.GetPlayerArenaData(1, _currOpponentId));
            // Render player info
            _textPlayerLevel = content.GetChild("text_playerLV").asTextField;
            _textPlayerName = content.GetChild("text_playerName").asTextField;
            _playerParties = _playerService.GetAvailableParties();
            _framePlayer = content.GetChild("frame_player").asCom;
            _textPlayerLevel.text = "Lv " + Convert.ToString(_playerService.CurrentPlayerInfo.Level);
            _textPlayerName.text = _playerService.CurrentPlayerInfo.Name;
            _listCast = content.GetChild("list_cast").asList;
            _listCast.itemRenderer = RenderCastList;
            _listCast.numItems = _playerParties.Length;
            _listCast.onClickItem.Add(OnCastTabClicked);
            _listCast.selectedIndex = _currCastId;

            _monsterFormationCompPlayer = new MonsterFormationComp(_framePlayer, _formationInfo, enableCheck: false);

            _btnReady.onClick.Set(() => { GameFramework.Inst.StartCoroutine(GotoMatch()); });

            RenderPlayerCast(_currCastId);
        }

        private IEnumerator GotoMatch()
        {
            //弹出loading框
            var wait = UIWaitingWindow.ShowWaiting("Loading...");

            var task = _arenaService.RunMatchBattle(1, _currCastId, _currOpponentId);
            yield return task.AsCoroution();

            wait.Hide();
            var result = task.Result;
            if (result.IsTimeout)
            {
                UIDialog.ShowTimeoutMessage();
            }
            else if (result.IsError)
            {
                UIDialog.ShowErrorMessage(result.MessageCode);
            }
            else
            {
                var br = result.Result;
                _eventService.Emit(GameEvents.ArenaEvent.ArenaMatch, br.Result);
                BattleService.EnterArenaMatch(br);
            }
        }

        private void RenderPlayerCast(int index)
        {
            //_monsterFormationCompPlayer = row;
            var curCast = _playerParties[index];
            for (var i = 0; i < curCast.SlotCount; i++)
            {
                var petId = curCast.Slots[i];

                var petData = _playerService.GetPetData(petId);
                _monsterFormationCompPlayer.SetSlotPetData(i, petData);
            }

            _monsterFormationCompPlayer.OnSlotClickedAction = slot =>
            {
                var petData = _playerService.GetPetData(_playerParties[index].Slots[slot]);
                if (petData == null) return;
                _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailDataList,
                    _playerService.GetPetListProvider());
                _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailData, petData);
                GameFramework.GetService<UIService>().OpenWindow<UIMonsterDetail>();
            };
        }

        private void OnCastTabClicked(EventContext context)
        {
            var obj = (GObject)context.data;

            var idx = _listCast.GetChildIndex(obj);
            //_dataNodeService.SetData(Constants.DataKeys.UIArenaPreparationCastId, idx);
            //修改为当前选择的partyId
            //PartyId == Index
            _currCastId = idx;
            RenderPlayerCast(idx);
        }

        private void RenderCastList(int index, GObject obj)
        {
            var comp = obj.asCom;
            var tabCastName = comp.GetChild("text_castName").asTextField;
            tabCastName.text = _playerParties[index].Name;
        }

        private IEnumerator GetOpponentData()
        {
            var task = _arenaService.GetPlayerArenaData(1, _currOpponentId);
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
                _textOpponentLevel.text = "Lv " + Convert.ToString(data.Result.PlayerLevel);
                _textOpponentName.text = data.Result.PlayerName;
            }
        }

        private IEnumerator GetPlayerArenaFormation()
        {
            var task = _arenaService.GetPlayerArenaFormation(1, _currOpponentId);
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
                _arenaFormationDetail = task.Result.Result;
                _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailDataList, _arenaFormationDetail);
                var row = new MonsterFormationComp(_frameOpponent, _formationInfo, true, false);
                _monsterFormationCompOpponent = row;
                row.OnSlotClickedAction = slot =>
                {
                    var monsterData = _arenaFormationDetail.GetSlot(slot);
                    if (monsterData == null) return;
                    _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailDataList, _arenaFormationDetail);
                    _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailData, monsterData);
                    GameFramework.GetService<UIService>().OpenWindow<UIMonsterDetail>();
                };
                for (var slot = 0; slot < _arenaFormationDetail.Count; slot++)
                {
                    var petData = _arenaFormationDetail.GetSlot(slot);
                    if (petData != null)
                    {
                        _monsterFormationCompOpponent.SetSlotPetData(slot, petData);
                    }
                }
            }
        }
    }
}