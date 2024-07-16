using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using cfg.avatar;
using Cysharp.Threading.Tasks;
using FairyGUI;
using GameEngine;
using GameEngine.Config;
using GameEngine.Event;
using GameEngine.Network;
using GameEngine.Resource;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Player;
using MetaVirus.Logic.Protocols.User;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.Common;
using MetaVirus.Net.Messages.User;
using UnityEngine;
using UnityEngine.Events;
using static MetaVirus.Logic.Service.GameDataService;

namespace MetaVirus.Logic.UI.Windows
{
    [UIWindow("ui_create_actor")]
    public class UICreateActor : BaseUIWindow
    {
        private GameDataService _gameDataService;
        private NetworkService _networkService;

        private readonly List<SelectButton> _partSelectBtns = new();

        private GameObject _modelAnchor;
        private const string PrefabAddress = "actor-player/PlayerTemplate.prefab";

        private CharacterTemplate _characterTemplate;
        private Controller _nameController;

        private Vector2 _turnPadTouchPoint;
        private Vector2 _turnPadMovePoint;
        private float _turnDelta;
        private bool _turning = false;
        private GObject _txtMessge;
        private GButton _btnCreate;
        private GTextInput _inputName;

        public UnityAction<int> OnActorCreated;

        public override bool IsFullscreenWindow => false;

        protected override GComponent MakeContent()
        {
            GameFramework.Inst.StartCoroutine(LoadModel());
            var comp = UIPackage.CreateObject("Common", "CreateActorUI").asCom;
            GameFramework.GetService<EventService>().Emit(GameEvents.GameEvent.CreateActorOpened);
            return comp;
        }

        private IEnumerator LoadModel()
        {
            _modelAnchor = GameObject.Find("ModelAnchor");

            // var task = Addressables.InstantiateAsync(PrefabAddress);
            // yield return task;

            var task = GameFramework.GetService<YooAssetsService>().InstanceAsync(PrefabAddress);
            GameObject result = null;
            yield return task.ToCoroutine(r => result = r);

            result.SetActive(true);
            _characterTemplate = result.GetComponent<CharacterTemplate>();
            _characterTemplate.gameObject.SetLayerAll(LayerMask.NameToLayer("UI"));

            _characterTemplate.transform.SetParent(_modelAnchor.transform, false);
            _characterTemplate.transform.localPosition = Vector3.zero;
            _characterTemplate.transform.localRotation = Quaternion.identity;

            RefreshAvatar();
        }

