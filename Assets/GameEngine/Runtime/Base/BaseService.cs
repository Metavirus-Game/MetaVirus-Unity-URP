using System.Reflection;
using FairyGUI;
using GameEngine.Base.Attributes;
using GameEngine.Event;
using GameEngine.Network;
using UnityEngine;
using UnityEngine.Events;
using static GameEngine.Common.EngineConsts;

namespace GameEngine.Base
{
    /**
     * service 生命周期
     * Instance  → PostConstruct → Awake → Start
     * PreDestory → Destory
     */
    public abstract class BaseService : MonoBehaviour
    {
        protected LocalizeService Localize => GameFramework.Runtime.GetService<LocalizeService>();
        protected EventService Event => GameFramework.Runtime.GetService<EventService>();

        public int Priority { get; internal set; }

        public bool Ready { get; internal set; }

        public BaseService()
        {
            var sp = GetType().GetCustomAttribute<ServicePriority>();
            Priority = sp?.Order ?? ServicePriorityValue.OrderDefault;
        }

        private void Awake()
        {
            GameFramework.Runtime.RegisterService(this);
        }

        internal void InitService()
        {
            PostConstruct();
            Ready = true;
        }

        protected T GetService<T>() where T : BaseService
        {
            return GameFramework.GetService<T>();
        }

        public virtual void PostConstruct()
        {
        }

        public virtual void ServiceReady()
        {
        }

        public virtual void PreDestroy()
        {
        }

        public virtual void OnUpdate(float elapseTime, float realElapseTime)
        {
        }

        public virtual void OnLowerMemory()
        {
        }
    }
}