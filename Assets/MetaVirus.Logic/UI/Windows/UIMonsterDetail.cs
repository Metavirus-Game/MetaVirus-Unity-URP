using System.Collections;
using cfg.common;
using Cysharp.Threading.Tasks;
using FairyGUI;
using GameEngine;
using GameEngine.DataNode;
using GameEngine.Resource;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Data.Provider;
using MetaVirus.Logic.Service;
using MetaVirus.Logic.Service.UI;
using MetaVirus.Logic.UI.Component.MonsterPanel;
using UnityEngine;

namespace MetaVirus.Logic.UI.Windows
{
    /**
     * 怪物详情界面
     * 需要提供数据
     * UIMonsterDetailData
     * UIMonsterDetailDataList
     */
    [UIWindow("ui_monsters_detail")]
    public class UIMonsterDetail : BaseUIWindow
    {
        private DataNodeService _dataNodeService;
        private GameDataService _gameDataService;

        private IMonsterDataProvider _currPetData;
        private IMonsterListProvider _monsterListProvider;

        private GTextField _txtMonsterName;
        private GTextField _txtMonsterSpecies;
        private GGraph _modelAnchor;
        private GTextField _txtBasicRank;
        private GTextField _txtBasicLevel;
        private GTextField _txtBasicLevelMax;
        private GTextField _txtBasicSpecies;

        private GTextField _txtBasicHpMax;
        private GTextField _txtBasicMpMax;

        private GComponent _baseAttrLeft;
        private GComponent _baseAttrRight;

        private GComponent _calcAttrLeft;
        private GComponent _calcAttrRight;
        private GTextField _txtBasicExp;
        private GTextField _txtBasicExpToNext;

        private Image _modelImage;
        private UIModelLoader _uiModelLoader;
        private GComponent _resisElem;
        private GComponent _resisStatus;

        private MonsterListPanel _monsterListPanel;
        private MonsterSkillListPanel _monsterSkillListPanel;


        private IMonsterDataProvider CurrPetData
        {
            get => _currPetData;
            set
            {
                if (_currPetData != value)
                {
                    _currPetData = value;
                    OnPetDataChanged();
                }
            }
        }

        protected override GComponent MakeContent()
        {
            var comp = UIPackage.CreateObject("Common", "MonsterDetailUI").asCom;
            return comp;
        }

        private void OnPetDataChanged()
        {
            if (_currPetData != null)
            {
                _txtMonsterName.text = _currPetData.Name;
                _txtMonsterSpecies.text = _currPetData.Type.Name;
                GameFramework.Inst.StartCoroutine(LoadMonsterModel());
            }

            RefreshBasicInfo();
            RefreshResisInfo();
            RefreshAbilityInfo();
        }

        private void RefreshBasicInfo()
        {
            _txtBasicRank.text = Constants.QualityToStr(_currPetData.Quality);
            _txtBasicRank.color = _gameDataService.QualityToColor(_currPetData.Quality);
            _txtBasicLevel.text = _currPetData.Level.ToString();
            _txtBasicLevelMax.text = _currPetData.LevelUpTable.LvMax.ToString();
            _txtBasicSpecies.text = _currPetData.Type.Name;
            FillAttributeToPanel(_baseAttrLeft, AttributeId.AttrHp, AttributeId.AttrSpr);
            FillAttributeToPanel(_baseAttrRight, AttributeId.AttrWei, AttributeId.AttrCri);

            FillAttributeToPanel(_calcAttrLeft, AttributeId.CalcHpMax, AttributeId.CalcAtk);
            FillAttributeToPanel(_calcAttrRight, AttributeId.CalcDef, AttributeId.CalcMDef);

            _txtBasicExp.text = _currPetData.CurrExp.ToString();
            _txtBasicExpToNext.text = _currPetData.ExpToNextLevel.ToString();
        }

        private void RefreshResisInfo()
        {
            FillResisToPanel(_resisElem, ResistanceId.ResiFire, ResistanceId.ResiDark);
            FillResisToPanel(_resisStatus, ResistanceId.ResiPoi, ResistanceId.ResiSleep);
        }

        private void RefreshAbilityInfo()
        {
            _monsterSkillListPanel.DataProvider = _currPetData;
        }

        private void FillResisToPanel(GComponent resisPanel, ResistanceId idStart, ResistanceId idEnd)
        {
            for (var resisId = idStart; resisId <= idEnd; resisId++)
            {
                var txtValue = resisPanel.GetChildByPath("resist_" + (int)resisId + ".value").asTextField;
                var resis = _currPetData.GetResistance(resisId);
                txtValue.text = resis.ToString();
                txtValue.color = resis switch
                {
                    > 0 => _gameDataService.BattleColorHpInc(),
                    < 0 => _gameDataService.BattleColorHpDec(),
                    _ => Color.white
                };
            }
        }

        private void FillAttributeToPanel(GComponent attrPanel, AttributeId idStart, AttributeId idEnd)
        {
            for (var attrId = idStart; attrId <= idEnd; attrId++)
            {
                var txtValue = attrPanel.GetChildByPath("attr_" + (int)attrId + ".value").asTextField;
                var txtValueAdd = attrPanel.GetChildByPath("attr_" + (int)attrId + ".addValue")?.asTextField;

                txtValue.text = _currPetData.GetAttribute(attrId).ToString();
                if (txtValueAdd != null)
                {
                    txtValueAdd.text = "+" + _currPetData.GetBaseAttributeGrow(attrId).ToString("F");
                }
            }
        }

