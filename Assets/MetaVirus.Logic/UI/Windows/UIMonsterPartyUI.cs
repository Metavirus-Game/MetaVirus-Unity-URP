using System;
using System.Linq;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Network;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Protocols.Player;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.MonsterPanel;
using MetaVirus.Logic.UI.Component.MonsterPanel.Formation;
using MetaVirus.Net.Messages.Common;
using MetaVirus.Net.Messages.Player;
using static MetaVirus.Logic.Data.Constants;

namespace MetaVirus.Logic.UI.Windows
{
    [UIWindow("ui_monsters_party")]
    public class UIMonsterPartyUI : BaseUIWindow
    {
        private MonsterListPanel _monsterListPanel;
        private PlayerService _playerService;
        private NetworkService _networkService;
        private DataNodeService _dataNodeService;
        private UIService _uiService;

        private int _currentPartyId = 0;
        private int _battlePartyid = 0;

        private PlayerParty CurrentParty => _playerService.GetPlayerParty(_currentPartyId);

        /// <summary>
        /// 临时的阵型数据信息，第一行2个slot，第二行3个slot，第三行0
        /// </summary>
        private readonly int[] _formationInfo = { 2, 3, 0 };

        private MonsterFormationComp _formationComp = null;

        private GTextField _txtPartyName;
        private GTextInput _inputPartyName;
        private GButton _btnPrev;
        private GButton _btnNext;
        private GButton _btnEditName;
        private GButton _btnRemove;
        private GButton _btnDetail;
        private GButton _switchBattle;

        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "MonsterPartyUI").asCom;
            _monsterListPanel = new MonsterListPanel(comp.GetChild("listMonsters").asList);
            return comp;
        }

        private void UpdateFormationData()
        {
            if (CurrentParty != null)
            {
                for (var slot = 0; slot < CurrentParty.SlotCount; slot++)
                {
                    var petId = CurrentParty[slot];
                    var petData = petId <= 0 ? null : _playerService.GetPetData(petId);
                    _formationComp.SetSlotPetData(slot, petData);
                }

                _txtPartyName.text = (CurrentParty.PartyId + 1) + ". " + CurrentParty.Name;
                _monsterListPanel.SelectedPets = _formationComp.SelectedPets;
                if (_formationComp.CurrSlot.PetData == null)
                {
                    _monsterListPanel.CheckedPets = Array.Empty<int>();
                }
                else
                {
                    _monsterListPanel.CheckedPets = new[] { _formationComp.CurrSlot.PetData.Id };
                }

                _btnDetail.enabled = _btnRemove.enabled = _formationComp.CurrSlot.PetData != null;

                _switchBattle.selected = CurrentParty.PartyId == _battlePartyid;

                _switchBattle.touchable = !_switchBattle.selected && _formationComp.SelectedPets.Length > 0;
                
                var selectedPets = _formationComp.SelectedPets.Length;
                if (selectedPets == 1 && _currentPartyId == _battlePartyid)
                {
                    //出战状态下的小队至少要有一个队员
                    _btnRemove.enabled = false;
                }
                
            }
        }

        /// <summary>
        /// 当前阵型数据是否有变化，如果有变化则提交服务器
        /// </summary>
        private void CheckPartyChanges()
        {
            var editSlots = _formationComp.SlotsData;
            var partySlots = CurrentParty?.Slots;

            if (partySlots != null && !editSlots.SequenceEqual(partySlots))
            {
                //数据有变化，存储并提交给服务器
                CurrentParty.Slots = editSlots;
                SendPartyDataToServer(CurrentParty);
            }
        }

        private void SendPartyDataToServer(PlayerParty party)
        {
            var fd = new PBFormationData()
            {
                FormationId = party.PartyId,
                FormationDataId = party.FormationDataId,
                Name = party.Name,
            };
            fd.Slots.AddRange(party.Slots);

            var pb = new UpdateFormationRequestCsPb()
            {
                Formations = { fd },
                AttackFormation = _battlePartyid,
            };

            _networkService.SendPacketTo(new UpdateFormationRequestCs(pb),
                _playerService.CurrentPlayerInfo.sceneServerId);
        }

        public override void BeforeHiding()
        {
            //窗体关闭之前检查是否需要保存当前阵型
            CheckPartyChanges();
        }

        private void UpdatePartyName()
        {
            var n = _inputPartyName.text;
            if (CurrentParty.Name != n)
            {
                CurrentParty.Name = n;
                SendPartyDataToServer(CurrentParty);

                CurrentParty.Name = _inputPartyName.text;
                _txtPartyName.text = CurrentParty.Name;
            }

            _btnEditName.enabled = _btnNext.enabled = _btnPrev.enabled = true;

            _inputPartyName.visible = false;
            _txtPartyName.visible = true;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _playerService = GameFramework.GetService<PlayerService>();
            _networkService = GameFramework.GetService<NetworkService>();
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _uiService = GameFramework.GetService<UIService>();

            _battlePartyid = _playerService.CurrentPlayerInfo.AttackPartyId;

            _monsterListPanel.LoadData();

            //init controls
            _txtPartyName = content.GetChild("txtPartyName").asTextField;
            _inputPartyName = content.GetChild("inputPartyName").asTextInput;
            _btnPrev = content.GetChild("btnPrev").asButton;
            _btnNext = content.GetChild("btnNext").asButton;
            _btnRemove = content.GetChild("btnRemove").asButton;
            _btnDetail = content.GetChild("btnDetail").asButton;

            _switchBattle = content.GetChild("btnBattleParty").asButton;

            _switchBattle.onClick.Set(() =>
            {
                if (_switchBattle.selected)
                {
                    _switchBattle.touchable = false;
                    _battlePartyid = CurrentParty.PartyId;
                    _playerService.CurrentPlayerInfo.AttackPartyId = _battlePartyid;
                    SendPartyDataToServer(CurrentParty);
                }
            });

            _btnRemove.onClick.Set(() =>
            {
                _formationComp.CurrSlot.PetData = null;
                _monsterListPanel.SelectedPets = _formationComp.SelectedPets;
                _monsterListPanel.CheckedPets = null;
                _btnDetail.enabled = _btnRemove.enabled = false;
            });

            _btnDetail.onClick.Set(() =>
            {
                var petData = _formationComp.CurrSlot.PetData;
                if (petData == null) return;
                _dataNodeService.SetData(DataKeys.UIMonsterDetailData, petData);
                _dataNodeService.SetData(DataKeys.UIMonsterDetailDataList,
                    _playerService.GetPetListProvider());
                _uiService.OpenWindow<UIMonsterDetail>();
            });

            _btnEditName = content.GetChild("btnEditName").asButton;

            _inputPartyName.onSubmit.Set(UpdatePartyName);
            _inputPartyName.onFocusOut.Set(UpdatePartyName);

            _btnEditName.onClick.Set(() =>
            {
                _inputPartyName.RequestFocus();
                _inputPartyName.text = CurrentParty.Name;
                _inputPartyName.visible = true;
                _txtPartyName.visible = false;
                _btnEditName.enabled = _btnNext.enabled = _btnPrev.enabled = false;
            });

            var btnSortBy = content.GetChild("btnSortBy").asButton;
            btnSortBy.onClick.Set(c =>
            {
                _monsterListPanel.SortMethod =
                    btnSortBy.selected ? MonsterSort.ByQuality : MonsterSort.BySpecies;
            });

            var formation = content.GetChild("Formation").asCom;
            var row = new MonsterFormationComp(formation, _formationInfo);
            _formationComp = row;

            _btnNext.onClick.Set(() =>
            {
                CheckPartyChanges();
                _currentPartyId++;
                if (_currentPartyId >= _playerService.PartySize)
                {
                    _currentPartyId = 0;
                }

                UpdateFormationData();
            });

            _btnPrev.onClick.Set(() =>
            {
                CheckPartyChanges();
                _currentPartyId--;
                if (_currentPartyId < 0)
                {
                    _currentPartyId = _playerService.PartySize - 1;
                }

                UpdateFormationData();
            });

            //init formation data
            UpdateFormationData();

            row.OnSlotClickedAction = slot =>
            {
                _btnDetail.enabled = _btnRemove.enabled = row.CurrSlot.PetData != null;

                var selectedPets = _formationComp.SelectedPets.Length;
                if (selectedPets == 1 && _currentPartyId == _battlePartyid)
                {
                    //出战状态下的小队至少要有一个队员
                    _btnRemove.enabled = false;
                }

                _monsterListPanel.CheckedPets =
                    row.CurrSlot.PetData == null ? null : new[] { row.CurrSlot.PetData.Id };
            };

            _monsterListPanel.OnItemSelected = (item, petData) =>
            {
                //var headerComp = item.GetChild("n7").asCom;
                // if (headerComp.GetController("selected").selectedIndex == 1)
                if (item.GetController("label").selectedIndex == 2)
                {
                    if (row.CurrSlot.PetData == petData)
                    {
                        //点击与阵型上当前已选的slot里相同的怪物时取消上阵
                        row.SetSelectedSlotPetData(null);
                        _monsterListPanel.SelectedPets = row.SelectedPets;
                        _monsterListPanel.CheckedPets = null;
                    }
                    else
                    {
                        //点击与阵型上不同的slot里的怪物时，选中对应的阵型slot
                        row.SelectPetData(petData);
                        _monsterListPanel.CheckedPets = new[] { petData.Id };
                    }
                }
                else
                {
                    row.SetSelectedSlotPetData(petData);
                    _monsterListPanel.SelectedPets = row.SelectedPets;
                    _monsterListPanel.CheckedPets =
                        row.CurrSlot.PetData == null ? null : new[] { row.CurrSlot.PetData.Id };
                }

                _btnDetail.enabled = _btnRemove.enabled = row.CurrSlot.PetData != null;
            };
        }
    }
}