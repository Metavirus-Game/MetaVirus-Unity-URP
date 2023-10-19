using System;
using System.Collections;
using System.Collections.Generic;
using GameEngine.Event;
using UnityEngine;
using static GameEngine.Common.Events;

namespace GameEngine.Sound
{
    public class SoundPlayer : MonoBehaviour
    {
        public SoundClip clip;

        private bool _lock;

        public void Lock()
        {
            _lock = true;
        }

        public void Unlock()
        {
            _lock = false;
        }

        public bool IsLocked => _lock;

        private bool _isMute = false;

        private AudioSource _audioSource;

        //播放音量
        private float _volume;

        /// <summary>
        /// 播放音量衰减百分比
        /// </summary>
        private float _volumePercent = 1;

        public float VolumePercent
        {
            set => _volumePercent = value;
            get => _volumePercent;
        }

        //声音绑定的transform
        private Transform _bindTransform;

        private Vector3? _playPosition = null;

        private Coroutine _eventCor;

        private Coroutine _fadeinCor;
        private Coroutine _fadeoutCor;

        private string _playerName = "";

        private SoundConstants.SoundPlayerStatus _playerStatus;

        private EventService _eventService;

        private List<SoundTask> _taskList = new();

        private bool loop
        {
            get => _audioSource != null && _audioSource.loop;

            set
            {
                if (_audioSource != null)
                {
                    _audioSource.loop = value;
                }
            }
        }

        public bool IsPlaying => _audioSource != null && _audioSource.isPlaying;

        public int Priority
        {
            get
            {
                if (clip != null)
                {
                    return clip.catalog.priority;
                }

                return 0;
            }
        }

        // Use this for initialization
        void Start()
        {
            _eventService = GameFramework.GetService<EventService>();

            _playerName = name;
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
            _audioSource.minDistance = 5;
        }

        // Update is called once per frame
        void Update()
        {
            if (IsPlaying)
            {
                name = _playerName + " " + clip;
            }
            else
            {
                name = _playerName;
            }

            FixVolume();
        }

        public void Mute()
        {
            _isMute = true;
        }

        public void UnMute()
        {
            _isMute = false;
        }

        private void FixVolume()
        {
            if (IsPlaying && _playerStatus == SoundConstants.SoundPlayerStatus.Playing)
            {
                float changeTime = Time.deltaTime;

                float volumePercent = this._volumePercent;
                if (_isMute)
                {
                    volumePercent = 0;
                }

                if (volumePercent < 1)
                {
                    float targetVolume = volumePercent * _volume;
                    if (clip.catalog.decreaseMethod == SoundConstants.SoundDecreaseMethod.渐变)
                    {
                        changeTime = Time.deltaTime;
                        _audioSource.volume = Mathf.Lerp(_audioSource.volume, targetVolume, changeTime);
                    }
                    else
                    {
                        _audioSource.volume = targetVolume;
                    }
                }
                else
                {
                    if (clip.catalog.decreaseMethod == SoundConstants.SoundDecreaseMethod.渐变)
                    {
                        changeTime = Time.deltaTime;
                        _audioSource.volume = Mathf.Lerp(_audioSource.volume, _volume, changeTime);
                    }
                    else
                    {
                        _audioSource.volume = _volume;
                    }
                }
            }
        }

        public void LateUpdate()
        {
            FlollowTransform();
        }

        private void FlollowTransform()
        {
            if (_bindTransform != null)
            {
                transform.position = _bindTransform.position;
            }
            else if (_playPosition != null)
            {
                transform.position = (Vector3)_playPosition;
            }
        }

        /// <summary>
        /// 播放一个音效
        /// </summary>
        /// <param name="volume">播放音量</param>
        /// <param name="pitch">播放音调</param>
        /// <param name="blend">3D效果(0-1)</param>
        /// <param name="loop">是否循环</param>
        public void Play(SoundClip clip, float volume, float pitch, float blend, bool loop)
        {
            if (IsPlaying)
            {
                return;
            }

            if (clip != null)
            {
                if (_bindTransform == null)
                {
                    transform.position = Vector3.zero;
                }
                else
                {
                    transform.position = _bindTransform.position;
                }

                _audioSource.clip = clip.clip;
                _audioSource.loop = loop;
                _audioSource.volume = this._volume = volume;
                _audioSource.spatialBlend = blend;
                _audioSource.pitch = pitch;
                _audioSource.Play();
                _eventService.Emit(SoundEvent.SoundPlayerEvent,
                    new SoundPlayerEvent(clip, SoundPlayerEvent.Type.StartPlaying));
                _eventCor = StartCoroutine(EventLoop());
                _playerStatus = SoundConstants.SoundPlayerStatus.Playing;
            }
        }

