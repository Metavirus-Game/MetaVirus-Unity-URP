using System.Collections;
using System.Collections.Generic;
using AmazingAssets.TerrainToMesh;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Utils;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Logic.Service.Arena;
using MetaVirus.Logic.Service.Arena.data;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.MonsterPanel.Formation;
using UnityEngine;
using Constants = MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.UI.Windows
{
    public class UIPreparartion : BaseUIWindow
    {
        private DataNodeService _dataNodeService;
        private ArenaService _arenaService;
        private PlayerService _playerService;
        private int _currOpponentId;
        private GComponent _framePlayer;
        private GComponent _frameOpponent;
        private MonsterFormationComp _monsterFormationComp;
        private int[] _formationInfo = { 2, 3, 0 };
        private PlayerParty[] _playerParties;
        private ArenaFormationDetail _arenaFormationDetail;
        private GList _listCast;
        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "ArenaPreparationUI").asCom;
            return comp;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _arenaService = GameFramework.GetService<ArenaService>();
            _playerService = GameFramework.GetService<PlayerService>();
            _currOpponentId = _dataNodeService.GetDataAndClear<int>(Constants.DataKeys.UIArenaMatchingOpponentData);
            Debug.Log(_currOpponentId);
            // Get opponent cast
            GameFramework.Inst.StartCoroutine(GetPlayerArenaFormation());
            _frameOpponent = content.GetChildByPath("comp_opponent.frame_opponent").asCom;
            
            // Get player cast
            // GameFramework.Inst.StartCoroutine(GetAvailableParties());
            _playerParties = _playerService.GetAvailableParties();
            Debug.Log(_playerParties);
            _framePlayer = content.GetChildByPath("comp_player.frame_player").asCom;
            _listCast = content.GetChildByPath("comp_player.list.cast").asList;
        }

        private IEnumerator GetPlayerArenaFormation()
        {
            var task = _arenaService.GetPlayerArenaFormation(1, _currOpponentId);
            yield return task.AsCoroution();
            if (task.Result.IsTimeout)
            {
                UIDialog.ShowTimeoutMessage();
            }else if (task.Result.IsError)
            {
                UIDialog.ShowErrorMessage(task.Result.MessageCode);
            }
            else
            {
                _arenaFormationDetail = task.Result.Result;
                var row = new MonsterFormationComp(_frameOpponent, _formationInfo, true);
                _monsterFormationComp = row;
                for (var slot = 0; slot < _arenaFormationDetail.Count; slot++)
                {
                    var petData = _arenaFormationDetail.GetSlot(slot);
                    if (petData!=null)
                    {
                        _monsterFormationComp.SetSlotPetData(slot, petData);
                    }
                }
            }
        }

        // private IEnumerator GetAvailableParties()
        // {
        //     var task = _playerService.GetAvailableParties();
        //     yield return task.
        // }
    }
}