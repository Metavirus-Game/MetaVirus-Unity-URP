using System.Collections;
using System.Collections.Generic;
using cfg.common;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using GameEngine;
using GameEngine.Resource;
using MetaVirus.Logic.Data;
using MetaVirus.Logic.Service.Battle;
using MetaVirus.ResExplorer.Resource;
using UnityEngine;
using UnityEngine.Events;

public class ModelLoader : MonoBehaviour
{
    public Texture modelTexture;

    private NpcResourceData _npcResourceData;
    private Transform _anchor;

    private GameObject _modelLoaded;

    private Camera _modelCamera;

    public float zoomOutDistance = -10;
    public float zoomInDistance = -3;

    public GameObject ModelLoaded => _modelLoaded;

    public AnimatorOverrideController ModelController => _currentAniCtrl;

    public RuntimeAnimatorController aniController;

    private AnimatorOverrideController _currentAniCtrl;

    private ExplorerResourceService _explorerResourceService;
    private YooAssetsService _yooAssetsService;

    private readonly List<AnimationClip> _animationClips = new();

    private AnimatorOverrideController _modelOriginAniController;

    private readonly List<KeyValuePair<AnimationClip, AnimationClip>> _overrideSetting = new();

    public List<KeyValuePair<AnimationClip, AnimationClip>> AniOverrideSetting => _overrideSetting;

    public List<AnimationClip> AnimationClips => _animationClips;

    private void Awake()
    {
        _explorerResourceService = GameFramework.GetService<ExplorerResourceService>();
        _yooAssetsService = GameFramework.GetService<YooAssetsService>();
        _modelCamera = GetComponentInChildren<Camera>();
    }

    public NpcResourceData ResourceData => _npcResourceData;

    public void SetResourceData(NpcResourceData resData, UnityAction<NpcResourceData> onDataLoaded)
    {
        if (_npcResourceData != resData)
        {
            _npcResourceData = resData;
            OnDataChanged(onDataLoaded);
        }
    }

    /// <summary>
    /// 缩放摄像机，value=0 to 100
    /// </summary>
    /// <param name="value"></param>
    public void ZoomCamera(float value)
    {
        var percent = value / 100;
        var distance = (zoomOutDistance - zoomInDistance) * percent + zoomInDistance;
        var p = _modelCamera.transform.localPosition;
        p.z = distance;
        _modelCamera.transform.localPosition = p;
    }

    private void OnDataChanged(UnityAction<NpcResourceData> onDataLoaded = null)
    {
        if (_anchor == null)
        {
            _anchor = transform.Find("ModelAnchor");
        }

        if (_modelLoaded != null)
        {
            //Addressables.ReleaseInstance(_modelLoaded);
        }

        if (_npcResourceData != null)
        {
            StartCoroutine(LoadModel(onDataLoaded));
        }
    }

    private IEnumerator LoadModel(UnityAction<NpcResourceData> onDataLoaded = null)
    {
        var resAddress = Constants.ResAddress.BattleUnitRes(_npcResourceData.Id);
        //var task = Addressables.InstantiateAsync(resAddress).Task;
        var task = _yooAssetsService.InstanceAsync(resAddress);
        yield return task.ToCoroutine(r => _modelLoaded = r);
        _modelLoaded.transform.SetParent(_anchor, false);
        _modelLoaded.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        var bua = _modelLoaded.GetComponent<BattleUnitAni>();

        _modelLoaded.SetActive(true);

        //获取模型本身配置的动画控制器
        var ani = _modelLoaded.GetComponent<Animator>();
        _modelOriginAniController = (AnimatorOverrideController)ani.runtimeAnimatorController;

        //生成一个能播放指定动画的动画控制器
        _currentAniCtrl = new AnimatorOverrideController(aniController);
        ani.runtimeAnimatorController = _currentAniCtrl;
        ani.enabled = false;

        //读取模型相关的所有动画
        var sId = _npcResourceData.Id / 1000;
        var resPath = $"CuteRes-{sId:D2}/{_npcResourceData.Id}-{_npcResourceData.Name}";

        var anims = _explorerResourceService.GetResourcePath(resPath, ".anim");
        _animationClips.Clear();

        foreach (var animPath in anims)
        {
            // var aniTask = Addressables.LoadAssetAsync<AnimationClip>(animPath).Task;
            var handle = _yooAssetsService.GetPackage().LoadAssetAsync<AnimationClip>(animPath);
            yield return handle;
            var r = handle.GetAssetObject<AnimationClip>();
            if (r != null)
            {
                _animationClips.Add(r);
            }
        }

        _overrideSetting.Clear();
        //读取模型当前已经配置的动画
        _modelOriginAniController.GetOverrides(_overrideSetting);

        ani.enabled = true;

        onDataLoaded?.Invoke(_npcResourceData);
    }

    public void SetCurrentAniClip(AnimationClip clip)
    {
        _currentAniCtrl["Ghost@Idle"] = clip;
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }
}