        /// <summary>
        /// 立即停止一个正在播放的音效
        /// </summary>
        public void Stop()
        {
            if (IsPlaying)
            {
                _audioSource.Stop();
                _bindTransform = null;

                if (_eventCor != null)
                {
                    StopCoroutine(_eventCor);
                    _eventCor = null;
                }

                foreach (var soundTask in _taskList)
                {
                    try
                    {
                        soundTask.InvokeAction();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }

                _taskList.Clear();

                _playerStatus = SoundConstants.SoundPlayerStatus.Idle;
                _eventService.Emit(SoundEvent.SoundPlayerEvent,
                    new SoundPlayerEvent(clip, SoundPlayerEvent.Type.Stopped));
            }
        }

        private IEnumerator EventLoop()
        {
            yield return new WaitForSeconds(_audioSource.clip.length);
            if (loop)
            {
                _eventService.Emit(SoundEvent.SoundPlayerEvent,
                    new SoundPlayerEvent(clip, SoundPlayerEvent.Type.LoopCompleted));
                _eventCor = StartCoroutine(EventLoop());
            }
            else
            {
                _eventService.Emit(SoundEvent.SoundPlayerEvent,
                    new SoundPlayerEvent(clip, SoundPlayerEvent.Type.Completed));
                _playerStatus = SoundConstants.SoundPlayerStatus.Idle;
            }
        }

        public void Play()
        {
            if (clip != null)
            {
                this._bindTransform = null;
                _playPosition = null;
                Play(clip, clip.volume, clip.pitch, clip.blend, clip.loop);
            }
        }

        public void Play(SoundClip clip)
        {
            this.clip = clip;
            Play();
        }

        /// <summary>
        /// 在指定位置播放音效，音效自动切换为3D模式
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="position"></param>
        public void PlayAtPosition(SoundClip clip, Vector3 position)
        {
            this.clip = clip;
            this.transform.position = position;
            this._bindTransform = null;
            _playPosition = position;
            Play(clip, clip.volume, clip.pitch, 1, clip.loop);
        }

        /// <summary>
        /// 跟随一个Transform播放音效，音效自动切换为3D模式
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="transform"></param>
        public void PlayWithTransform(SoundClip clip, Transform transform)
        {
            this.clip = clip;
            this._bindTransform = transform;
            this._playPosition = null;
            Play(clip, clip.volume, clip.pitch, 1, clip.loop);
        }

        /// <summary>
        /// 将声音淡出，然后STOP
        /// </summary>
        /// <param name="second"></param>
        public SoundTask FadeOut(float second)
        {
            var task = new SoundTask(this);

            if (_fadeoutCor != null)
            {
                _taskList.Add(task);
                return task;
            }

            if (IsPlaying)
            {
                _taskList.Add(task);
                _fadeoutCor = StartCoroutine(FadeOutSound(second));
            }

            return task;
        }

        private IEnumerator FadeOutSound(float second)
        {
            while (_playerStatus == SoundConstants.SoundPlayerStatus.Fadein)
            {
                yield return new WaitForEndOfFrame();
            }

            _playerStatus = SoundConstants.SoundPlayerStatus.Fadeout;
            var remaining = second;
            var v = _audioSource.volume;
            while (remaining > 0 && IsPlaying)
            {
                _audioSource.volume = Mathf.MoveTowards(_audioSource.volume, 0, v * (Time.deltaTime / second));
                yield return new WaitForEndOfFrame();
                remaining -= Time.deltaTime;
            }

            if (IsPlaying)
            {
                _audioSource.volume = 0;
                Stop();
            }

            _fadeoutCor = null;
        }

        /// <summary>
        /// 将声音淡入
        /// </summary>
        /// <param name="second"></param>
        public void FadeIn(float second)
        {
            if (_fadeinCor != null)
            {
                return;
            }

            if (IsPlaying)
            {
                _audioSource.volume = 0;
                _volume = clip.volume;
                _fadeinCor = StartCoroutine(FadeInSound(second));
            }
        }

        public void PlayOneShot(SoundClip sc, Transform playTransform = null)
        {
            if (clip != null)
            {
                _bindTransform = playTransform;
                if (_bindTransform == null)
                {
                    transform.position = Vector3.zero;
                }
                else
                {
                    transform.position = _bindTransform.position;
                }

                _audioSource.clip = clip.clip;
                _audioSource.loop = loop;
                _audioSource.volume = clip.volume;
                _audioSource.spatialBlend = clip.blend;
                _audioSource.pitch = clip.pitch;
                _audioSource.PlayOneShot(clip.clip);

                _eventService.Emit(SoundEvent.SoundPlayerEvent,
                    new SoundPlayerEvent(clip, SoundPlayerEvent.Type.StartPlaying));

                _eventCor = StartCoroutine(EventLoop());
                _playerStatus = SoundConstants.SoundPlayerStatus.Idle;
            }
        }

        private IEnumerator FadeInSound(float second)
        {
            _playerStatus = SoundConstants.SoundPlayerStatus.Fadein;
            float remaining = second;
            while (remaining > 0 && IsPlaying)
            {
                _audioSource.volume =
                    Mathf.MoveTowards(_audioSource.volume, _volume, _volume * (Time.deltaTime / second));
                yield return new WaitForEndOfFrame();
                remaining -= Time.deltaTime;
            }

            if (IsPlaying)
            {
                _audioSource.volume = _volume;
                _playerStatus = SoundConstants.SoundPlayerStatus.Playing;
            }

            _fadeinCor = null;
        }

        public void OnDrawGizmos()
        {
            if (_playPosition != null && (_playerStatus != SoundConstants.SoundPlayerStatus.Idle))
            {
                Gizmos.DrawWireSphere((Vector3)_playPosition, 1);
            }
        }
    }
}