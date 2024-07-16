using UnityEngine;
using YooAsset;

namespace GameEngine.Sound
{
    public class SoundClip : MonoBehaviour
    {
        public AudioClip clip;
        [Range(0, 1)] public float volume = 1;
        [Range(0, 1)] public float pitch = 1;
        [Range(0, 1)] public float blend = 0;
        public bool loop = false;

        internal bool AllowUnload = true;

        internal int Reference { get; set; } = 0;

        public SoundCatalog catalog { get; set; }
        
        public AssetHandle ResHandle { get; set; }

        internal int IncRef()
        {
            Reference = AllowUnload ? ++Reference : 1;
            return Reference;
        }

        internal int DecRef()
        {
            Reference = AllowUnload ? --Reference : 1;
            return Reference;
        }

        public override string ToString()
        {
            return $"Clip[{catalog.name}-{name}]";
        }
    }
}