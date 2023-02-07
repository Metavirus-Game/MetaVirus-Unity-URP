using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameEngine.Base;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace GameEngine.Sound
{
    public class SoundService : BaseService
    {
        private readonly Dictionary<string, SoundCatalog> _catalogs = new();
        private SoundPlayer[] _soundPlayers;

        [Range(1, 100)] public int soundPoolSize = 20;
        //public bool dontDestroyOnLoad = false;

        private List<string> _bgmStack = new();
        private SoundPlayer _currentBgmPlayer;

        private Transform SoundCatalogRoot
        {
            get
            {
                var cataTrans = transform.Find("Catalogs");
                if (cataTrans == null)
                {
                    cataTrans = new GameObject("Catalogs").transform;
                    cataTrans.SetParent(transform, false);
                }

                return cataTrans;
            }
        }

        public override void PostConstruct()
        {
            InitCatalogs();
            InitSoundPool();
        }

        private void InitCatalogs()
        {
            var catalogs = GetComponentsInChildren<SoundCatalog>();
            foreach (var catalog in catalogs)
            {
                _catalogs[catalog.name] = catalog;
            }
        }

        private void InitSoundPool()
        {
            _soundPlayers = new SoundPlayer[soundPoolSize];
            var players = new GameObject("SoundPlayers")
            {
                transform =
                {
                    parent = transform
                }
            };
            for (var i = 0; i < soundPoolSize; i++)
            {
                var sp = new GameObject("SoundPlayer_" + (i + 1));
                sp.transform.parent = players.transform;
                _soundPlayers[i] = sp.AddComponent<SoundPlayer>();
            }
        }

        private SoundCatalog GetSoundCatalog(string catalogName)
        {
            _catalogs.TryGetValue(catalogName, out var catalog);
            return catalog;
            // return _cagalogs.FirstOrDefault(sc => sc.name.Equals(catalogName));
        }

        private SoundClip FindSoundClip(string catalogName, string clipName)
        {
            var catalog = GetSoundCatalog(catalogName);
            return catalog != null ? catalog.GetSoundClip(clipName) : null;
        }

        public async Task<SoundClip> AsyncLoadSoundClip(string fullName, string soundAddress,
            float volume, bool loop, bool allowUnload = true)
        {
            ParseClipPath(fullName, out var catalog, out var sound);
            if (string.IsNullOrEmpty(catalog) || string.IsNullOrEmpty(sound)) return null;
            var clip = await AsyncLoadSoundClip(catalog, sound, soundAddress, volume, loop, allowUnload);
            return clip;
        }

        public IEnumerator LoadSoundClipCor(string soundCatalog, string soundName, string soundAddress,
            float volume, bool loop, bool allowUnload = true)
        {
            var catalog = GetSoundCatalog(soundCatalog);
            if (catalog == null)
            {
                Debug.LogError($"Sound Catalog {soundCatalog} not found");
                yield break;
            }


            Debug.Log($"SoundService -- SoundClip[{soundCatalog}/{soundName}] Loading");

            var soundClip = catalog.GetSoundClip(soundName);
            if (soundClip == null)
            {
                var handle = Addressables.LoadAssetAsync<AudioClip>(soundAddress);
                while (handle.Status == AsyncOperationStatus.None)
                {
                    Debug.Log($"SoundService -- SoundClip[{soundCatalog}/{soundName}] Status None");
                    if (handle.OperationException != null)
                    {
                        Debug.Log(handle.OperationException);
                    }
                    yield return null;
                }

                Debug.Log($"SoundService -- SoundClip[{soundCatalog}/{soundName}] Status {handle.Status}");

                if (handle.Status == AsyncOperationStatus.Failed)
                {
                    Debug.Log($"SoundService -- SoundClip[{soundCatalog}/{soundName}] Load Failed");
                    Debug.Log(handle.OperationException);
                }
                else
                {
                    Debug.Log($"SoundService -- SoundClip[{soundCatalog}/{soundName}] Loaded");
                    var go = new GameObject(soundName);
                    go.transform.SetParent(catalog.transform, false);
                    soundClip = go.AddComponent<SoundClip>();
                    soundClip.clip = handle.Result;
                    soundClip.name = soundName;
                    soundClip.volume = volume;
                    soundClip.loop = loop;
                    catalog.AddClip(soundClip);
                }
            }

            if (soundClip != null)
            {
                soundClip.AllowUnload = allowUnload;
                soundClip.IncRef();
            }
        }

        public async Task<SoundClip> AsyncLoadSoundClip(string soundCatalog, string soundName, string soundAddress,
            float volume, bool loop, bool allowUnload = true)
        {
            var catalog = GetSoundCatalog(soundCatalog);
            if (catalog == null)
            {
                Debug.LogError($"Sound Catalog {soundCatalog} not found");
                return null;
            }


            Debug.Log($"SoundService -- SoundClip[{soundCatalog}/{soundName}] Loading");

            var soundClip = catalog.GetSoundClip(soundName);
            if (soundClip == null)
            {
                try
                {
                    var audioClip = await Addressables.LoadAssetAsync<AudioClip>(soundAddress).Task;
                    Debug.Log($"SoundService -- SoundClip[{soundCatalog}/{soundName}] Loaded");
                    var go = new GameObject(soundName);
                    go.transform.SetParent(catalog.transform, false);
                    soundClip = go.AddComponent<SoundClip>();
                    soundClip.clip = audioClip;
                    soundClip.name = soundName;
                    soundClip.volume = volume;
                    soundClip.loop = loop;
                    catalog.AddClip(soundClip);
                }
                catch (Exception e)
                {
                    Debug.Log(e);
                }
            }

            soundClip.AllowUnload = allowUnload;
            soundClip.IncRef();


            return soundClip;
        }

        public void UnloadSoundClip(string soundCatalog, string soundName)
        {
            var catalog = GetSoundCatalog(soundCatalog);
            if (catalog == null)
            {
                Debug.LogError($"Sound Catalog {soundCatalog} not found");
                return;
            }

            var soundClip = catalog.GetSoundClip(soundName);
            if (soundClip == null) return;
            if (soundClip.DecRef() > 0) return;

            //卸载soundClip，并停止所有正在播放的soundClip
            foreach (var soundPlayer in _soundPlayers)
            {
                if (soundPlayer.IsPlaying && soundPlayer.clip == soundClip)
                {
                    soundPlayer.Stop();
                }
            }

            catalog.RemoveClip(soundClip);
            Addressables.Release(soundClip.clip);
            Destroy(soundClip.gameObject);
        }

        /// <summary>
        /// 新增SoundCatalog
        /// </summary>
        /// <param name="catalogName"></param>
        /// <param name="priority"></param>
        /// <param name="decreasePercent"></param>
        public SoundCatalog AddCatalog(string catalogName, int priority, float decreasePercent)
        {
            var sc = GetSoundCatalog(catalogName);
            if (sc != null)
            {
                return sc;
            }

            var go = new GameObject(catalogName);
            go.transform.SetParent(SoundCatalogRoot, false);
            sc = go.AddComponent<SoundCatalog>();

            sc.priority = priority;
            sc.decreasePercent = decreasePercent;
            sc.decreaseMethod = SoundConstants.SoundDecreaseMethod.渐变;
            _catalogs[catalogName] = sc;
            return sc;
        }

        public SoundPlayer PickIdlePlayer()
        {
            return _soundPlayers.FirstOrDefault(sp => !sp.IsPlaying && !sp.IsLocked);
        }

        public override void OnUpdate(float elapseTime, float realElapseTime)
        {
            BalanceSoundVolume();
        }

        public void StopAllSoundPlayer()
        {
            foreach (SoundPlayer sp in _soundPlayers)
            {
                if (sp.IsPlaying)
                {
                    if (sp.clip.loop)
                    {
                        sp.FadeOut(3);
                    }
                    else
                    {
                        sp.Stop();
                    }
                }
            }
        }

        private void BalanceSoundVolume()
        {
            var topPriority = GetTopPriority();

            foreach (var sp in _soundPlayers)
            {
                if (topPriority > -1)
                {
                    if (sp.IsPlaying)
                    {
                        if (sp.Priority < topPriority)
                        {
                            //低于最高优先权的音效降低音量
                            sp.VolumePercent = sp.clip.catalog.decreasePercent;
                        }
                        else
                        {
                            sp.VolumePercent = 1;
                        }
                    }
                }
                else
                {
                    sp.VolumePercent = 1;
                }
            }
        }

        /// <summary>
        /// 获取当前播放音效的最高优先权值
        /// </summary>
        /// <returns></returns>
        private int GetTopPriority()
        {
            int topPriority = -1;
            foreach (var sp in _soundPlayers)
            {
                if (sp.IsPlaying && sp.Priority > topPriority)
                {
                    topPriority = sp.Priority;
                }
            }

            return topPriority;
        }

        public static string ToFullPath(string catalog, string clipName)
        {
            return $"{catalog}/{clipName}";
        }

        public static void ParseClipPath(string clipFullPath, out string catalog, out string clip)
        {
            catalog = clip = "";
            if (clipFullPath.Contains("/"))
            {
                var ret = clipFullPath.Split('/');
                if (ret.Length >= 2)
                {
                    catalog = ret[0];
                    clip = ret[1];
                }
            }
        }

        public SoundPlayer PlayBGM(string fullName, float fadeTime = 1)
        {
            ParseClipPath(fullName, out var catalog, out var sound);
            if (!string.IsNullOrEmpty(catalog) && !string.IsNullOrEmpty(sound))
            {
                return PlayBGM(catalog, sound);
            }

            return null;
        }

        /// <summary>
        /// 播放指定的背景音乐，自动淡出当前正在播放的背景音乐，并将当前播放的背景音乐播放列表中
        /// </summary>
        /// <param name="catalog"></param>
        /// <param name="sound"></param>
        /// <param name="fadeTime"></param>
        /// <returns></returns>
        public SoundPlayer PlayBGM(string catalog, string sound, float fadeTime = 1)
        {
            if (_currentBgmPlayer != null)
            {
                var clip = _currentBgmPlayer.clip;
                var soundPath = clip.catalog.name + "/" + clip.name;
                _bgmStack.Insert(0, soundPath);

                _currentBgmPlayer.FadeOut(fadeTime);
                _currentBgmPlayer = null;
            }

            Debug.Log($"SoundService -- Play BGM {catalog}/{sound}");

            _currentBgmPlayer = Play(catalog, sound, fadeTime);
            return _currentBgmPlayer;
        }

        /// <summary>
        /// 停止指定的背景音乐，
        /// 如果指定的背景音乐是当前正在播放的，则
        /// ①淡出当前正在播放的音乐
        /// ②从bgmStack的顶端取出上一个播放的背景音乐，并淡入播放
        /// 否则 仅将指定的背景音乐从 播放列表中移除
        /// </summary>
        public void StopBGM(string catalog, string sound, float fadeTime = 1, bool unload = false)
        {
            if (_currentBgmPlayer != null && _currentBgmPlayer.clip.catalog.name == catalog &&
                _currentBgmPlayer.clip.name == sound)
            {
                var task = _currentBgmPlayer.FadeOut(fadeTime);
                _currentBgmPlayer = null;

                if (unload)
                {
                    task.OnStopped(p =>
                    {
                        //unload this sound clip
                        UnloadSoundClip(catalog, sound);
                    });
                }

                if (_bgmStack.Count > 0)
                {
                    var fullPath = _bgmStack[0];
                    _bgmStack.RemoveAt(0);

                    _currentBgmPlayer = Play(fullPath, fadeTime);
                }
            }
            else
            {
                _bgmStack.Remove(catalog + "/" + sound);
                UnloadSoundClip(catalog, sound);
            }
        }

        /// <summary>
        /// 播放指定类别下的音效
        /// </summary>
        /// <param name="catalog">类别名称</param>
        /// <param name="sound">音效名称</param>
        /// <param name="fadeInTime">淡入音效的时间，0表示不使用淡入</param>
        public SoundPlayer Play(string catalog, string sound, float fadeInTime = 0)
        {
            if (string.IsNullOrEmpty(catalog) && string.IsNullOrEmpty(sound))
            {
                return null;
            }

            var clip = FindSoundClip(catalog, sound);
            if (clip != null)
            {
                var sp = PickIdlePlayer();
                if (sp != null)
                {
                    sp.Play(clip);
                    if (fadeInTime > 0)
                    {
                        sp.FadeIn(fadeInTime);
                    }

                    return sp;
                }
                else
                {
                    Debug.Log(string.Format("Sound [{0}-{1}] play failed, not enough SoundPlayer", catalog, sound));
                }
            }
            else
            {
                Debug.Log(string.Format("Sound [{0}-{1}] Not Found", catalog, sound));
            }

            return null;
        }

        public SoundPlayer Play(string clipFullPath, float fadeInTime = 0)
        {
            ParseClipPath(clipFullPath, out var catalog, out var sound);
            var clip = FindSoundClip(catalog, sound);
            if (clip != null)
            {
                SoundPlayer sp = PickIdlePlayer();
                if (sp != null)
                {
                    sp.Play(clip);
                    if (fadeInTime > 0)
                    {
                        sp.FadeIn(fadeInTime);
                    }

                    return sp;
                }
                else
                {
                    Debug.Log(string.Format("Sound [{0}-{1}] play failed, not enough SoundPlayer", catalog, sound));
                }
            }
            else
            {
                Debug.Log(string.Format("Sound [{0}-{1}] Not Found", catalog, sound));
            }

            return null;
        }

        public SoundPlayer Play(SoundClip clip, float fadeInTime = 0)
        {
            if (clip != null)
            {
                var sp = PickIdlePlayer();
                if (sp != null)
                {
                    sp.Play(clip);
                    if (fadeInTime > 0)
                    {
                        sp.FadeIn(fadeInTime);
                    }

                    return sp;
                }
                else
                {
                    Debug.Log(string.Format("Sound [{0}-{1}] play failed, not enough SoundPlayer", clip.catalog.name,
                        clip.name));
                }
            }

            return null;
        }

        /// <summary>
        /// 在指定位置播放音效
        /// </summary>
        /// <param name="catalog"></param>
        /// <param name="sound"></param>
        /// <param name="position"></param>
        /// <param name="fadeInTime"></param>
        public SoundPlayer PlayAtPosition(string catalog, string sound, Vector3 position, float fadeInTime = 0)
        {
            var clip = FindSoundClip(catalog, sound);
            if (clip != null)
            {
                var sp = PickIdlePlayer();
                if (sp != null)
                {
                    sp.PlayAtPosition(clip, position);
                    if (fadeInTime > 0)
                    {
                        sp.FadeIn(fadeInTime);
                    }

                    return sp;
                }
                else
                {
                    Debug.Log(string.Format("Sound [{0}-{1}] play failed, not enough SoundPlayer", catalog, sound));
                }
            }
            else
            {
                Debug.Log(string.Format("Sound [{0}-{1}] Not Found", catalog, sound));
            }

            return null;
        }

        public SoundPlayer PlayAtPosition(string clipFullPath, Vector3 position, float fadeInTime = 0)
        {
            ParseClipPath(clipFullPath, out var catalog, out var sound);
            var clip = FindSoundClip(catalog, sound);
            if (clip != null)
            {
                var sp = PickIdlePlayer();
                if (sp != null)
                {
                    sp.PlayAtPosition(clip, position);
                    if (fadeInTime > 0)
                    {
                        sp.FadeIn(fadeInTime);
                    }

                    return sp;
                }
                else
                {
                    Debug.Log(string.Format("Sound [{0}-{1}] play failed, not enough SoundPlayer", catalog, sound));
                }
            }
            else
            {
                Debug.Log(string.Format("Sound [{0}-{1}] Not Found", catalog, sound));
            }

            return null;
        }

        public SoundPlayer PlayAtPosition(SoundClip clip, Vector3 position, float fadeInTime = 0)
        {
            if (clip != null)
            {
                var sp = PickIdlePlayer();
                if (sp != null)
                {
                    sp.PlayAtPosition(clip, position);
                    if (fadeInTime > 0)
                    {
                        sp.FadeIn(fadeInTime);
                    }

                    return sp;
                }
                else
                {
                    Debug.Log(string.Format("Sound [{0}-{1}] play failed, not enough SoundPlayer", clip.catalog.name,
                        clip.name));
                }
            }
            else
            {
                Debug.Log(string.Format("Sound [{0}-{1}] Not Found", clip.catalog.name, clip.name));
            }

            return null;
        }

        /// <summary>
        /// 绑定音效到指定的Transform上播放
        /// </summary>
        /// <param name="catalog"></param>
        /// <param name="sound"></param>
        /// <param name="transform"></param>
        /// <param name="fadeInTime"></param>
        public SoundPlayer PlayWithTransform(string catalog, string sound, Transform transform, float fadeInTime = 0)
        {
            var clip = FindSoundClip(catalog, sound);
            if (clip != null)
            {
                var sp = PickIdlePlayer();
                if (sp != null)
                {
                    sp.PlayWithTransform(clip, transform);
                    if (fadeInTime > 0)
                    {
                        sp.FadeIn(fadeInTime);
                    }

                    return sp;
                }
                else
                {
                    Debug.Log(string.Format("Sound [{0}-{1}] play failed, not enough SoundPlayer", catalog, sound));
                }
            }
            else
            {
                Debug.Log(string.Format("Sound [{0}-{1}] Not Found", catalog, sound));
            }

            return null;
        }

        public SoundPlayer PlayWithTransform(string clipFullPath, Transform transform, float fadeInTime = 0)
        {
            string catalog;
            string sound;
            ParseClipPath(clipFullPath, out catalog, out sound);
            var clip = FindSoundClip(catalog, sound);
            if (clip != null)
            {
                var sp = PickIdlePlayer();
                if (sp != null)
                {
                    sp.PlayWithTransform(clip, transform);
                    if (fadeInTime > 0)
                    {
                        sp.FadeIn(fadeInTime);
                    }

                    return sp;
                }
                else
                {
                    Debug.Log(string.Format("Sound [{0}-{1}] play failed, not enough SoundPlayer", catalog, sound));
                }
            }
            else
            {
                Debug.Log(string.Format("Sound [{0}-{1}] Not Found", catalog, sound));
            }

            return null;
        }

        public SoundPlayer PlayWithTransform(SoundClip clip, Transform transform, float fadeInTime = 0)
        {
            if (clip != null)
            {
                var sp = PickIdlePlayer();
                if (sp != null)
                {
                    sp.PlayWithTransform(clip, transform);
                    if (fadeInTime > 0)
                    {
                        sp.FadeIn(fadeInTime);
                    }

                    return sp;
                }
                else
                {
                    Debug.Log(string.Format("Sound [{0}-{1}] play failed, not enough SoundPlayer", clip.catalog.name,
                        clip.name));
                }
            }
            else
            {
                Debug.Log(string.Format("Sound [{0}-{1}] Not Found", clip.catalog.name, clip.name));
            }

            return null;
        }
    }
}