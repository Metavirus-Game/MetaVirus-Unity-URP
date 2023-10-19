using System.Collections;
using System.Collections.Generic;
using cfg.common;
using FairyGUI;
using GameEngine;
using MetaVirus.Logic.Service;
using MetaVirus.ResExplorer.Resource;
using UnityEngine;

namespace MetaVirus.ResExplorer.UI
{
    public class MonsterEditorComponent
    {
        private GameDataService _gameDataService;
        private ExplorerResourceService _explorerResourceService;

        private GComponent _editorComp;
        private NpcResourceData _editMonster;
        private ModelLoader _modelLoader;
        private Image _modelImage;
        private bool _turning;
        private float _turnDelta;
        private Vector2 _turnPadMovePoint;
        private Vector2 _turnPadTouchPoint;

        private readonly List<string> _resPath = new();
        private MonsterAniList _monsterAniList;

        private Controller _loadingCtrl;
        private GSlider _sliderZoom;

        private bool _monsterLoading = false;

        public NpcResourceData EditMonster
        {
            get => _editMonster;
            set
            {
                if (_editMonster == value) return;
                _editMonster = value;
                OnMonsterChanged();
            }
        }


        public MonsterEditorComponent(GComponent editorComp)
        {
            _editorComp = editorComp;
            _modelLoader = Object.FindObjectOfType<ModelLoader>();
            _gameDataService = GameFramework.GetService<GameDataService>();
            _explorerResourceService = GameFramework.GetService<ExplorerResourceService>();

            var turnPad = editorComp.GetChild("turnPad").asButton;
            _loadingCtrl = editorComp.GetController("loading");
            _sliderZoom = editorComp.GetChild("sliderZoom").asSlider;

            var aniList = editorComp.GetChild("listAniSetting").asList;
            _monsterAniList = new MonsterAniList(aniList)
            {
                OnAniSelected = (name, clip) =>
                {
                    if (_monsterLoading || clip == null) return;
                    _modelLoader.SetCurrentAniClip(clip);
                }
            };

            _sliderZoom.onChanged.Set(context => { _modelLoader.ZoomCamera((float)_sliderZoom.value); });

            var graph = editorComp.GetChildByPath("n5.modelLoader").asGraph;
            _modelImage = new Image
            {
                texture = new NTexture(_modelLoader.modelTexture),
                blendMode = BlendMode.Normal
            };

            BindRotate(turnPad);

            graph.SetNativeObject(_modelImage);
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

                if (!_turning || _modelLoader.ModelLoaded == null) return;

                var euler = _modelLoader.ModelLoaded.transform.localEulerAngles;
                euler.y += _turnDelta / 4;
                _modelLoader.ModelLoaded.transform.localEulerAngles = euler;
            });

            turnPad.onTouchEnd.Set(context =>
            {
                _turning = false;
                _turnDelta = 0;
            });
        }

        private void OnMonsterChanged()
        {
            _monsterLoading = true;
            _loadingCtrl.SetSelectedIndex(1);
            _monsterAniList.ClearList();
            _modelLoader.SetResourceData(_editMonster, d =>
            {
                _monsterAniList.SetAniData(_modelLoader.AniOverrideSetting, _modelLoader.AnimationClips);
                _monsterLoading = false;
                _loadingCtrl.SetSelectedIndex(0);
                var setting = _monsterAniList.AniSettings[0];
                _modelLoader.SetCurrentAniClip(setting.Clip);
            });
        }
    }
}