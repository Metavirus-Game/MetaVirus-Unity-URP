using System;
using System.Collections;
using System.Collections.Generic;
using GameEngine.Config;
using GameEngine.ObjectPool;
using UnityEngine;

namespace GameEngine.Base
{
    public sealed class GameEngineRuntime
    {
        /// <summary>
        /// 设置引擎的环境信息
        /// </summary>
        private readonly List<string> _engineEnvList = new();

        private readonly List<BaseService> _serviceList = new List<BaseService>();
        private readonly Dictionary<Type, BaseService> _serviceMap = new Dictionary<Type, BaseService>();
        private Transform _root;

        private bool _running = false;

        private float _elapseTime = 0;
        private float _realElapseTime = 0;

        public bool IsRunning => _running;

        public GameEngineRuntime(Transform root)
        {
            this._root = root;
            Application.lowMemory += OnLowMemory;
        }

        public void AddEngineEnvValues(params string[] values)
        {
            foreach (var value in values)
            {
                if (!_engineEnvList.Contains(value))
                {
                    _engineEnvList.Add(value);
                }
            }
        }

        public bool HaveEngineEnvs(params string[] values)
        {
            bool ret = true;
            if (values.Length == 0)
            {
                return true;
            }

            foreach (var s in values)
            {
                if (!_engineEnvList.Contains(s))
                {
                    ret = false;
                }
            }

            return ret;
        }

        private void OnLowMemory()
        {
            var pool = GetService<ObjectPoolService>();
            pool.RemoveAllUnusedItems();
        }

        private T CreateService<T>() where T : BaseService
        {
            var go = new GameObject(typeof(T).Name);
            var service = go.AddComponent<T>();
            go.transform.SetParent(_root);
            if (_running)
            {
                //运行时添加service，调用初始化
                service.InitService();
                service.ServiceReady();
            }

            return service;
        }

        public void RegisterService(BaseService service)
        {
            if (_serviceList.Contains(service))
            {
                Debug.LogWarning($"Service [{service.GetType().FullName}] has been already registed");
                return;
            }

            _serviceList.Add(service);
            _serviceList.Sort(
                (s1, s2) => s1.Priority - s2.Priority
            );
            _serviceMap.Add(service.GetType(), service);
        }

        public T GetService<T>() where T : BaseService
        {
            _serviceMap.TryGetValue(typeof(T), out var service);
            if (service == null)
            {
                //service 不存在，创建一个
                return CreateService<T>();
            }

            return service as T;
        }

        public IEnumerator Init()
        {
            _running = true;
            Application.targetFrameRate = GameConfig.Inst.TargetFps;
            //var serviceLen = _serviceList.Count;
            var services = _serviceList.ToArray();

            foreach (var service in services)
            {
                service.InitService();
            }

            // for (var i = 0; i < serviceLen; i++)
            // {
            //     _serviceList[i].InitService();
            // }

            yield return null;

            foreach (var service in services)
            {
                service.ServiceReady();
            }

            // for (var i = 0; i < serviceLen; i++)
            // {
            //     _serviceList[i].ServiceReady();
            // }
        }

        public void Shutdown()
        {
            _running = false;
            for (var i = _serviceList.Count - 1; i >= 0; i--)
            {
                _serviceList[i].PreDestroy();
            }

            Application.lowMemory -= OnLowMemory;
        }

        internal void Update()
        {
            _elapseTime += Time.deltaTime;
            _realElapseTime += Time.unscaledDeltaTime;

            foreach (var service in _serviceList)
            {
                if (service.Ready)
                {
                    try
                    {
                        service.OnUpdate(Time.deltaTime, Time.unscaledDeltaTime);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Service[{service.name}] exception : {e.Message}");
                        Debug.LogException(e);
                    }
                }
            }
        }
    }
}