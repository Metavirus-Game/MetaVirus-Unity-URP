using System.Collections;
using System.Collections.Generic;
using cfg.battle;
using cfg.common;
using FairyGUI;
using GameEngine;
using GameEngine.Network;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Entities;
using MetaVirus.Logic.Data.Player;
using MetaVirus.Logic.Protocols.Test;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.Player;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.MonsterPanel;
using MetaVirus.Net.Messages.Test;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;

namespace MetaVirus.Logic.UI.Windows
{
    [UIWindow("ui_create_monster")]
    public class UICreateMonsterWindow : BaseUIWindow
    {
        private GameDataService _gameDataService;
        private NetworkService _networkService;
        private PlayerService _playerService;

        private GList _lstTypes;
        private GList _lstMonsters;

        private GRichTextField _monsterTitle;

        private MonsterType _currMonsterType;
        private List<PetData> _currentMonsters;
        private PetData _currentMonster;

        private MonsterAttributePanel _attributePanel;
        private MonsterResistancePanel _resistancePanel;

        private GGraph _modelAnchor;

        private Image _modelImage;
        private UIModelLoader _uiModelLoader;

        private GButton _btnCreate;
        private PlayerEntity _player;

        protected override GComponent MakeContent()
        {
            _gameDataService = GameFramework.GetService<GameDataService>();
            _playerService = GameFramework.GetService<PlayerService>();

            var comp = UIPackage.CreateObject("Common", "CreateMonsterUI").asCom;
            return comp;
        }

        protected override void AddComponentToParent(GComponent parentComp, GComponent content)
        {
            AddCompToParentFull(parentComp, content);
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _player = PlayerEntity.Current;

            _networkService = GameFramework.GetService<NetworkService>();

            var monsterTypes = _gameDataService.gameTable.MonsterTypes.DataList;

            var createMonsterComp = ContentComp;

            _modelAnchor = createMonsterComp.GetChild("modelAnchor").asGraph;

            _monsterTitle = createMonsterComp.GetChild("txtMonsterName").asRichTextField;

            _lstTypes = createMonsterComp.GetChild("listTypes").asList;
            _lstTypes.itemRenderer = RenderMonsterTypeList;
            _lstTypes.numItems = monsterTypes.Count;
            _lstTypes.onClickItem.Add(OnMonsterTypeClicked);
            _lstTypes.selectedIndex = 0;

            _lstMonsters = createMonsterComp.GetChild("listPortraits").asList;
            _lstMonsters.itemRenderer = RenderMonsterGridItem;
            _lstMonsters.onClickItem.Add(OnMonsterClicked);

            var attrList = createMonsterComp.GetChild("attributeList").asList;
            var attrComp = attrList.GetChildAt(0).asCom;
            var resistComp = attrList.GetChildAt(1).asCom;

            _attributePanel = new MonsterAttributePanel(attrComp);
            _resistancePanel = new MonsterResistancePanel(resistComp);

            _btnCreate = ContentComp.GetChild("btnCreate").asButton;

            _btnCreate.onClick.Set(CreateMonster);
            _btnCreate.enabled = false;

            OnMonsterTypeSelected(monsterTypes[0]);
        }

        private void CreateMonster()
        {
            if (_currentMonster != null)
            {
                UIService.SetButtonLoading(_btnCreate, true);
                var pb = new TestCreateMonsterRequestCSPb
                {
                    PetDataId = _currentMonster.Id
                };

                _networkService.SendPacketTo(new TestCreateMonsterRequest(pb), _player.PlayerInfo.sceneServerId,
                    resp =>
                    {
                        UIService.SetButtonLoading(_btnCreate, false);

                        if (resp.IsTimeout)
                        {
                            UIDialog.ShowTimeoutMessage(((btn, s, dialog) => dialog.Hide()));
                        }
                        else
                        {
                            var packet = resp.GetPacket<TestCreateMonsterResponse>();
                            if (packet != null)
                            {
                                _playerService.AddPetData(PlayerPetInfo.FromPbPetData(packet.ProtoBufMsg.PetData));
                                _lstMonsters.numItems = _currentMonsters.Count;
                            }
                        }
                    });
            }
        }

        public override void Release()
        {
            if (_modelImage != null)
            {
                _modelImage.Dispose();
                _modelImage = null;
            }

            if (_uiModelLoader != null)
            {
                Addressables.ReleaseInstance(_uiModelLoader.gameObject);
            }
        }

