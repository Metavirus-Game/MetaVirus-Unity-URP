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
        private int _currCastId;
        private GComponent _framePlayer;
        private GComponent _frameOpponent;
        private MonsterFormationComp _monsterFormationCompOpponent;
        private MonsterFormationComp _monsterFormationCompPlayer;
        private readonly int[] _formationInfo = { 2, 3, 0 };
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
            _currCastId = _dataNodeService.GetData<int>(Constants.DataKeys.UIArenaPreparationCastId);
            Debug.Log(_currCastId);
            // Render opponent cast
            GameFramework.Inst.StartCoroutine(GetPlayerArenaFormation());
            _frameOpponent = content.GetChildByPath("comp_opponent.frame_opponent").asCom;
            
            // Render player cast
            _playerParties = _playerService.GetAvailableParties();
            _framePlayer = content.GetChildByPath("comp_player.frame_player").asCom;
            _listCast = content.GetChildByPath("comp_player.list_cast").asList;
            _listCast.itemRenderer = RenderCastList;
            _listCast.numItems = _playerParties.Length;
            _listCast.onClickItem.Add(OnCastTabClicked);
            _listCast.selectedIndex = _currCastId;
           
            RenderPlayerCast(_currCastId);
           
        }

        private void RenderPlayerCast(int index)
        {   
            var row = new MonsterFormationComp(_framePlayer, _formationInfo, false);
            _monsterFormationCompPlayer = row;
            var curCast = _playerParties[index];
            for (var i = 0; i < curCast.SlotCount; i++)
            {
                var petId = curCast.Slots[i];
                //会返回null吗？这种调用服务的方法是异步的吗？
                var petData = _playerService.GetPetData(petId);
                _monsterFormationCompPlayer.SetSlotPetData(i, petData);
            }
            row.OnSlotClickedAction = slot =>
            {
                _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailDataList, _playerService.GetPetListProvider());
                _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailData,
                    _playerService.GetPetData(_playerParties[index].Slots[slot]));
                GameFramework.GetService<UIService>().OpenWindow<UIMonsterDetail>();
            };
        }

        private void OnCastTabClicked(EventContext context)
        {
            var obj = (GObject)context.data;
            // 如果返回的是-1需要包哪种错误？
            var idx = _listCast.GetChildIndex(obj);
            _dataNodeService.SetData(Constants.DataKeys.UIArenaPreparationCastId, idx);
            RenderPlayerCast(idx);
        }

        private void RenderCastList(int index, GObject obj)
        {
            var comp = obj.asCom;
            var tabCastName = comp.GetChild("text_castName").asTextField;
            tabCastName.text = _playerParties[index].Name;
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
                _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailDataList, _arenaFormationDetail);
                var row = new MonsterFormationComp(_frameOpponent, _formationInfo, true);
                _monsterFormationCompOpponent = row;
                row.OnSlotClickedAction = slot =>
                {
                    _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailDataList, _arenaFormationDetail);
                    // 这里需要处理GetSlot返回的null吗
                    _dataNodeService.SetData(Constants.DataKeys.UIMonsterDetailData,
                        _arenaFormationDetail.GetSlot(slot));
                    GameFramework.GetService<UIService>().OpenWindow<UIMonsterDetail>();
                };
                for (var slot = 0; slot < _arenaFormationDetail.Count; slot++)
                {
                    var petData = _arenaFormationDetail.GetSlot(slot);
                    if (petData!=null)
                    {
                        _monsterFormationCompOpponent.SetSlotPetData(slot, petData);
                    }
                }
            }
        }
    }
}