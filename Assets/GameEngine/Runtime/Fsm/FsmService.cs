using System.Collections.Generic;
using System.Linq;
using GameEngine.Base;
using GameEngine.Base.Attributes;
using GameEngine.Base.Exception;
using GameEngine.Common;
using GameEngine.ObjectPool;

namespace GameEngine.Fsm
{
    [ServicePriority(EngineConsts.ServicePriorityValue.FsmService)]
    public class FsmService : BaseService
    {
        private readonly Dictionary<TypeNamePair, IFsmEntity> _fsms
            = new Dictionary<TypeNamePair, IFsmEntity>();

        public int Count => _fsms.Count;

        public override void OnUpdate(float elapseTime, float realElapseTime)
        {
            foreach (var fsm in _fsms.Values.ToArray())
            {
                fsm.Update(elapseTime, realElapseTime);
            }
        }

        /**
         * 检查是否存在指定的状态机
         * <typeparam name="T">owner type</typeparam>
         * <param name="fsmName">状态机名称</param>
         */
        public bool HasFsm<T>(string fsmName)
        {
            var p = new TypeNamePair(fsmName, typeof(T));
            return _fsms.ContainsKey(p);
        }

        public FsmEntity<T> CreateFsm<T>(string fsmName, T owner, params FsmState<T>[] states) where T : class
        {
            var p = new TypeNamePair(fsmName, typeof(T));
            if (HasFsm<T>(fsmName))
            {
                //这个状态机已经存在了
                throw new GameEngineException($"FSM [{p}] existed");
            }

            var fsm = FsmEntity<T>.Create(fsmName, owner, states);
            _fsms[p] = fsm;
            return fsm;
        }

        public void AddFsm<T>(FsmEntity<T> fsmEntity) where T : class
        {
            var p = new TypeNamePair(fsmEntity.Name, typeof(T));
            if (HasFsm<T>(fsmEntity.Name))
            {
                //这个状态机已经存在了
                throw new GameEngineException($"FSM [{p}] existed");
            }

            _fsms[p] = fsmEntity;
        }

        public FsmEntity<T> GetFsm<T>(string fsmName) where T : class
        {
            var p = new TypeNamePair(fsmName, typeof(T));
            _fsms.TryGetValue(p, out var fsm);
            return (FsmEntity<T>)fsm;
        }

        /**
         * 销毁FSM
         * <typeparam name="T">owner type</typeparam>
         * <param name="fsmName">状态机名称</param>
         */
        public bool DestroyFsm<T>(string fsmName) where T : class
        {
            var p = new TypeNamePair(fsmName, typeof(T));
            var fsm = GetFsm<T>(fsmName);
            if (fsm != null)
            {
                fsm.Shutdown();
                _fsms.Remove(p);
                return true;
            }

            return false;
        }
    }
}