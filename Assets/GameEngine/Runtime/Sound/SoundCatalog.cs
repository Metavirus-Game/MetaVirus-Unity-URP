using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameEngine.Sound
{
    public class SoundCatalog : MonoBehaviour
    {
        [Tooltip("播放优先权，优先权高的音效播放时，会压低优先权低的音效，相等优先权的音效互不影响")]
        public int priority = 0;

        [Tooltip("当有优先权更高的音效播放时，当前类别音效的衰减百分比")]
        public float decreasePercent = 0.6f;

        public SoundConstants.SoundDecreaseMethod decreaseMethod = SoundConstants.SoundDecreaseMethod.渐变;
        private readonly List<SoundClip> _soundClips = new();

        public void Start()
        {
            var soundClips = GetComponentsInChildren<SoundClip>();
            foreach (var sc in _soundClips)
            {
                sc.catalog = this;
            }

            _soundClips.AddRange(soundClips);
        }

        public void Update()
        {
        }

        public void AddClip(SoundClip soundClip)
        {
            soundClip.catalog = this;
            _soundClips.Add(soundClip);
        }

        public void RemoveClip(SoundClip soundClip)
        {
            _soundClips.Remove(soundClip);
        }

        public SoundClip GetSoundClip(string soundName)
        {
            return _soundClips.FirstOrDefault(sc => sc.name.Equals(soundName));
        }
    }
}