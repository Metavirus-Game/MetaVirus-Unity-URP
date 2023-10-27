using System;
using System.Collections.Generic;
using GameEngine.Base;
using GameEngine.Base.Attributes;
using GameEngine.Base.Exception;
using GameEngine.Fsm;
using GameEngine.Utils;
using UnityEngine;
using static GameEngine.Common.EngineConsts;

namespace GameEngine.Procedure
{
    [ServicePriority(ServicePriorityValue.ProcedureService)]
    public class ProcedureService : BaseService
    {
        private FsmService _fsmService;
        private ProcedureEntity _procedureFsm;

        public ProcedureEntity ProcedureFsm => _procedureFsm;

        private readonly List<ProcedureBase> _procedures = new List<ProcedureBase>();
        private ProcedureBase _entryProcedure;

        public List<ProcedureBase> Procedures => _procedures;
        public ProcedureBase EntryProcedure => _entryProcedure;

        public ProcedureBase CurrProcedure
        {
            get
            {
                if (_procedureFsm == null)
                {
                    return null;
                }

                var procedure = (ProcedureBase)_procedureFsm.CurrState;
                return procedure;
            }
        }

        /**
         * 加载所有带有ProcedureAttribute标注的类，并启动IsEntry=true的类
         * 必须保证IsEntry=true的标注有且只有一个
         */
        public override void PostConstruct()
        {
            _fsmService = GetService<FsmService>();

            var tas = AppDomain.CurrentDomain.GetTypesHasAttribute<ProcedureAttribute>();
            foreach (var ta in tas)
            {
                var type = ta.Type;
                var pa = ta.Attribute;

                var ci = type.GetConstructor(Array.Empty<Type>());
                if (ci == null)
                {
                    Debug.LogError(
                        $"Constructor Procedure [{type.FullName}] Error, Default Constructor [{type.Name}()] Not Found");
                    continue;
                }

                var inst = ci.Invoke(Array.Empty<object>());

                if (inst == null || inst is ProcedureBase == false)
                {
                    Debug.LogError(
                        $"Constructor Procedure [{type.FullName}] Error, Type must extends ProcedureBase");
                    continue;
                }

                if (pa.LoadConditions.Length > 0)
                {
                    if (!GameFramework.Runtime.HaveEngineEnvs(pa.LoadConditions))
                    {
                        Debug.Log($"Skip Procedure [{type.FullName}]");
                        continue;
                    }
                }

                var pb = (ProcedureBase)inst;
                _procedures.Add(pb);

                if (pa.IsEntry)
                {
                    if (_entryProcedure == null)
                    {
                        _entryProcedure = pb;
                        Debug.Log($"Entry Procedure [{type.FullName}] Loaded");
                    }
                    else
                    {
                        Debug.Log($"Procedure [{type.FullName}] Loaded");
                    }
                }
            }

            if (_procedures.Count == 0)
            {
                Debug.LogError("GameFramework need one entry procedure at least");
                return;
            }

            var ps = _procedures.ToArray();

            _procedureFsm = ProcedureEntity.Create("GameProcedure", this, ps);
            _fsmService.AddFsm(_procedureFsm);
        }

        public override void ServiceReady()
        {
            if (_procedureFsm != null && _entryProcedure != null)
            {
                _procedureFsm.Start(_entryProcedure.GetType());
            }
        }

        public void StartProcedure<T>() where T : ProcedureBase
        {
            StartProcedure(typeof(T));
        }

        public void StartProcedure(Type procedureType)
        {
            _procedureFsm?.Start(procedureType);
        }
    }
}