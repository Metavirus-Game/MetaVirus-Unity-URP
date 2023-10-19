using System.Collections;
using System.Collections.Generic;
using cfg.battle;
using cfg.common;
using FairyGUI;
using GameEngine;
using GameEngine.Event;
using GameEngine.Utils;
using MetaVirus.Logic.Service.Vfx;
using MetaVirus.ResExplorer.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace MetaVirus.ResExplorer.MonsterExplorer
{
    public class VfxEditorComponent
    {
        private GComponent _comp;
        private BattleArea _battleArea;
        private Image _modelImage;
        private GTextField _txtVfxName;
        private Controller _loadingCtrl;

        private List<GameObject> _vfxObjs = new();

        private BattleVfxGameService _battleVfxGameService;

        private bool _turning;
        private float _turnDelta;
        private Vector2 _turnPadMovePoint;
        private Vector2 _turnPadTouchPoint;

        public VfxEditorComponent(GComponent component)
        {
            _comp = component;
            Load();
        }

        private VFXData _currVfxData;
        private GButton _btnPlay;
        private GButton _btnShoot;
        private EventService _eventService;

        public VFXData CurrVfxData
        {
            get => _currVfxData;
            set
            {
                _currVfxData = value;
                OnVfxDataChanged();
            }
        }

        private void OnVfxDataChanged()
        {
            GameFramework.Inst.StartCoroutine(LoadVfxRes());
            _btnShoot.visible = _currVfxData.Type == VfxType.ProjectileVfx;
        }

        private void ShootVfx()
        {
            var attack = _battleArea.attackUnit;
            var defence = _battleArea.defenceUnit;
            var pos = defence.GetVfxBindPos(_currVfxData.Id);
            var atkPos = attack.GetVfxBindPos(_currVfxData.Id);
            if (_currVfxData.Type == VfxType.ProjectileVfx)
            {
                var projectileObj = new GameObject("projectile_binder")
                {
                    transform =
                    {
                        position = atkPos.position,
                        forward = pos.position - atkPos.position,
                    }
                };
                var binder = projectileObj.AddComponent<VfxProjectileBinder>();
                binder.bindVfxId = _currVfxData.Id;
            }
            else
            {
                PlayVfx();
            }
        }

        private void PlayVfx()
        {
            if (_vfxObjs.Count > 0)
            {
                foreach (var go in _vfxObjs)
                {
                    _battleVfxGameService.ReleaseVfxInst(go);
                }
            }

            var attack = _battleArea.attackUnit;
            var defence = _battleArea.defenceUnit;
            var pos = defence.GetVfxBindPos(_currVfxData.Id);
            var atkPos = attack.GetVfxBindPos(_currVfxData.Id);

            switch (_currVfxData.Type)
            {
                case VfxType.HitVfx:
                    defence.TakeDamage();
                    attack.TakeDamage();
                    _battleVfxGameService.InstanceVfx(_currVfxData.Id, pos.position, pos.rotation);
                    _battleVfxGameService.InstanceVfx(_currVfxData.Id, atkPos.position, atkPos.rotation);
                    break;
                case VfxType.MuzzleFlash:
                    _battleVfxGameService.InstanceVfx(_currVfxData.Id, pos.position, pos.rotation);
                    _battleVfxGameService.InstanceVfx(_currVfxData.Id, atkPos.position, atkPos.rotation);
                    break;
                case VfxType.ProjectileVfx:
                case VfxType.AttachVfx:
                    var vfx1 = _battleVfxGameService.InstanceVfx(_currVfxData.Id, pos.gameObject, autoDestroy: false);
                    var vfx2 = _battleVfxGameService.InstanceVfx(_currVfxData.Id, atkPos.gameObject,
                        autoDestroy: false);
                    _vfxObjs.Add(vfx1);
                    _vfxObjs.Add(vfx2);
                    break;
            }
        }

        private void BindRotate(GObject turnPad)
        {
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

                if (!_turning || _battleArea.cameraAnchor == null) return;

                var euler = _battleArea.cameraAnchor.transform.localEulerAngles;
                euler.y -= _turnDelta / 4;
                _battleArea.cameraAnchor.transform.localEulerAngles = euler;
            });

            turnPad.onTouchEnd.Set(context =>
            {
                _turning = false;
                _turnDelta = 0;
            });
        }

        private void OnResTypeChanged(int idx)
        {
            if (idx == 1)
            {
                _battleArea.SwitchToSingleTarget();
            }
        }

        private IEnumerator LoadVfxRes()
        {
            var task = _battleVfxGameService.AsyncLoadVfxes(new[] { _currVfxData.Id });
            yield return task.AsCoroution();
        }

        private void Load()
        {
            _eventService = GameFramework.GetService<EventService>();
            _battleVfxGameService = GameFramework.GetService<BattleVfxGameService>();

            _eventService.On<int>(ExplorerUIService.EventOnResTypeChanged, OnResTypeChanged);
            _loadingCtrl = _comp.GetController("loading");
            _battleArea = Object.FindObjectOfType<BattleArea>();

            _battleArea.attackUnit.OnActive();
            _battleArea.defenceUnit.OnActive();
            
            var turnPad = _comp.GetChild("turnPad").asButton;
            BindRotate(turnPad);

            var graph = _comp.GetChildByPath("loader.modelLoader").asGraph;
            _txtVfxName = _comp.GetChild("n5").asTextField;
            _btnPlay = _comp.GetChild("btnPlay").asButton;
            _btnShoot = _comp.GetChild("btnShoot").asButton;

            _btnPlay.onClick.Set(() =>
            {
                if (_currVfxData != null)
                {
                    PlayVfx();
                }
            });
            _btnShoot.onClick.Set(() =>
            {
                if (_currVfxData != null)
                {
                    ShootVfx();
                }
            });

            _modelImage = new Image
            {
                texture = new NTexture(_battleArea.texture),
                blendMode = BlendMode.Normal
            };

            graph.SetNativeObject(_modelImage);
        }
    }
}