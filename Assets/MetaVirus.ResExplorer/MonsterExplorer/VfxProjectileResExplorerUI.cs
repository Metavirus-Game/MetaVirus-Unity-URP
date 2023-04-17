using System.Collections.Generic;
using System.Linq;
using cfg.battle;
using cfg.common;
using cfg.skill;
using FairyGUI;
using GameEngine;
using GameEngine.Event;
using MetaVirus.Logic.Service;
using MetaVirus.ResExplorer.UI;
using UnityEngine;

namespace MetaVirus.ResExplorer.MonsterExplorer
{
    public class VfxProjectileResExplorerUI
    {
        private GComponent _comp;
        private readonly GameDataService _gameDataService;
        private readonly EventService _eventService;
        private List<ProjectileData> _projectileDatas;
        private BattleArea _battleArea;
        private GList _lstData;
        private GComboBox _cmbMuzzle;
        private GComboBox _cmbProjectile;
        private GComboBox _cmbHit;
        private GComboBox _cmbType;
        private GTextField _txtTypeDesc;

        private bool _turning;
        private float _turnDelta;
        private Vector2 _turnPadMovePoint;
        private Vector2 _turnPadTouchPoint;

        private ProjectileData _currProjectileData;

        private BattleSimulator _battleSimulator;

        private static Dictionary<ProjectileType, string> _typeDescMap = new()
        {
            { ProjectileType.Single_Bullet, "单体特效，从施法者位置发射向目标位置，多个目标会发射多个子弹" },
            { ProjectileType.Single_Target, "单体特效，直接在目标位置播放的特效，多个目标会播放多个特效" },
            { ProjectileType.Single_Summon, "单体特效，从施法者头顶位置发射向目标，到达后造成伤害，多个目标会依次发射召唤物" },
            { ProjectileType.Single_Bomb, "单体特效，从施法者位置发射向目标位置，多个目标会发射多个炸弹，炸弹轨迹为抛物线" },
            { ProjectileType.Multi_Target, "群体特效，直接在目标阵型中心位置播放特效，多个目标持续受到伤害" },
            { ProjectileType.Multi_Summon, "群体特效，从施法者头顶位置发射向目标阵型中心位，到达后产生爆炸，此处设定的命中特效为爆炸特效，各单位的命中特效使用 技能中设定的 命中特效" },
            { ProjectileType.Multi_Bomb, "群体特效，从源位置发射向目标阵型中心位，到达后产生爆炸，此处设定的命中特效为爆炸特效，各单位的命中特效使用 技能中设定的 命中特效，炸弹轨迹为抛物线" },
            { ProjectileType.Shockwave, "群体特效，一般用于近战群体攻击，从施法者脚下位置发出，面向目标中心位置施放特效" },
            { ProjectileType.Across, "从原位置向目标中心位置发射，经过目标时产生击中效果，投射物最终会飞出屏幕" }
        };

        private static Dictionary<ProjectileType, string> _typeNameMap = new()
        {
            { ProjectileType.Single_Bullet, "单体-子弹" },
            { ProjectileType.Single_Target, "单体-目标特效" },
            { ProjectileType.Single_Summon, "单体-头顶召唤" },
            { ProjectileType.Single_Bomb, "单体-炸弹" },
            { ProjectileType.Multi_Target, "群体-目标特效" },
            { ProjectileType.Multi_Summon, "群体-头顶召唤" },
            { ProjectileType.Multi_Bomb, "群体-炸弹" },
            { ProjectileType.Shockwave, "群体-震荡波" },
            { ProjectileType.Across, "群体-击穿" }
        };

        private Image _modelImage;

        public VfxProjectileResExplorerUI(GComponent vfxProjectileResExplorerComp)
        {
            _comp = vfxProjectileResExplorerComp;
            _gameDataService = GameFramework.GetService<GameDataService>();
            _battleArea = Object.FindObjectOfType<BattleArea>();
            _eventService = GameFramework.GetService<EventService>();
            _battleSimulator = BattleSimulator.Inst;
            Load();
        }

        private void OnProjectileDataChanged()
        {
            _cmbMuzzle.value = _currProjectileData.MuzzleVfx.ToString();
            _cmbProjectile.value = _currProjectileData.ProjectileVfx.ToString();
            _cmbHit.value = _currProjectileData.HitVfx.ToString();
            _cmbType.value = ((int)_currProjectileData.Type).ToString();
            _txtTypeDesc.text = _typeDescMap[_currProjectileData.Type];

            _battleSimulator.DoProjectileAttack(_currProjectileData, () => { });
        }

        private void InitCmbWithVfxType(GComboBox cmb, VfxType type)
        {
            var list = _gameDataService.gameTable.VFXDatas.DataList.Where(v => v.Type == type).ToList();
            cmb.items = list.Select(m => m.Id + "  " + m.Name).ToArray();
            cmb.values = list.Select(m => m.Id.ToString()).ToArray();
        }

        private void OnResTypeChanged(int idx)
        {
            if (idx == 2)
            {
                _battleArea.SwitchToFormationTarget();
            }

            _battleSimulator.SwitchAllEntities(idx == 2);
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

        private void Load()
        {
            _eventService.On<int>(ExplorerUIService.EventOnResTypeChanged, OnResTypeChanged);

            _projectileDatas = _gameDataService.gameTable.ProjectileDatas.DataList.Where(p => p.Id != 0).ToList();
            _lstData = _comp.GetChild("listData").asList;

            _cmbMuzzle = _comp.GetChild("cmbMuzzle").asComboBox;
            InitCmbWithVfxType(_cmbMuzzle, VfxType.MuzzleFlash);

            _cmbProjectile = _comp.GetChild("cmbProjectile").asComboBox;
            InitCmbWithVfxType(_cmbProjectile, VfxType.ProjectileVfx);

            _cmbHit = _comp.GetChild("cmbHit").asComboBox;
            InitCmbWithVfxType(_cmbHit, VfxType.HitVfx);

            var turnPad = _comp.GetChild("turnPad").asButton;
            BindRotate(turnPad);

            var graph = _comp.GetChildByPath("loader.modelLoader").asGraph;

            _cmbType = _comp.GetChild("cmbVfxType").asComboBox;
            _cmbType.items = _typeNameMap.Values.ToArray();
            _cmbType.values = _typeNameMap.Keys.Select(t => ((int)t).ToString()).ToArray();

            _cmbType.onChanged.Set(() =>
            {
                var key = (ProjectileType)int.Parse(_cmbType.value);
                _txtTypeDesc.text = _typeDescMap[key];
            });


            _txtTypeDesc = _comp.GetChild("txtVfxTypeDesc").asTextField;

            _lstData.itemRenderer = RenderListData_Monster;
            _lstData.numItems = _projectileDatas.Count;
            _lstData.onClickItem.Set(OnListItemClicked);

            _modelImage = new Image
            {
                texture = new NTexture(_battleArea.texture),
                blendMode = BlendMode.Normal
            };

            graph.SetNativeObject(_modelImage);
        }

        private void OnListItemClicked(EventContext context)
        {
            var obj = (GObject)context.data;
            var idx = _lstData.GetChildIndex(obj);

            var data = _projectileDatas[idx];
            _currProjectileData = data;
            OnProjectileDataChanged();
        }

        void RenderListData_Monster(int index, GObject obj)
        {
            var btnData = obj.asButton;
            var data = _projectileDatas[index];
            var txtId = btnData.GetChild("txtId");
            txtId.text = data.Id.ToString();
            btnData.title = data.Name;

            var bgCtrl = btnData.GetController("bg");
            bgCtrl.SetSelectedIndex(index % 2);
        }
    }
}