        private void SetErrorMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                _btnCreate.enabled = !string.IsNullOrEmpty(_inputName.text);
                _nameController.selectedIndex = 0;
                _txtMessge.text = "";
            }
            else
            {
                _btnCreate.enabled = false;
                _nameController.selectedIndex = 1;
                _txtMessge.text = message;
            }
        }

        private void RequestCreateActor(string actorName)
        {
            if (string.IsNullOrEmpty(actorName)) return;

            var avatarSetting = _characterTemplate.AvatarToLong();
            UIService.SetButtonLoading(_btnCreate, true);

            //发送创建角色请求
            var req = new CreateActorRequestCs(
                new CreateActorRequestCsPb()
                {
                    ActorName = actorName,
                    AvatarSetting = (long)avatarSetting,
                }
            );

            _networkService.SendPacketTo(req, GameConfig.Inst.WorldServerId, resp =>
            {
                UIService.SetButtonLoading(_btnCreate, false);
                if (resp.IsTimeout)
                {
                    UIDialog.ShowTimeoutMessage((btn, s, dialog) => dialog.Hide());
                }
                else
                {
                    var pb = resp.GetPacket<CreateActorResponseSc>().ProtoBufMsg;
                    if (pb.ResultCode == 0)
                    {
                        GameFramework.GetService<EventService>()
                            .Emit(GameEvents.GameEvent.CreateActorDone);
                        //成功，进入游戏
                        OnActorCreated?.Invoke(pb.PlayerId);
                    }
                    else
                    {
                        //UIDialog.ShowErrorMessage(pb.ResultCode, (btn, s, dialog) => { dialog.Hide(); });
                        SetErrorMessage(LM(pb.ResultCode));
                    }
                }
            });
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _gameDataService = GameFramework.GetService<GameDataService>();
            _networkService = GameFramework.GetService<NetworkService>();

            _nameController = content.GetController("name");
            _txtMessge = content.GetChild("txtMessage");

            var selBody = content.GetChild("selBody").asCom;
            var selBack = content.GetChild("selBack").asCom;
            var selHead = content.GetChild("selHead").asCom;
            var selHair = content.GetChild("selHair").asCom;
            var selHat = content.GetChild("selHat").asCom;
            var selEyebrow = content.GetChild("selEyebrow").asCom;
            var selEye = content.GetChild("selEye").asCom;
            var selMouth = content.GetChild("selMouth").asCom;
            var selHeadwear = content.GetChild("selHeadwear").asCom;
            var selMainHand = content.GetChild("selMainHand").asCom;
            var selOffHand = content.GetChild("selOffHand").asCom;

            _inputName = content.GetChild("inputName").asTextInput;
            _inputName.onChanged.Set(() =>
            {
                SetErrorMessage("");
                var t = _inputName.text.ToLower();
                if (t.Length == 1)
                {
                    _inputName.text = t.ToUpper();
                    var match = Regex.IsMatch(_inputName.text, "[a-zA-Z]");
                    if (!match)
                    {
                        _inputName.text = "";
                    }
                }
                else if (t.Length > 1)
                {
                    t = t[..1].ToUpper() + t[1..];
                    _inputName.text = t;
                }

                _btnCreate.enabled = !string.IsNullOrEmpty(_inputName.text);
            });

            var btnRandom = content.GetChild("btnRandom").asButton;
            _btnCreate = content.GetChild("btnCreate").asButton;
            _btnCreate.onClick.Set(() => RequestCreateActor(_inputName.text));
            _btnCreate.enabled = false;

            var turnPad = content.GetChild("turnPad").asButton;
            turnPad.onTouchBegin.Set(context =>
            {
                _turning = true;
                _turnDelta = 0;
                _turnPadMovePoint = _turnPadTouchPoint = context.inputEvent.position;
            });

            turnPad.onTouchMove.Set(context =>
            {
                _turnPadMovePoint = context.inputEvent.position;
                _turnDelta = _turnPadTouchPoint.x - _turnPadMovePoint.x;
                _turnPadTouchPoint = context.inputEvent.position;
            });

            turnPad.onTouchEnd.Set(context =>
            {
                _turning = false;
                _turnDelta = 0;
            });

            var txtHide = LT("common.text.hide");

            var bodyItems = (
                from d in _gameDataService.gameTable.AvatarBodyDatas.DataList
                where d.CreateActor
                select new SelectButtonData(d.Id, d, d.Id == 0 ? txtHide : $"{d.Id:00}")
            ).ToList();
            _partSelectBtns.Add(new SelectButton(selBody, bodyItems, OnBodyChanged));


            var backItems = (
                from d in _gameDataService.gameTable.AvatarBackDatas.DataList
                where d.CreateActor
                select new SelectButtonData(d.Id, d, d.Id == 0 ? txtHide : $"{d.Id:00}")
            ).ToList();
            _partSelectBtns.Add(new SelectButton(selBack, backItems, OnBackChanged));


            var headItems = (
                from d in _gameDataService.gameTable.AvatarHeadDatas.DataList
                where d.CreateActor
                select new SelectButtonData(d.Id, d, d.Id == 0 ? txtHide : $"{d.Id:00}")
            ).ToList();
            _partSelectBtns.Add(new SelectButton(selHead, headItems, OnHeadChanged));


            var hairItems = (from d in _gameDataService.gameTable.AvatarSenseDatas.DataList
                where d.Part == HeadWearType.Hair && d.CreateActor
                select new SelectButtonData(d.PartId, d, d.PartId == 0 ? txtHide : $"{d.PartId:00}")).ToList();
            _partSelectBtns.Add(new SelectButton(selHair, hairItems, OnHairChanged));

            var hatItems = (from d in _gameDataService.gameTable.AvatarSenseDatas.DataList
                where d.Part == HeadWearType.Hat && d.CreateActor
                select new SelectButtonData(d.PartId, d, d.PartId == 0 ? txtHide : $"{d.PartId:00}")).ToList();
            _partSelectBtns.Add(new SelectButton(selHat, hatItems, OnHatChanged));


            var eyebrowItems = (from d in _gameDataService.gameTable.AvatarSenseDatas.DataList
                where d.Part == HeadWearType.Eyebrow && d.CreateActor
                select new SelectButtonData(d.PartId, d, d.PartId == 0 ? txtHide : $"{d.PartId:00}")).ToList();
            _partSelectBtns.Add(new SelectButton(selEyebrow, eyebrowItems, OnEyebrowChanged));

            var eyeItems = (from d in _gameDataService.gameTable.AvatarSenseDatas.DataList
                where d.Part == HeadWearType.Eye && d.CreateActor
                select new SelectButtonData(d.PartId, d, d.PartId == 0 ? txtHide : $"{d.PartId:00}")).ToList();
            _partSelectBtns.Add(new SelectButton(selEye, eyeItems, OnEyeChanged));

            var mouthItems = (from d in _gameDataService.gameTable.AvatarSenseDatas.DataList
                where d.Part == HeadWearType.Mouth && d.CreateActor
                select new SelectButtonData(d.PartId, d, d.PartId == 0 ? txtHide : $"{d.PartId:00}")).ToList();
            _partSelectBtns.Add(new SelectButton(selMouth, mouthItems, OnMouthChanged));

            var headwearItems = (from d in _gameDataService.gameTable.AvatarSenseDatas.DataList
                where d.Part == HeadWearType.Headwear && d.CreateActor
                select new SelectButtonData(d.PartId, d, d.PartId == 0 ? txtHide : $"{d.PartId:00}")).ToList();
            _partSelectBtns.Add(new SelectButton(selHeadwear, headwearItems, OnHeadwearChanged));

            var mainHand = (from d in _gameDataService.gameTable.AvatarWeaponDatas.DataList
                where d.RightHand && d.CreateActor
                select new SelectButtonData(d.Id, d, d.Id == 0 ? txtHide : $"{d.Id:00}")).ToList();
            _partSelectBtns.Add(new SelectButton(selMainHand, mainHand, OnMainHandChanged));

            var offHand = (from d in _gameDataService.gameTable.AvatarWeaponDatas.DataList
                where d.LeftHand && d.TwoHands == false && d.CreateActor
                select new SelectButtonData(d.Id, d, d.Id == 0 ? txtHide : $"{d.Id:00}")).ToList();
            _partSelectBtns.Add(new SelectButton(selOffHand, offHand, OnOffHandChanged));

            RefreshAvatar();
        }

        private void RefreshAvatar()
        {
            if (_characterTemplate == null) return;
            _characterTemplate.RebuildAvatar();

            //主手武器选择影响副手武器选择的可用性
            _partSelectBtns[10].SelectButtonComp.enabled =
                !(_characterTemplate.MainHand.TwoHands || _characterTemplate.MainHand.Id == 0);

            //头部选择影响所有头部饰品选择的可用性
            for (var i = 3; i <= 8; i++)
            {
                _partSelectBtns[i].SelectButtonComp.enabled = _characterTemplate.HeadData.AllowHeadWear;
            }

            //头发和帽子的选择影响眉毛选择的可用性
            //没有选择头发，或者选择了帽子，都需要设置眉毛
            if (_partSelectBtns[5].SelectButtonComp.enabled)
            {
                _partSelectBtns[5].SelectButtonComp.enabled = _characterTemplate.HairData.PartId == 0 ||
                                                              _characterTemplate.HatData.PartId != 0;
            }

            //帽子的选择影响头发选择的可用性
            if (_partSelectBtns[3].SelectButtonComp.enabled)
            {
                _partSelectBtns[3].SelectButtonComp.enabled = _characterTemplate.HatData.PartId == 0;
            }
        }

        private void OnBodyChanged(SelectButtonData sbd)
        {
            _characterTemplate.body = sbd.DataId;
            RefreshAvatar();
        }

        private void OnBackChanged(SelectButtonData sbd)
        {
            _characterTemplate.back = sbd.DataId;
            RefreshAvatar();
        }

        private void OnHeadChanged(SelectButtonData sbd)
        {
            _characterTemplate.head = sbd.DataId;
            RefreshAvatar();
        }

        private void OnHairChanged(SelectButtonData sbd)
        {
            _characterTemplate.hair = sbd.DataId;
            RefreshAvatar();
        }


        private void OnHatChanged(SelectButtonData sbd)
        {
            _characterTemplate.hat = sbd.DataId;
            RefreshAvatar();
        }

        private void OnEyebrowChanged(SelectButtonData sbd)
        {
            _characterTemplate.eyebrow = sbd.DataId;
            RefreshAvatar();
        }

        private void OnEyeChanged(SelectButtonData sbd)
        {
            _characterTemplate.eye = sbd.DataId;
            RefreshAvatar();
        }

        private void OnMouthChanged(SelectButtonData sbd)
        {
            _characterTemplate.mouth = sbd.DataId;
            RefreshAvatar();
        }

        private void OnHeadwearChanged(SelectButtonData sbd)
        {
            _characterTemplate.headwear = sbd.DataId;
            RefreshAvatar();
        }

        private void OnMainHandChanged(SelectButtonData sbd)
        {
            _characterTemplate.rightHand = sbd.DataId;
            RefreshAvatar();
        }

        private void OnOffHandChanged(SelectButtonData sbd)
        {
            _characterTemplate.leftHand = sbd.DataId;
            RefreshAvatar();
        }

        public override void Release()
        {
            if (_characterTemplate != null)
            {
                // Addressables.ReleaseInstance(_characterTemplate.gameObject);
                GameFramework.GetService<YooAssetsService>().ReleaseInstance(_characterTemplate.gameObject);
            }
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();
            if (_turning)
            {
                var euler = _characterTemplate.transform.localEulerAngles;
                euler.y += _turnDelta / 4;
                _characterTemplate.transform.localEulerAngles = euler;
            }
        }
    }
}