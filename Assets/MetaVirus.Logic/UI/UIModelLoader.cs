using System.Collections;
using Cysharp.Threading.Tasks;
using FairyGUI;
using GameEngine;
using GameEngine.Resource;
using GameEngine.Utils;
using MetaVirus.Logic.Data;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

namespace MetaVirus.Logic.UI
{
    public class UIModelLoader : MonoBehaviour
    {
        private GameObject _modelLoaded;
        private string _npcResAddressLoaded;

        public Transform anchor;
        public Camera modelCamera;

        private Image _image;
        private RenderTexture _modelTexture;

        private Vector2 _textureSize = new(400, 400);

        public Vector2 TextureSize
        {
            get => _textureSize;
            set
            {
                _textureSize = value;
                if (_modelTexture != null)
                {
                    modelCamera.targetTexture = null;
                    RenderTexture.ReleaseTemporary(_modelTexture);
                    _modelTexture =
                        RenderTexture.GetTemporary((int)TextureSize.x, (int)TextureSize.y, 24,
                            RenderTextureFormat.ARGB32);
                    modelCamera.targetTexture = _modelTexture;
                }
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            if (anchor == null)
            {
                anchor = transform.Find("Anchor");
            }

            if (modelCamera == null)
            {
                modelCamera = GetComponentInChildren<Camera>();
            }

            _modelTexture =
                RenderTexture.GetTemporary((int)TextureSize.x, (int)TextureSize.y, 24, RenderTextureFormat.ARGB32);
            modelCamera.targetTexture = _modelTexture;
        }

        public void LoadModel(int modelResId, UnityAction<RenderTexture> onLoaded)
        {
            var npcResAddress = Constants.ResAddress.NpcRes(modelResId);
            if (npcResAddress != _npcResAddressLoaded)
            {
                StartCoroutine(LoadModelEnum(npcResAddress, onLoaded));
            }
        }

        private IEnumerator LoadModelEnum(string resAddress, UnityAction<RenderTexture> onLoaded)
        {
            var yooService = GameFramework.GetService<YooAssetsService>();
            if (_modelLoaded != null)
            {
                //Addressables.ReleaseInstance(_modelLoaded);
                yooService.ReleaseInstance(_modelLoaded);
                _modelLoaded = null;
                _npcResAddressLoaded = null;
            }

            //var task = Addressables.InstantiateAsync(resAddress).Task;
            // yield return task.AsCoroution();
            // _modelLoaded = task.Result;
            var task = yooService.InstanceAsync(resAddress);
            _npcResAddressLoaded = resAddress;
            yield return task.ToCoroutine((r) =>
            {
                _modelLoaded = r;
            });
            // _modelLoaded = task.GetAwaiter().GetResult();

            _modelLoaded.SetActive(true);
            _modelLoaded.transform.SetParent(anchor, false);
            _modelLoaded.transform.localPosition = Vector3.zero;
            _modelLoaded.transform.localRotation = Quaternion.identity;

            GameEngineUtils.ChangeObjectsLayer(_modelLoaded, LayerMask.NameToLayer("UIModelLayer"), true);

            var navMeshAgent = _modelLoaded.GetComponentInChildren<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = false;
            }

            var animator = _modelLoaded.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                animator.SetBool(Constants.AniParamName.IsBorn, false);
            }

            onLoaded?.Invoke(_modelTexture);
        }

        // Update is called once per frame
        void Update()
        {
        }

        private void OnDestroy()
        {
            if (_modelLoaded != null)
            {
                //Addressables.ReleaseInstance(_modelLoaded);
                GameFramework.GetService<YooAssetsService>().ReleaseInstance(_modelLoaded);
            }

            if (_modelTexture != null)
            {
                RenderTexture.ReleaseTemporary(_modelTexture);
            }
        }
    }
}