        private void OnMonsterClicked(EventContext context)
        {
            var obj = (GObject)context.data;
            var idx = _lstMonsters.GetChildIndex(obj);
            var monster = _currentMonsters[idx];
            OnMonsterSelected(monster);
        }

        private void OnMonsterTypeClicked(EventContext context)
        {
            var monsterTypes = _gameDataService.gameTable.MonsterTypes.DataList;

            var obj = (GObject)context.data;
            var idx = _lstTypes.GetChildIndex(obj);
            OnMonsterTypeSelected(monsterTypes[idx]);
        }

        private void OnMonsterTypeSelected(MonsterType type)
        {
            _currMonsterType = type;
            _currentMonsters = _gameDataService.GetPetsByType(type);
            _lstMonsters.numItems = _currentMonsters.Count;
            OnMonstersChanged();
            if (_currentMonsters.Count > 0)
            {
                _lstMonsters.selectedIndex = 0;
                OnMonsterSelected(_currentMonsters[0]);
            }
        }

        private void OnMonsterSelected(PetData pet)
        {
            _currentMonster = pet;
            _attributePanel.PetData = _resistancePanel.PetData = pet;
            _monsterTitle.text = pet.Name;
            _monsterTitle.color = _gameDataService.QualityToColor(pet.Quality);
            GameFramework.Inst.StartCoroutine(LoadMonsterModel());

            var owned = _playerService.GetPetDataBySourceDataId(pet.Id) != null;

            _btnCreate.enabled = !owned;
        }

        private IEnumerator LoadMonsterModel()
        {
            if (_uiModelLoader == null)
            {
                var task = Addressables.InstantiateAsync(Constants.ResAddress.UIModelLoader).Task;
                yield return task.AsCoroution();
                _uiModelLoader = task.Result.GetComponent<UIModelLoader>();
            }

            _uiModelLoader.LoadModel(_currentMonster.ResDataId, texture =>
            {
                var tex = new NTexture(texture);
                _modelImage?.Dispose();

                _modelImage = new Image
                {
                    texture = tex,
                    blendMode = BlendMode.Add
                };

                _modelAnchor.SetNativeObject(_modelImage);
            });

            // if (_modelResObj != null)
            // {
            //     Addressables.ReleaseInstance(_modelResObj);
            //     _modelResObj = null;
            // }
            //
            // var npcResAddress = Constants.ResAddress.NpcRes(_currentMonster.ResDataId_Ref.Id);
            // var task = Addressables.InstantiateAsync(npcResAddress).Task;
            // yield return task.AsCoroution();
            // _modelResObj = task.Result;
            // _modelResObj.SetActive(true);
            // _modelResObj.GetComponent<NavMeshAgent>().enabled = false;
            //
            // _modelResObj.transform.localPosition = new Vector3(50, -80, 1000);
            // _modelResObj.transform.localEulerAngles = new Vector3(0, 180, 0);
            // _modelResObj.transform.localScale = new Vector3(80, 80, 80);
            //
            // if (_modelWrapper == null)
            // {
            //     _modelWrapper = new GoWrapper();
            //     _modelAnchor.SetNativeObject(_modelWrapper);
            // }
            //
            // _modelWrapper.SetWrapTarget(_modelResObj, true);
        }

        private void OnMonstersChanged()
        {
        }

        private void RenderMonsterTypeList(int index, GObject obj)
        {
            var monsterTypes = _gameDataService.gameTable.MonsterTypes.DataList;
            var comp = obj.asCom;

            var txtTitle = comp.GetChild("title").asRichTextField;
            txtTitle.text = monsterTypes[index].Name;
        }

        private void RenderMonsterGridItem(int index, GObject obj)
        {
            var comp = obj.asCom;
            var petData = _currentMonsters[index];

            var headerLoader = comp.GetChild("n16").asCom.GetChild("PortraitLoader").asLoader;
            headerLoader.url = Constants.FairyImageUrl.Header(petData.ResDataId_Ref.Id);

            var owned = _playerService.GetPetDataBySourceDataId(petData.Id) != null;

            var oc = comp.GetController("owned");
            oc.selectedIndex = owned ? 1 : 0;
        }
    }
}