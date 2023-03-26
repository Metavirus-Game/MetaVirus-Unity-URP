using System.Collections;
using System.Collections.Generic;
using System.IO;
using cfg.common;
using GameEngine.Base;
using MetaVirus.Logic.Player;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace MetaVirus.Logic.Service.NpcHeader
{
    public class NpcHeaderService : BaseService
    {
        internal enum HeaderLoadStatus
        {
            Waiting,
            Processing,
            Loaded
        }

        internal class HeaderRequest
        {
            public HeaderType HeaderType { get; set; }
            public ulong avatar { get; set; }
            public NpcResourceData npcResData { get; set; }

            public UnityAction<Texture> OnLoaded { get; set; }

            public HeaderLoadStatus Status { get; set; }

            public static HeaderRequest AvatarRequest(ulong avatar, UnityAction<Texture> onLoaded)
            {
                var ret = new HeaderRequest
                {
                    HeaderType = HeaderType.Avatar,
                    avatar = avatar,
                    npcResData = null,
                    OnLoaded = onLoaded,
                    Status = HeaderLoadStatus.Waiting
                };
                return ret;
            }

            public static HeaderRequest NpcDataRequest(NpcResourceData npcData, UnityAction<Texture> onLoaded)
            {
                var ret = new HeaderRequest
                {
                    HeaderType = HeaderType.NpcRes,
                    avatar = 0,
                    npcResData = npcData,
                    OnLoaded = onLoaded,
                    Status = HeaderLoadStatus.Waiting
                };
                return ret;
            }
        }

        private const string HeaderCapturerPrefabAssets = "HeaderCapturer/Prefab/HeaderCapturer.prefab";
        private static readonly string NpcHeaderPath = "NpcHeader_";

        private Camera _capturerCamera;
        private RenderTexture _capturerTexture;
        private Transform _capturerAnchor;

        private CharacterTemplate _playerTemplate;
        private Texture2D _outTex;

        /// <summary>
        /// key = Type_Id
        /// </summary>
        private HeaderCache _headerCache;

        private List<HeaderRequest> _requests = new();

        public override void ServiceReady()
        {
            _headerCache = new HeaderCache();
            StartCoroutine(LoadHeaderCapturer());
        }

        public override void OnLowerMemory()
        {
            _headerCache.RemoveAllUnused();
        }

        private IEnumerator LoadHeaderCapturer()
        {
            var op = Addressables.InstantiateAsync(HeaderCapturerPrefabAssets);
            yield return op;

            var go = op.Result;
            _capturerCamera = go.GetComponentInChildren<Camera>();
            _capturerAnchor = go.transform.Find("ModelAnchor");
            _playerTemplate = go.GetComponentInChildren<CharacterTemplate>();
            _capturerTexture = new RenderTexture(256, 256, 32)
            {
                antiAliasing = 4
            };
            _capturerCamera.targetTexture = _capturerTexture;
            _outTex = new Texture2D(_capturerTexture.width, _capturerTexture.height, TextureFormat.RGBA32, false,
                false);

            _playerTemplate.gameObject.SetActive(false);

            DontDestroyOnLoad(go);
        }

        public override void OnUpdate(float elapseTime, float realElapseTime)
        {
            if (_requests.Count > 0)
            {
                var request = _requests[0];
                switch (request.Status)
                {
                    case HeaderLoadStatus.Waiting:
                        request.Status = HeaderLoadStatus.Processing;
                        StartCoroutine(LoadNpcAvatarHeader(request.avatar, tex =>
                        {
                            request.Status = HeaderLoadStatus.Loaded;
                            request.OnLoaded?.Invoke(tex);
                        }));
                        break;
                    case HeaderLoadStatus.Processing:
                        //do nothing
                        break;
                    case HeaderLoadStatus.Loaded:
                        _requests.Remove(request);
                        break;
                }
            }
        }

        public void GetNpcHeader(ulong npcAvatar, UnityAction<Texture> onLoaded)
        {
            var request = HeaderRequest.AvatarRequest(npcAvatar, onLoaded);
            _requests.Add(request);
            //var taskCompletionSource = new TaskCompletionSource<Texture>();
            // StartCoroutine(LoadNpcHeaderCor(npcAvatar, onLoaded));
            //return taskCompletionSource.Task;
        }

        public void ReleaseNpcHeader(ulong npcAvatar)
        {
            _headerCache.Release(HeaderType.Avatar, npcAvatar);
        }

        private IEnumerator LoadNpcAvatarHeader(ulong npcAvatar, UnityAction<Texture> onLoaded)
        {
            var item = _headerCache.Get(HeaderType.Avatar, npcAvatar);
            if (item != null)
            {
                onLoaded?.Invoke(item.Texture);
            }
            else
            {
                var npcHeaderFile = $"{Application.persistentDataPath}/{NpcHeaderPath}{npcAvatar.ToString()}.png";
                var npcHeader = $"file://{npcHeaderFile}";
                var uwr = UnityWebRequestTexture.GetTexture(npcHeader);
                yield return uwr.SendWebRequest();

                if (uwr.result == UnityWebRequest.Result.Success)
                {
                    var texture = DownloadHandlerTexture.GetContent(uwr);
                    _headerCache.Add(HeaderType.Avatar, npcAvatar, texture);
                    onLoaded(texture);
                }
                else
                {
                    //创建文件并存储

                    _playerTemplate.gameObject.SetActive(true);
                    _playerTemplate.ParseFromLongData(npcAvatar);
                    yield return null;

                    try
                    {
                        RenderTexture.active = _capturerTexture;

                        _outTex.ReadPixels(new Rect(0, 0, _capturerTexture.width, _capturerTexture.height), 0, 0);
                        _outTex.Apply();

                        var png = _outTex.EncodeToPNG();
                        File.WriteAllBytes(npcHeaderFile, png);

                        var texture = Instantiate(_outTex);

                        _headerCache.Add(HeaderType.Avatar, npcAvatar, texture);
                        onLoaded(texture);
                    }
                    finally
                    {
                        RenderTexture.active = null;
                        _playerTemplate.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}