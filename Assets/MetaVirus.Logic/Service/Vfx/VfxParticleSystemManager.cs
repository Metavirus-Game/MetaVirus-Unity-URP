using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace MetaVirus.Logic.Service.Vfx
{
    [RequireComponent(typeof(ParticleSystem))]
    public class VfxParticleSystemManager : MonoBehaviour
    {
        private ParticleSystem _ps;

        private ParticleSystem[] AllPsComponents => gameObject.GetComponentsInChildren<ParticleSystem>();

        private readonly Dictionary<ParticleSystem, bool> _psLoopState = new();

        public bool IsLoopVfx => AllPsComponents.Any(ps => ps.main.loop);

        public UnityAction<GameObject> OnStopped;

        public bool IsStopped { get; private set; } = false;

        private void Awake()
        {
            _ps = GetComponent<ParticleSystem>();
            var mainModule = _ps.main;
            mainModule.stopAction = ParticleSystemStopAction.Callback;
        }

        public void OnSpawn()
        {
            IsStopped = false;
        }

        /// <summary>
        /// 将所有粒子的loop属性设置为false
        /// </summary>
        public void UnloopAllVfx()
        {
            if (!IsLoopVfx) return;

            foreach (var ps in AllPsComponents)
            {
                var main = ps.main;
                if (!main.loop) continue;
                _psLoopState[ps] = main.loop;
                main.loop = false;
            }
        }

        /// <summary>
        /// 将特效的loop状态恢复到停止以前，带有循环的粒子特效，被停止时会将loop设置成为false，完全停止后调用此方法恢复所有粒子的状态，以便下次使用
        /// </summary>
        public void RestoreLoopVfx()
        {
            foreach (var ps in _psLoopState.Keys)
            {
                var loop = _psLoopState[ps];
                var main = ps.main;
                main.loop = loop;
            }

            _psLoopState.Clear();
        }

        private void OnParticleSystemStopped()
        {
            IsStopped = true;
            OnStopped?.Invoke(gameObject);
        }
    }
}