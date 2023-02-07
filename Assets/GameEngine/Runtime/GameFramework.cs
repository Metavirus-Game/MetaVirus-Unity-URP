using System;
using System.Collections;
using GameEngine.Base;
using GameEngine.Common;
using GameEngine.Event;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameEngine
{
    [DefaultExecutionOrder(-9999)]
    public class GameFramework : MonoBehaviour
    {
        public string[] engineEnvList = Array.Empty<string>();

        private static GameFramework _inst;
        private static Transform _rootTrans;

        private GameEngineRuntime _runtime;

        public static GameEngineRuntime Runtime
        {
            get
            {
                //GameEngine必须手动拖到引擎中
                if (_inst == null)
                {
                    throw new Exception("GameEngine MonoBehaviour not found in scene!");
                }

                return _inst._runtime;
            }
        }

        public static GameFramework Inst => _inst;

        public void Awake()
        {
            _inst = this;
            _rootTrans = transform.Find("Services");
            _runtime = new GameEngineRuntime(_rootTrans);
            _runtime.AddEngineEnvValues(engineEnvList);
            DontDestroyOnLoad(gameObject);
        }

        public void Start()
        {
            StartCoroutine(OnStartInit());
        }

        private IEnumerator OnStartInit()
        {
            yield return null;
            yield return Runtime.Init();
            GetService<EventService>().Emit(Events.Engine.EngineStarted);
        }

        public static T GetService<T>() where T : BaseService
        {
            var service = Runtime.GetService<T>();
            return service;
        }

        /**
         * 返回key对应的localized值
         */
        public static string L(string key)
        {
            return Runtime.GetService<LocalizeService>().GetLanguageText(key);
        }

        private void OnDestroy()
        {
            Runtime?.Shutdown();
        }

        private void Update()
        {
            Runtime.Update();
        }
    }
}