        private IEnumerator LoadMonsterModel()
        {
            if (_uiModelLoader == null)
            {
                // var task = Addressables.InstantiateAsync(Constants.ResAddress.UIModelLoader).Task;
                // yield return task.AsCoroution();
                // _uiModelLoader = task.Result.GetComponent<UIModelLoader>();

                var task = GameFramework.GetService<YooAssetsService>()
                    .InstanceAsync(Constants.ResAddress.UIModelLoader);
                GameObject go = null;
                yield return task.ToCoroutine(r => go = r);
                _uiModelLoader = go.GetComponent<UIModelLoader>();
                _uiModelLoader.TextureSize = _modelAnchor.size;
            }

            _uiModelLoader.LoadModel(_currPetData.ModelResId, texture =>
            {
                var tex = new NTexture(texture);
                _modelImage?.Dispose();

                _modelImage = new Image
                {
                    texture = tex,
                    blendMode = BlendMode.Normal
                };

                _modelAnchor.SetNativeObject(_modelImage);
            });


            // _modelWrapper ??= new GoWrapper();
            //
            // var npcResAddress = Constants.ResAddress.NpcRes(_currPetData.PetData.ResDataId);
            // yield return GameUtil.LoadMonsterModelToWrapper(npcResAddress, _modelAnchor, _modelWrapper);

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

        private int ChangePetData(bool next = true)
        {
            var c = next ? 1 : -1;
            var idx = _monsterListProvider.GetMonsterDataIndex(_currPetData.Id);
            idx += c;

            if (idx < 0) idx = 0;
            if (idx >= _monsterListProvider.Count)
            {
                idx = _monsterListProvider.Count - 1;
            }

            var pet = _monsterListProvider.GetMonsterDataAt(idx);
            CurrPetData = pet;

            return idx;
        }

        public override void LoadData(GComponent parentComp, GComponent content)
        {
            _dataNodeService = GameFramework.GetService<DataNodeService>();
            _gameDataService = GameFramework.GetService<GameDataService>();

            //load ui elements
            _txtMonsterName = content.GetChildByPath("basicInfo.txtName").asTextField;
            _txtMonsterSpecies = content.GetChildByPath("basicInfo.txtFamily").asTextField;
            _modelAnchor = content.GetChild("modelAnchor").asGraph;

            var btnPrev = content.GetChild("btnPrev").asButton;
            var btnNext = content.GetChild("btnNext").asButton;

            btnPrev.onClick.Set(() =>
            {
                var idx = ChangePetData(false);
                btnPrev.enabled = idx > 0;
                btnNext.enabled = idx < _monsterListProvider.Count - 1;
            });

            btnNext.onClick.Set(() =>
            {
                var idx = ChangePetData();
                btnPrev.enabled = idx > 0;
                btnNext.enabled = idx < _monsterListProvider.Count - 1;
            });

            var listBasic = content.GetChild("listBasic").asList;
            var listAbility = content.GetChild("listAbility").asList;
            var listResistance = content.GetChild("listResistance").asList;

            var basicPanel = listBasic.GetChildAt(0).asCom;
            _txtBasicRank = basicPanel.GetChild("txtRank").asTextField;
            _txtBasicLevel = basicPanel.GetChild("txtLevel").asTextField;
            _txtBasicLevelMax = basicPanel.GetChild("txtLevelMax").asTextField;
            _txtBasicSpecies = basicPanel.GetChild("txtSpecies").asTextField;

            _baseAttrLeft = basicPanel.GetChildByPath("attrs.attr_left").asCom;
            _baseAttrRight = basicPanel.GetChildByPath("attrs.attr_right").asCom;

            _calcAttrLeft = basicPanel.GetChildByPath("calcAttrs.attr_left").asCom;
            _calcAttrRight = basicPanel.GetChildByPath("calcAttrs.attr_right").asCom;

            _txtBasicExp = basicPanel.GetChildByPath("calcAttrs.exp").asTextField;
            _txtBasicExpToNext = basicPanel.GetChildByPath("calcAttrs.expToNextLevel").asTextField;

            var abilityPanel = listAbility.GetChildAt(0).asCom;
            _monsterSkillListPanel = new MonsterSkillListPanel(abilityPanel);

            var resisPanel = listResistance.GetChildAt(0).asCom;
            _resisElem = resisPanel.GetChild("resi_elem").asCom;
            _resisStatus = resisPanel.GetChild("resi_status").asCom;

            //load data
            _monsterListProvider =
                _dataNodeService.GetDataAndClear<IMonsterListProvider>(Constants.DataKeys.UIMonsterDetailDataList);
            CurrPetData =
                _dataNodeService.GetDataAndClear<IMonsterDataProvider>(Constants.DataKeys.UIMonsterDetailData) ??
                _monsterListProvider?.GetMonsterDataAt(0);

            _monsterSkillListPanel.DataProvider = _currPetData;
        }

        public override void Release()
        {
            base.Release();
            if (_modelImage != null)
            {
                _modelImage.Dispose();
                _modelImage = null;
            }

            if (_uiModelLoader != null)
            {
                //Addressables.ReleaseInstance(_uiModelLoader.gameObject);
                GameFramework.GetService<YooAssetsService>().ReleaseInstance(_uiModelLoader.gameObject);
            }
        }
    }
}